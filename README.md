# 🔗 DistributedShortener

> A production-grade URL shortener built with .NET 9, designed to practice and demonstrate distributed systems architecture patterns: CQRS, Event Sourcing, cache-aside, and async event processing.

---

## 📌 Overview

DistributedShortener is a learning-focused backend project that solves a deceptively simple problem — shortening URLs — using the same architectural patterns found in high-traffic production systems.

The core challenge: a URL redirect must resolve in **under 5ms** even under heavy load, while every click must also be **tracked asynchronously** without slowing down the redirect. This tension naturally introduces:

- **CQRS** to separate write operations (create link) from read operations (resolve link)
- **Redis** as a cache layer to serve redirects at memory speed
- **AWS SQS** to decouple click tracking from the hot path
- **Event Sourcing** on the write side to maintain a full audit log of all link operations

---

## 🏗️ Architecture

```
┌──────────────┐     POST /links      ┌─────────────────────┐
│              │ ──────────────────▶  │   Command Handler   │
│    Client    │                      │  (Create Short Link) │
│              │                      └────────┬────────────┘
│              │                               │ Store event
│              │                               ▼
│              │                      ┌─────────────────────┐
│              │                      │   PostgreSQL        │
│              │                      │   (Event Store)     │
│              │                      └─────────────────────┘
│              │
│              │   GET /{code}        ┌─────────────────────┐
│              │ ──────────────────▶  │   Query Handler     │
│              │                      │  (Resolve Redirect) │
│              │                      └────────┬────────────┘
│              │                               │ Cache lookup
│              │                               ▼
│              │                      ┌─────────────────────┐
│              │                      │   Redis Cache       │◀── Cache miss → PostgreSQL
│              │                      └────────┬────────────┘
└──────────────┘                               │ Fire & forget
                                               ▼
                                      ┌─────────────────────┐     ┌──────────────────┐
                                      │   AWS SQS Queue     │────▶│ Analytics Worker │
                                      │   (click events)    │     │  (background)    │
                                      └─────────────────────┘     └──────────────────┘
```

### C4 – Container Level

| Container | Technology | Responsibility |
|---|---|---|
| **API** | .NET 9 Minimal API | Exposes HTTP endpoints, routes commands/queries |
| **Command Handler** | MediatR | Creates short links, publishes domain events |
| **Query Handler** | MediatR | Resolves short codes, applies cache-aside |
| **Event Store** | PostgreSQL | Persists all link events (source of truth) |
| **Read Cache** | Redis | Serves redirects at sub-millisecond latency |
| **Message Queue** | AWS SQS | Buffers click events for async processing |
| **Analytics Worker** | .NET 9 Worker Service | Consumes click events, writes analytics data |

---

## 🧠 Architectural Patterns

### CQRS (Command Query Responsibility Segregation)

Commands and queries use separate models, handlers, and data paths. The write side validates, stores an event, and invalidates cache. The read side consults cache first and falls back to the database only on a miss.

```
Commands ──▶ CommandHandlers ──▶ PostgreSQL (write)
Queries  ──▶ QueryHandlers   ──▶ Redis → PostgreSQL (read)
```

### Event Sourcing (Write Side)

Instead of storing the current state of a link directly, we store the sequence of events that produced that state. This means every creation, update, expiration, or deletion is recorded as an immutable fact.

```
LinkCreatedEvent    { code, originalUrl, createdAt, createdBy }
LinkDisabledEvent   { code, disabledAt, reason }
LinkExpiredEvent    { code, expiredAt }
```

The current state of any link is derived by replaying its event stream.

### Cache-Aside (Read Side)

On every redirect request:

1. Check Redis for the short code key
2. **Hit** → return the URL immediately (< 1ms)
3. **Miss** → query PostgreSQL, populate cache with TTL, return the URL
4. Fire a `ClickRegisteredEvent` onto SQS (non-blocking)

### Async Analytics (Fire & Forget)

Click tracking is completely decoupled from the redirect response. The API publishes a lightweight message to SQS and immediately returns the redirect. A background Worker Service consumes the queue at its own pace, enriching click data (geo, device, referrer) and persisting it to the analytics table.

---

## 📁 Project Structure

