# Agent Documentation — SynapseSRE

## Overview

SynapseSRE implements a single-agent architecture where an LLM-powered SRE agent (`OpenAIEdgeAgent`) orchestrates the full incident triage pipeline. The agent receives structured incident data, performs root-cause analysis, and produces actionable reports consumed by downstream automation.

---

## Agent Identity

| Property | Value |
|---|---|
| Agent name | SynapseSRE Triage Agent |
| Model | GPT-4o-mini via OpenRouter |
| Role | Senior SRE — observability and incident triage specialist |
| Input | Incident title + description + raw Docker/application logs |
| Output | Structured Markdown report (5 mandatory sections) |

---

## Use Cases

### UC-1: Automated incident triage
A user submits an incident report with a title, description, and log file. The agent analyzes the logs, identifies the root cause (CPU exhaustion, DB deadlock, OOM kill, network timeout, unhandled exception), scores severity from 1–5, and produces an actionable technical report in under 30 seconds.

### UC-2: On-call team notification
Upon completing triage, the agent triggers notifications to the engineering team via email and Slack with the severity score and AI summary embedded in the alert.

### UC-3: Reporter notification on resolution
When an incident is marked resolved via `PATCH /incidents/{id}/resolve`, the agent notifies the original reporter automatically, closing the communication loop without manual intervention.

---

## Implementation Details

### Agent class
`SynapseSRE.Infrastructure.Services.OpenAIEdgeAgent`

### System prompt design
The system prompt enforces:
- A fixed Markdown output schema (5 sections, no additions or omissions)
- Evidence-based analysis (findings must cite log excerpts)
- A severity score embedded as `[SEVERITY_VALUE: X]` for programmatic extraction
- Concise professional tone suitable for CTO-level briefing

### Severity extraction
The agent embeds a structured marker at the end of its response:
[SEVERITY_VALUE: 4]
The API extracts this with a regex, stores it as `TriageAnalysis.SeverityScore`, and strips the marker from the displayed report.

### Model configuration
```csharp
temperature: 0.2   // deterministic analysis, low hallucination risk
max_tokens: 1200   // sufficient for full 5-section report
model: gpt-4o-mini // cost-efficient, strong reasoning for log analysis
```

---

## Pipeline Stages — Observability Evidence

All 5 stages are instrumented with OpenTelemetry `ActivitySource` and Serilog structured logging.

| Stage | ActivitySource | Log tag | What is traced |
|---|---|---|---|
| Ingest | `SynapseSRE.Triage` / `stage.ingest` | `[INGEST]` | Title, has_log_file, guardrail result |
| Triage | `SynapseSRE.Triage` / `stage.triage` | `[TRIAGE]` | Model used, severity score, duration |
| Ticket | `SynapseSRE.Ticket` / `stage.ticket_created` | `[TICKET]` | Incident ID, DB persistence |
| Notify | `SynapseSRE.Notify` / `stage.notify_team` | `[NOTIFY]` | Channel, severity, recipients |
| Resolved | `SynapseSRE.Notify` / `stage.resolved_notify_reporter` | `[RESOLVED]` | Reporter notified, resolved by |

### Sample log output
```text
[00:39:16 INF] [INGEST]   Receiving incident: API Gateway timeout {"Service":"SynapseSRE.Api"}
[00:39:16 INF] [TRIAGE]   Sending to AI agent. Model: openai/gpt-4o-mini
[00:39:28 INF] [TRIAGE]   Analysis complete. Severity: 4/5 | IncidentId: 55f93c82
[00:39:28 INF] [TICKET]   Incident persisted. TicketId: 55f93c82
[00:39:28 INF] [NOTIFY]   Team alerted for incident 55f93c82
[00:46:41 INF] [RESOLVED] IncidentId=55f93c82 ResolvedBy=SRE Team ReporterNotified=True
```
---

## Safety Measures

### Input guardrails
Implemented in `IncidentController.SanitizeInput()`:

| Threat | Detection | Response |
|---|---|---|
| Prompt injection | Regex patterns: `ignore previous`, `act as`, `jailbreak`, `[INST]`, `system prompt`, etc. | HTTP 400, log warning with IP |
| Malicious file upload | Extension whitelist: `.log`, `.txt`, `.json` only | HTTP 400 |
| Oversized files | 5 MB max file size | HTTP 400 |
| Log token flooding | Log content truncated at 8000 chars before sending to LLM | Silent truncation with marker |

### Output guardrails
- The system prompt instructs the model to never fabricate log data
- The system prompt forbids suggesting infrastructure reboots as a first action
- Fixed output schema prevents the model from adding unexpected sections
- Severity score is extracted programmatically — not parsed from free text

### Infrastructure security
- API runs as non-root user (`appuser` UID 1001) inside the container
- No secrets in source code — all credentials via environment variables
- `appsettings.Development.json` excluded from Docker image and `.gitignore`
- JWT with zero clock skew tolerance

---

## Known Limitations

- GitHub Issues integration is configured but requires a valid `GITHUB_TOKEN` 
  with `repo` scope — falls back to mock mode if not set
- SMTP email requires valid credentials — falls back to console mock if not configured

See [SCALING.md](SCALING.md) for the production architecture that addresses these limitations.