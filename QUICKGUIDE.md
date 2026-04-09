# Quick Start Guide

Get SynapseSRE running in under 5 minutes.

## Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) installed and running
- An OpenRouter API key — free at [openrouter.ai/keys](https://openrouter.ai/keys)

---

## Steps

### 1. Clone the repository

```bash
git clone https://github.com/merliariza/synapse-sre-agent.git
cd SynapseSRE-Project
```

### 2. Configure environment

```bash
cp .env.example .env
```

Open `.env` and set your API key:

```dotenv
AI__ProviderKey=sk-or-v1-your-key-here
```

Everything else works out of the box. Notification variables can stay empty — the app runs in mock mode and prints alerts to the console.

### 3. Start the stack

```bash
docker compose up --build
```

First build takes 3–5 minutes (downloads .NET SDK and Node images). Subsequent builds are fast due to layer caching.

### 4. Verify it's running

| Service | URL |
|---|---|
| Frontend | http://localhost:4200 |
| API Swagger | http://localhost:5210/swagger |
| API Health | http://localhost:5210/api/incidents |

---

## Test the Full E2E Flow

### Step 1 — Register and login

```bash
# Register
curl -X POST http://localhost:5210/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"username":"sreteam","email":"team@test.com","password":"Test1234!"}'

# Login — copy the token from the response
curl -X POST http://localhost:5210/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"sreteam","password":"Test1234!"}'
```

### Step 2 — Submit an incident

Create a test log file:

```bash
echo "2026-04-09T00:01:23Z ERROR Gateway timeout after 30000ms
2026-04-09T00:01:23Z ERROR upstream connect error: connection refused
2026-04-09T00:01:24Z WARN  retry attempt 3/3 failed" > test.log
```

Submit the incident:

```bash
curl -X POST http://localhost:5210/api/incidents \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -F "Title=API Gateway timeout on checkout" \
  -F "Description=Users cannot complete purchases, gateway returns 504" \
  -F "LogFile=@test.log"
```

Expected response: `201 Created` with `triageAnalysis.aiSummary` containing the full AI report.

### Step 3 — Resolve the incident

```bash
curl -X PATCH http://localhost:5210/api/incidents/INCIDENT_ID/resolve \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "resolvedBy": "SRE Team",
    "resolutionNotes": "Restarted connection pool",
    "reporterEmail": "reporter@test.com"
  }'
```

### Step 4 — Watch the logs

```bash
docker logs synapse_api -f
```

You should see all 5 pipeline stages:

[INGEST]   Receiving incident...
[TRIAGE]   Analysis complete. Severity: 4/5
[TICKET]   Incident persisted
[NOTIFY]   Team alerted
[RESOLVED] ReporterNotified=True

---

## Stopping the stack

```bash
docker compose down
# To also remove the database volume:
docker compose down -v
```