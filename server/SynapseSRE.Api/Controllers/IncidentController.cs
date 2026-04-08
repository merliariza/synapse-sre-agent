using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SynapseSRE.Application.DTOs;
using SynapseSRE.Domain.Entities;
using SynapseSRE.Domain.Interfaces;
using SynapseSRE.Api.Models; 
using Microsoft.AspNetCore.Http; 

namespace SynapseSRE.Api.Controllers;

public class IncidentController : BaseApiController
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IAgentService _aiAgent;
    private readonly INotificationService _notificationService; 

    public IncidentController(
        IUnitOfWork unitOfWork, 
        IMapper mapper, 
        IAgentService aiAgent,
        INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _aiAgent = aiAgent;
        _notificationService = notificationService;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<IncidentDto>>> Get()
    {
        var incidents = await _unitOfWork.Incidents.GetAllAsync();
        return Ok(_mapper.Map<IEnumerable<IncidentDto>>(incidents));
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IncidentDto>> Get(Guid id)
    {
        var incident = await _unitOfWork.Incidents.GetByIdAsync(id);
        if (incident == null) return NotFound("El incidente no existe.");
        return Ok(_mapper.Map<IncidentDto>(incident));
    }

    [HttpPost]
    [Consumes("multipart/form-data")] 
    public async Task<ActionResult<IncidentDto>> Post([FromForm] IncidentCreateRequest request)
    {
        var incident = new Incident 
        { 
            Id = Guid.NewGuid(),
            Title = request.Title, 
            Description = request.Description,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };
        
        string logContent = "No log provided";
        if (request.LogFile != null)
        {
            using var reader = new StreamReader(request.LogFile.OpenReadStream());
            logContent = await reader.ReadToEndAsync();
        }

        var aiAnalysis = await _aiAgent.AnalyzeIncidentAsync(incident.Title, incident.Description, logContent);

        var triage = new TriageAnalysis
        {
            Id = Guid.NewGuid(),
            IncidentId = incident.Id,
            AiSummary = aiAnalysis,
            SeverityScore = aiAnalysis.Contains("Critical") || aiAnalysis.Contains("High") ? 5 : 3,
            SuggestedFix = "Análisis automático generado por Agente SRE.",
            ModelUsed = "gpt-4o-mini", 
            ProcessedAt = DateTime.UtcNow
        };

        _unitOfWork.Incidents.Add(incident);
        _unitOfWork.TriageAnalyses.Add(triage);
        
        _unitOfWork.Logs.Add(new ActivityLog { 
            Action = "IncidentCreated", 
            Details = $"AI triage completed for: {incident.Id}" 
        });

        await _unitOfWork.SaveAsync();

        _notificationService.SendAlert($"🚨 Incidente: {incident.Title} | Severidad: {triage.SeverityScore}/5");

        var resultDto = _mapper.Map<IncidentDto>(incident);
        return CreatedAtAction(nameof(Get), new { id = incident.Id }, resultDto);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Put(Guid id, [FromBody] IncidentDto dto)
    {
        var existing = await _unitOfWork.Incidents.GetByIdAsync(id);
        if (existing == null) return NotFound("Inicidente no encontrado.");

        _mapper.Map(dto, existing); 
        _unitOfWork.Incidents.Update(existing);
        await _unitOfWork.SaveAsync();
        return Ok(dto);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var incident = await _unitOfWork.Incidents.GetByIdAsync(id);
        if (incident == null) return NotFound();

        _unitOfWork.Incidents.Remove(incident);
        await _unitOfWork.SaveAsync();
        return NoContent();
    }
}