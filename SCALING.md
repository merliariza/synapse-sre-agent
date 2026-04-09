# Scaling Strategy

## Current Architecture

SynapseSRE runs as three Docker containers orchestrated by Docker Compose:

- `synapse_api` — .NET 9 ASP.NET Core Web API
- `synapse_frontend` — Angular 18 served via nginx
- `synapse_db` — PostgreSQL 15

This setup handles the hackathon scope comfortably. Below is the scaling path for production.

---

## Horizontal Scaling — API Layer

The API is stateless by design. JWT tokens carry all auth state; no server-side sessions exist. This means the API can scale horizontally with zero code changes.

**From Docker Compose to production:**

```yaml
# Scale API replicas with Docker Swarm or Kubernetes
docker service scale synapsesre_api=5
```

With a load balancer (nginx, Traefik, or AWS ALB) in front, multiple API instances handle concurrent triage requests independently.

**Bottleneck:** Each `POST /incidents` triggers a synchronous AI API call (~5–15s). Under high load this blocks threads. The mitigation is a message queue (see below).

---

## Database Scaling

PostgreSQL scales vertically first (larger instance), then horizontally:

- **Read replicas** — `GET /incidents` queries route to replicas, writes go to primary
- **Connection pooling** — PgBouncer in front of PostgreSQL reduces connection overhead under concurrent load
- **Partitioning** — `incidents` table partitioned by `created_at` month for time-series query performance

---

## AI Triage — Async Queue

The current synchronous flow blocks the HTTP request during AI analysis. At scale, this becomes a problem.

**Target architecture:**

POST /incidents
│
▼
Queue (Redis Streams / RabbitMQ)
│
▼
Triage Worker (separate .NET Worker Service)
│
├──▶ AI API call
├──▶ Ticket creation
└──▶ Notification dispatch

This decouples ingestion from processing, allows multiple triage workers to run in parallel, and makes the system resilient to AI API slowdowns.

---

## Notification Layer

Current mock implementation logs to console. Production path:

| Channel | Service | Notes |
|---|---|---|
| Email | SendGrid / AWS SES | High deliverability, templates |
| Slack | Slack Webhooks API | Real webhook URL in `.env` |
| PagerDuty | PagerDuty Events API | For severity 4–5 incidents |

All three are drop-in replacements for the current `NotificationService` — the interface `INotificationService` is already abstracted.

---

## Observability at Scale

Current: Serilog console + OpenTelemetry console exporter.

Production path:
- **Logs** → Loki + Grafana
- **Traces** → Jaeger or Tempo
- **Metrics** → Prometheus + Grafana dashboards
- **Alerts** → AlertManager rules on `severity_score >= 4`

OpenTelemetry is already instrumented across all 5 pipeline stages — switching exporters requires one config change, no code changes.

---

## Assumptions

1. The AI provider (OpenRouter) is the primary external dependency — its rate limits cap concurrent triage throughput in the current synchronous model
2. PostgreSQL is sufficient up to ~10M incidents before sharding is needed
3. The frontend is static and can be served from a CDN (Cloudflare, CloudFront) with zero backend involvement
4. Docker Compose is the deployment target for this hackathon — the same `docker-compose.yml` translates to a Kubernetes manifest with `kompose convert`