using System.Diagnostics;
using System.Text.RegularExpressions;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SynapseSRE.Api.Models;
using SynapseSRE.Application.DTOs;
using SynapseSRE.Domain.Entities;
using SynapseSRE.Domain.Interfaces;

namespace SynapseSRE.Api.Controllers;

public class IncidentController : BaseApiController
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IAgentService _aiAgent;
    private readonly INotificationService _notificationService;
    private readonly IConfiguration _config;
    private readonly ILogger<IncidentController> _logger;

    private static readonly ActivitySource TriageSource = new("SynapseSRE.Triage");
    private static readonly ActivitySource TicketSource  = new("SynapseSRE.Ticket");
    private static readonly ActivitySource NotifySource  = new("SynapseSRE.Notify");

    public IncidentController(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IAgentService aiAgent,
        INotificationService notificationService,
        IConfiguration config,
        ILogger<IncidentController> logger)
    {
        _unitOfWork          = unitOfWork;
        _mapper              = mapper;
        _aiAgent             = aiAgent;
        _notificationService = notificationService;
        _config              = config;
        _logger              = logger;
    }

    // ── GET /api/incidents ────────────────────────────────────────────────────
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<IncidentDto>>> Get()
    {
        var incidents = await _unitOfWork.Incidents.GetAllAsync();
        return Ok(_mapper.Map<IEnumerable<IncidentDto>>(incidents));
    }

    // ── GET /api/incidents/{id} ───────────────────────────────────────────────
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IncidentDto>> Get(Guid id)
    {
        var incident = await _unitOfWork.Incidents.GetByIdAsync(id);
        if (incident == null) return NotFound("El incidente no existe.");
        return Ok(_mapper.Map<IncidentDto>(incident));
    }

    // ── POST /api/incidents ───────────────────────────────────────────────────
    [HttpPost]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IncidentDto>> Post([FromForm] IncidentCreateRequest request)
    {
        using var ingestActivity = TriageSource.StartActivity("stage.ingest");

        _logger.LogInformation("[INGEST] Receiving incident: {Title}", request.Title);

        var sanitizedTitle       = SanitizeInput(request.Title);
        var sanitizedDescription = SanitizeInput(request.Description);

        if (sanitizedTitle == "__BLOCKED__" || sanitizedDescription == "__BLOCKED__")
        {
            _logger.LogWarning("[GUARDRAIL] Prompt injection attempt blocked. IP: {IP}",
                HttpContext.Connection.RemoteIpAddress);
            return BadRequest(new { error = "Input contiene patrones no permitidos.", code = "INJECTION_BLOCKED" });
        }

        if (request.LogFile != null)
        {
            var allowedExtensions = new[] { ".log", ".txt", ".json" };
            var ext = Path.GetExtension(request.LogFile.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(ext))
                return BadRequest(new { error = $"Tipo de archivo no permitido. Solo: {string.Join(", ", allowedExtensions)}" });

            if (request.LogFile.Length > 5 * 1024 * 1024)
                return BadRequest(new { error = "El archivo no puede superar 5 MB." });
        }

        string logContent = "No log provided";
        if (request.LogFile != null)
        {
            using var reader = new StreamReader(request.LogFile.OpenReadStream());
            logContent = await reader.ReadToEndAsync();

            if (logContent.Length > 8000)
                logContent = logContent[..8000] + "\n[LOG TRUNCATED FOR SAFETY — max 8000 chars]";
        }

        ingestActivity?.SetTag("incident.title",       sanitizedTitle);
        ingestActivity?.SetTag("incident.has_log_file", request.LogFile != null);

        using var triageActivity = TriageSource.StartActivity("stage.triage");

        _logger.LogInformation("[TRIAGE] Sending to AI agent. Model: {Model}",
            _config["AI:ModelId"]);

        var incident = new Incident
        {
            Id          = Guid.NewGuid(),
            Title       = sanitizedTitle,
            Description = sanitizedDescription,
            Status      = "Pending",
            CreatedAt   = DateTime.UtcNow
        };

        var aiAnalysis = await _aiAgent.AnalyzeIncidentAsync(
            sanitizedTitle, sanitizedDescription, logContent);

        int finalScore = 3;
        var match = Regex.Match(aiAnalysis, @"\[SEVERITY_VALUE:\s*(\d)\]");
        if (match.Success)
        {
            int.TryParse(match.Groups[1].Value, out finalScore);
            aiAnalysis = aiAnalysis.Replace(match.Value, "").Trim();
        }

        var triage = new TriageAnalysis
        {
            Id            = Guid.NewGuid(),
            IncidentId    = incident.Id,
            AiSummary     = aiAnalysis,
            SeverityScore = finalScore,
            SuggestedFix  = "Análisis automático generado por Agente SRE.",
            ModelUsed     = _config["AI:ModelId"] ?? "gpt-4o-mini",
            ProcessedAt   = DateTime.UtcNow
        };

        triageActivity?.SetTag("triage.severity_score", finalScore);
        triageActivity?.SetTag("triage.model",          triage.ModelUsed);

        _logger.LogInformation("[TRIAGE] Analysis complete. Severity: {Score}/5 | IncidentId: {Id}",
            finalScore, incident.Id);

        // ── TICKET ───────────────────────────────────────────────────
        using var ticketActivity = TicketSource.StartActivity("stage.ticket_created");

        _unitOfWork.Incidents.Add(incident);
        _unitOfWork.TriageAnalyses.Add(triage);
        _unitOfWork.Logs.Add(new ActivityLog
        {
            Action    = "IncidentCreated",
            Details   = $"AI triage completed | IncidentId: {incident.Id} | Score: {finalScore}",
            CreatedAt = DateTime.UtcNow
        });

        await _unitOfWork.SaveAsync();

        ticketActivity?.SetTag("ticket.incident_id", incident.Id.ToString());
        ticketActivity?.SetTag("ticket.status",      "created");

        _logger.LogInformation("[TICKET] Incident persisted. TicketId: {Id}", incident.Id);

        // ── NOTIFY TEAM ──────────────────────────────────────────────
        using var notifyActivity = NotifySource.StartActivity("stage.notify_team");

        await _notificationService.SendTeamAlertAsync(
            incident.Title,
            finalScore,
            incident.Id,
            aiAnalysis
        );

        notifyActivity?.SetTag("notify.channel", "email+slack");
        notifyActivity?.SetTag("notify.severity", finalScore);

        _logger.LogInformation("[NOTIFY] Team alerted for incident {Id}", incident.Id);

        var resultDto = _mapper.Map<IncidentDto>(incident);
        resultDto.TriageAnalysis = _mapper.Map<TriageAnalysisDto>(triage);

        return CreatedAtAction(nameof(Get), new { id = incident.Id }, resultDto);
    }

    // ── PATCH /api/incidents/{id}/resolve ─────────────────────────────────────
    [HttpPatch("{id}/resolve")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Resolve(Guid id, [FromBody] ResolveIncidentRequest request)
    {
        var incident = await _unitOfWork.Incidents.GetByIdAsync(id);
        if (incident == null) return NotFound("Incidente no encontrado.");

        // ── NOTIFY REPORTER ──────────────────────────────
        using var resolveActivity = NotifySource.StartActivity("stage.resolved_notify_reporter");

        incident.Status     = "Resolved";
        incident.ResolvedAt = DateTime.UtcNow;

        _unitOfWork.Incidents.Update(incident);
        _unitOfWork.Logs.Add(new ActivityLog
        {
            Action    = "IncidentResolved",
            Details   = $"ResolvedBy: {request.ResolvedBy} | Notes: {request.ResolutionNotes}",
            CreatedAt = DateTime.UtcNow
        });

        await _unitOfWork.SaveAsync();

        if (!string.IsNullOrEmpty(request.ReporterEmail))
        {
            await _notificationService.SendReporterNotificationAsync(
                request.ReporterEmail,
                incident.Title,
                incident.Id
            );
        }

        resolveActivity?.SetTag("resolve.incident_id",   id.ToString());
        resolveActivity?.SetTag("resolve.reporter_email", request.ReporterEmail ?? "none");

        _logger.LogInformation(
            "[RESOLVED] IncidentId={Id} ResolvedBy={ResolvedBy} ReporterNotified={Notified}",
            id, request.ResolvedBy, !string.IsNullOrEmpty(request.ReporterEmail));

        return Ok(new
        {
            message     = "Incidente resuelto. Reporter notificado.",
            incidentId  = id,
            resolvedAt  = incident.ResolvedAt,
            resolvedBy  = request.ResolvedBy
        });
    }

    // ── PUT /api/incidents/{id} ───────────────────────────────────────────────
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Put(Guid id, [FromBody] IncidentDto dto)
    {
        var existing = await _unitOfWork.Incidents.GetByIdAsync(id);
        if (existing == null) return NotFound("Incidente no encontrado.");

        _mapper.Map(dto, existing);
        _unitOfWork.Incidents.Update(existing);
        await _unitOfWork.SaveAsync();

        _logger.LogInformation("[INCIDENT_UPDATED] IncidentId={Id}", id);
        return Ok(dto);
    }

    // ── DELETE /api/incidents/{id} ────────────────────────────────────────────
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var incident = await _unitOfWork.Incidents.GetByIdAsync(id);
        if (incident == null) return NotFound();

        _unitOfWork.Incidents.Remove(incident);
        await _unitOfWork.SaveAsync();

        _logger.LogInformation("[INCIDENT_DELETED] IncidentId={Id}", id);
        return NoContent();
    }

    // ── Guardrail helper ──────────────────────────────────────────────────────
    private static string SanitizeInput(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;

        var injectionPatterns = new[]
        {
            @"ignore\s+(previous|all|above)",
            @"forget\s+(everything|instructions)",
            @"you\s+are\s+now",
            @"act\s+as\s+",
            @"system\s*prompt",
            @"jailbreak",
            @"<\|.*?\|>",
            @"\[INST\]",
            @"###\s*instruction",
            @"disregard\s+(all|previous)",
            @"override\s+(your|the)\s+(instructions|rules)",
        };

        foreach (var pattern in injectionPatterns)
        {
            if (Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase))
                return "__BLOCKED__";
        }

        return input.Length > 2000 ? input[..2000] : input;
    }
}