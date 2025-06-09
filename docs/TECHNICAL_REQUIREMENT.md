# Flight Price Monitor – Technical Requirements  
*(Updated 2025-06-09)*  

> This document translates the PRD into an implementable, technology-specific specification.  
> **Key deltas in this revision**  
> • Azure is the single cloud platform  
> • Front-end stack fixed to React + TypeScript (Next.js 14, App Router)  
> • Back-end fixed to C# 9 (compiled with LangVersion=9)  
> • DevOps pipelines hosted in GitHub  
> • Playwright mandated for end-to-end & integration tests  
> • Authentication/Authorization via Microsoft Entra ID  

---

## 1. High-Level Architecture (Azure Reference)

```
 ┌────────────┐  HTTPS  ┌─────────────────────────┐
 │  Browser   │ ─────── ►  Next.js 14 (Vercel OR │
 └────────────┘         │  Azure Static Web Apps) │
                        └────────▲────────────────┘
                                 │ GraphQL/REST
                                 ▼
                     ┌─────────────────────────────┐
                     │ ASP.NET Core API (.NET 8,   │
                     │ C# 9) – Azure App Service   │
                     └──────────┬──────────────────┘
                                │ CQRS / MediatR
   ┌─────────────────┐  Azure   │
   │ Ingestion Jobs  │ Functions│
   │   (.NET 8)      │ <Timer>  │
   └────────┬────────┘          ▼
            │            ┌──────────────────────────┐
            │            │ Azure Database for       │
            │            │ PostgreSQL (Flexible) +  │
            ▼            │ TimescaleDB extension    │
   ┌─────────────────┐   └──────────────────────────┘
   │ Azure Storage   │    ▲           ▲
   │  (Blob – raw)   │    │           │
   └─────────────────┘    │           │
                      ┌───┴────┐ ┌────┴────────┐
                      │ Redis  │ │  Azure KV   │
                      │ Cache  │ │  (Secrets)  │
                      └────────┘ └─────────────┘
```

---

## 2. Back-End (C# 9 / .NET 8)

| Area | Requirement | Tech / Azure Service |
|------|-------------|----------------------|
| Language | C# 9 (`<LangVersion>9.0</LangVersion>`) |  |
| Runtime | .NET 8 LTS | Azure App Service (Linux) |
| Web Framework | ASP.NET Core Minimal APIs (+ controllers where needed) |  |
| API Layer | GraphQL (HotChocolate) + REST |  |
| Data Access | EF Core 8 for writes, Dapper for high-perf reads |  |
| Time-Series | TimescaleDB on Azure Database for PostgreSQL |  |
| Background Jobs | Azure Functions (Timer Trigger) + Durable Functions for orchestration |  |
| External Flight APIs | Pluggable `IFlightProvider` implementations (Kiwi/Tequila, Amadeus…) via `HttpClientFactory` |  |
| Dependency Injection | Built-in .NET DI |  |
| Messaging | In-process CQRS with MediatR; optional Azure Service Bus if cross-app needed |  |
| Caching | Azure Cache for Redis (5-min TTL for duplicate searches) |  |
| AuthN | Microsoft.Identity.Web → Entra ID (OIDC) access tokens |  |
| AuthZ | Role-based policies (“Visitor”, “User”, “Admin”) + Entra ID groups |  |
| Validation | FluentValidation + ProblemDetails |  |
| Rate Limiting | `AspNetCoreRateLimit` (60 unauth, 600 auth req/h) |  |
| Observability | OpenTelemetry → Azure Monitor / Application Insights |  |
| Logging | Serilog → Azure Log Analytics |  |
| Tests | • Unit: xUnit + AutoFixture + NSubstitute<br>• Integration: Microsoft.AspNetCore.Mvc.Testing + Playwright (API fixtures) |  |

### Key Endpoints (v1)

| Verb | Path | Summary |
|------|------|---------|
| GET | /api/v1/flights/search | Live fares |
| GET | /api/v1/flights/history | Snapshot or full series |
| POST | /api/v1/alerts | Create price alert |
| GET | /api/v1/admin/ingestions | Job health (admin) |

---

## 3. Front-End (React + TypeScript, Next.js 14)

