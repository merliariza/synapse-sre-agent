# SynapseSRE — Intelligent Incident Triage Agent

> Automated SRE incident intake, AI-powered triage, and end-to-end notification system for e-commerce platforms.

[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![Docker](https://img.shields.io/badge/Docker-Compose-blue.svg)](docker-compose.yml)
[![.NET](https://img.shields.io/badge/.NET-9.0-purple.svg)](server/)
[![Angular](https://img.shields.io/badge/Angular-18-red.svg)](client/)

---

## What is SynapseSRE?

SynapseSRE is an AI-powered SRE agent that automates the full incident lifecycle for e-commerce applications:

1. **Ingest** — accepts multimodal incident reports (text + log files) via UI or API
2. **Triage** — a GPT-4o-mini agent analyzes logs, identifies root cause, and scores severity (1–5)
3. **Ticket** — automatically creates a structured ticket with the AI analysis attached
4. **Notify** — alerts the engineering team via email and Slack
5. **Resolve** — when the ticket is closed, notifies the original reporter automatically

### Tech Stack

| Layer | Technology |
|---|---|
| Frontend | Angular 18, TypeScript |
| Backend | .NET 9, ASP.NET Core, C# |
| Database | PostgreSQL 15 |
| AI Agent | OpenRouter → GPT-4o-mini |
| ORM | Entity Framework Core 9 |
| Auth | JWT Bearer tokens |
| Observability | Serilog + OpenTelemetry |
| Container | Docker, Docker Compose |

---

## Project Structure

```text
SynapseSRE-Project/
├── client/
│   └── SynapseSRE.Web/          # Angular 18 frontend
├── server/
│   ├── SynapseSRE.Api/          # ASP.NET Core Web API
│   ├── SynapseSRE.Application/  # DTOs, interfaces, use cases
│   ├── SynapseSRE.Domain/       # Entities, domain interfaces
│   └── SynapseSRE.Infrastructure/ # EF Core, repositories
├── docker-compose.yml
├── .env.example
└── README.md
```

## Quick Start

```bash
git clone https://github.com/merliariza/synapse-sre-agent.git
cd SynapseSRE-Project
cp .env.example .env
# Edit .env and set your AI__ProviderKey from https://openrouter.ai/keys
docker compose up --build
```

- API + Swagger: `http://localhost:5210/swagger`
- Frontend: `http://localhost:4200`

See [QUICKGUIDE.md](QUICKGUIDE.md) for full step-by-step instructions.

---

## E2E Flow
```text
User submits incident (title + description + log file)
│
▼
[INGEST] Input validation + guardrails (anti prompt-injection)
│
▼
[TRIAGE] AI Agent analyzes logs → Executive Summary + Root Cause + Severity Score
│
▼
[TICKET] Incident persisted to PostgreSQL with full triage analysis
│
▼
[NOTIFY] Team alerted via Email + Slack (mock or real via SMTP/webhook)
│
▼
PATCH /incidents/{id}/resolve
│
▼
[RESOLVED] Original reporter notified — loop closed
```
---

## Key Features

- **Multimodal input** — text description + `.log` / `.txt` / `.json` file upload
- **AI triage** — structured Markdown report: Executive Summary, Log Analysis, Severity Score (1–5), Mitigation Steps, Monitoring Metric
- **Guardrails** — prompt injection detection, file type validation, log truncation at 8000 chars
- **Observability** — Serilog structured logs + OpenTelemetry traces covering all 5 pipeline stages
- **Notifications** — email (SMTP or mock) + Slack webhook (or mock) — fully demoable without real credentials
- **Docker Compose** — entire stack runs with a single command

---

## API Endpoints

| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/auth/register` | Register new user |
| POST | `/api/auth/login` | Login, returns JWT |
| POST | `/api/incidents` | Submit incident (multipart) |
| GET | `/api/incidents` | List all incidents |
| GET | `/api/incidents/{id}` | Get incident by ID |
| PATCH | `/api/incidents/{id}/resolve` | Resolve + notify reporter |
| GET | `/api/triage` | List all triage analyses |
| GET | `/api/activitylog` | Observability log entries |

---

## Environment Variables

See [.env.example](.env.example) for all required variables with descriptions.

The only required variable to run the AI agent is `AI__ProviderKey` — get a free key at [openrouter.ai/keys](https://openrouter.ai/keys).

All notification variables (`SMTP_HOST`, `SLACK_WEBHOOK_URL`) are optional — leaving them empty enables mock mode, which logs notifications to the console and is fully demoable.

---

## License

MIT — see [LICENSE](LICENSE)