```
DistributedShortener/
│
├── src/
│   ├── DistributedShortener.Api/              # .NET 9 Minimal API + Carter modules
│   │   ├── Endpoints/
│   │   │   ├── LinksModule.cs                 # POST /links
│   │   │   └── RedirectModule.cs              # GET /{code}
│   │   ├── Program.cs
│   │   └── appsettings.json
│   │
│   ├── DistributedShortener.Application/      # Use cases (CQRS with MediatR)
│   │   ├── Commands/
│   │   │   ├── CreateShortLink/
│   │   │   │   ├── CreateShortLinkCommand.cs
│   │   │   │   └── CreateShortLinkHandler.cs
│   │   ├── Queries/
│   │   │   ├── ResolveShortCode/
│   │   │   │   ├── ResolveShortCodeQuery.cs
│   │   │   │   └── ResolveShortCodeHandler.cs
│   │   └── Abstractions/
│   │       ├── ILinkRepository.cs
│   │       └── IEventPublisher.cs
│   │
│   ├── DistributedShortener.Domain/           # Core domain model
│   │   ├── Aggregates/
│   │   │   └── ShortLink.cs
│   │   ├── Events/
│   │   │   ├── LinkCreatedEvent.cs
│   │   │   ├── LinkDisabledEvent.cs
│   │   │   └── LinkExpiredEvent.cs
│   │   └── ValueObjects/
│   │       ├── ShortCode.cs
│   │       └── OriginalUrl.cs
│   │
│   ├── DistributedShortener.Infrastructure/   # External concerns
│   │   ├── Persistence/
│   │   │   ├── AppDbContext.cs                # EF Core
│   │   │   └── LinkRepository.cs
│   │   ├── Cache/
│   │   │   └── RedisCacheService.cs
│   │   └── Messaging/
│   │       └── SqsEventPublisher.cs
│   │
│   └── DistributedShortener.Worker/           # Background analytics consumer
│       ├── ClickEventConsumer.cs
│       └── Program.cs
│
├── tests/
│   ├── DistributedShortener.Domain.Tests/     # Pure domain unit tests
│   ├── DistributedShortener.Application.Tests/ # Handler tests with mocks
│   └── DistributedShortener.Integration.Tests/ # Testcontainers (Redis + Postgres)
│
├── docker-compose.yml                         # Local Redis + PostgreSQL
├── .gitignore
└── README.md
```

---

## 🛠️ Tech Stack

| Layer | Technology | Version |
|---|---|---|
| Runtime | .NET | 9.0 |
| Web Framework | Minimal API + Carter | Latest |
| CQRS / Mediator | MediatR | 12.x |
| ORM | Entity Framework Core | 9.x |
| Cache | Redis (StackExchange.Redis) | Latest |
| Message Queue | AWS SQS (AWSSDK.SQS) | Latest |
| Database | PostgreSQL | 16 |
| Testing | xUnit + Testcontainers | Latest |
| Validation | FluentValidation | Latest |
| Observability | OpenTelemetry + Seq | Latest |

---

## 🚀 Getting Started

### Prerequisites

- .NET 9 SDK
- Docker Desktop
- AWS CLI configured (or LocalStack for local development)
- Rider or VS Code

### Run locally

```bash
# Clone the repo
git clone https://github.com/your-username/DistributedShortener.git
cd DistributedShortener

# Start local dependencies (Redis + PostgreSQL)
docker-compose up -d

# Run the API
cd src/DistributedShortener.Api
dotnet run

# In another terminal, run the worker
cd src/DistributedShortener.Worker
dotnet run
```

### Run tests

```bash
# Unit tests
dotnet test tests/DistributedShortener.Domain.Tests

# Integration tests (requires Docker)
dotnet test tests/DistributedShortener.Integration.Tests
```

---

## 📡 API Endpoints

### Create a short link

```http
POST /links
Content-Type: application/json

{
  "originalUrl": "https://www.example.com/very/long/path?with=params",
  "expiresAt": "2025-12-31T23:59:59Z"  // optional
}
```

**Response**
```json
{
  "code": "x7kR2m",
  "shortUrl": "https://short.ly/x7kR2m",
  "originalUrl": "https://www.example.com/very/long/path?with=params",
  "createdAt": "2025-03-14T10:00:00Z"
}
```

### Resolve a redirect

```http
GET /{code}
```

Returns `HTTP 302` redirect to the original URL. Fires a click event to SQS asynchronously.

### Get link analytics

```http
GET /links/{code}/stats
```

```json
{
  "code": "x7kR2m",
  "totalClicks": 1432,
  "clicksByDay": [ ... ],
  "topCountries": [ ... ]
}
```

---

## 🗺️ Roadmap

- [x] Project scaffold and solution structure
- [ ] Domain model (ShortLink aggregate + events)
- [ ] CQRS handlers with MediatR
- [ ] PostgreSQL persistence with EF Core
- [ ] Redis cache-aside implementation
- [ ] AWS SQS integration (LocalStack for local dev)
- [ ] Analytics Worker Service
- [ ] OpenTelemetry instrumentation
- [ ] Integration tests with Testcontainers
- [ ] Deploy to AWS (Lambda + ElastiCache + RDS)

---

## 📚 Key Concepts Practiced

- **CQRS** — separate read/write models with MediatR
- **Event Sourcing** — immutable event log as source of truth
- **Cache-Aside Pattern** — Redis as read-through cache
- **Async Messaging** — fire-and-forget with AWS SQS
- **Clean Architecture** — domain isolated from infrastructure
- **Integration Testing** — real dependencies via Testcontainers

---

## 👤 Author

**Gastón Cisneros** — Senior .NET Backend Developer  
[gcisneros.dev@gmail.com](mailto:gcisneros.dev@gmail.com)

---

*Part of a personal distributed systems learning roadmap — building real patterns with real tech.*