| Concern | Choice / Library |
|---------|------------------|
| Framework | Next.js 14 (App Router, React 18) |
| Language | TypeScript 5 |
| Styling | Tailwind CSS 4 + shadcn/ui |
| State & Data | TanStack Query (React Query) + GraphQL (Apollo or urql) |
| Forms | React Hook Form + Zod |
| Charts | Recharts (or Nivo for advanced) |
| Auth | next-auth w/ Entra ID provider (`@next-auth/azure-ad`) |
| Payments | Stripe.js + React-Stripe-JS (Pro tier) |
| PWA | Next-PWA plugin (phase 2) |
| Testing | **Playwright**:<br>• Component testing with `@playwright/experimental-ct-react`<br>• E2E flows (search, history, auth) |

SSR/ISR via Next.js enables SEO on search URLs; deployment target is Azure Static Web Apps or Vercel (bring-your-own Azure resources still apply).

---

## 4. Database & Schema (TimescaleDB on Azure)

```sql
-- Hypertable for historical prices
CREATE TABLE price_snapshot (
  id                 BIGSERIAL,
  query_id           UUID            NOT NULL REFERENCES flight_query(id),
  airline_code       CHAR(2)         NOT NULL,
  cabin              VARCHAR(10)     NOT NULL,
  price_cents        INT             NOT NULL,
  currency           CHAR(3)         NOT NULL,
  deep_link          TEXT,
  collected_at       TIMESTAMPTZ     NOT NULL,
  PRIMARY KEY (query_id, airline_code, cabin, collected_at)
);
SELECT create_hypertable('price_snapshot', 'collected_at');
```

Backups: point-in-time restore enabled on Flexible Server; snapshots to Azure Blob.

---

## 5. DevOps & CI/CD (GitHub → Azure)

| Stage | Tooling / Service |
|-------|-------------------|
| Version Control | GitHub (mono-repo) |
| CI | **GitHub Actions**: build .NET, run xUnit & Playwright, lint, Docker build & scan |
| Container Registry | GitHub Container Registry (ghcr.io) |
| CD | GitHub Actions → `az` CLI → <br>• Azure App Service (API)<br>• Azure Functions (jobs)<br>• Azure Static Web Apps (Next.js) |
| IaC | Bicep (preferred) or Terraform; environment per branch using GitHub Environments |
| Secrets | GitHub OIDC federated creds → Azure Key Vault |
| Release Strategy | Blue-green (App Service deployment slots) |
| Observability | Dashboards & alerts in Azure Monitor; GitHub Actions annotations on failing SLOs |

---

## 6. Testing Strategy (All Layers)

| Layer | Framework | Notes |
|-------|-----------|-------|
| Domain/Unit | xUnit + AutoFixture | 80 % coverage gate |
| API Contract | Playwright API tests hitting `WebApplicationFactory` | Run in CI |
| Integration (DB, Cache) | Testcontainers-dotnet spawns Postgres + Redis | Parallel matrix |
| E2E (Browser) | **Playwright**: <br>Chrome + WebKit; mock flight APIs | Record HAR for regressions |
| Performance | k6 scripts in GitHub Actions nightly; thresholds feed SLOs |
| Security | Bandit (container scan), dotnet-securify, npm-audit | Blocking on Critical |

---

## 7. Security & Compliance

• TLS 1.3 with Azure Front Door WAF  
• RBAC via Entra ID groups; least-privileged managed identities for Functions/App Service  
• GDPR: Data export & delete endpoints; data encrypted at rest (Postgres storage encryption)  
• Secrets never in code; pulled at runtime from Key Vault via managed identity  
• Dependabot for npm & NuGet; automatic PRs + required review  

---

## 8. Performance & Scalability NFRs

| Metric | Target |
|--------|--------|
| p95 search latency | ≤ 800 ms |
| p95 history lookup | ≤ 1 s |
| Ingestion throughput | 50 k price rows/hour |
| API cold start | ≤ 2 s (App Service with pre-warmed instance) |

---

## 9. Monitoring & Alert Rules (Azure)

| Signal | Alert When | Action Group |
|--------|------------|--------------|
| API p95 > 1 s | 5 min sustained | PagerDuty + Teams |
| Job failures > 2 | within 1 h | Email dev-on-call |
| DB CPU > 70 % | 10 min | Scale Postgres tier hint |

---

## 10. Open Questions

1. Which ingestion frequency vs. API quota (cost) is acceptable?  
2. Do we need multi-region redundancy Day-1?  
3. Stripe tax & VAT handling for EU?  

Once clarified, sprint planning can begin.
