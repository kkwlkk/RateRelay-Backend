<div align="center">

# RateRelay Backend

**A review-exchange platform for Google Business Profiles.**
Business owners earn reviews by reviewing others, in a points-based, queue-driven economy.

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![MariaDB](https://img.shields.io/badge/MariaDB-10.11-003545?logo=mariadb&logoColor=white)](https://mariadb.org/)
[![Redis](https://img.shields.io/badge/Redis-7.2-DC382D?logo=redis&logoColor=white)](https://redis.io/)
[![Hangfire](https://img.shields.io/badge/Hangfire-1.8-blue)](https://www.hangfire.io/)
[![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?logo=docker&logoColor=white)](https://docs.docker.com/compose/)

### 🌐 Language / Język

**🇬🇧 English** &nbsp;·&nbsp; [🇵🇱 Polski](README.pl.md)

</div>

---

## Table of contents

- [What is RateRelay?](#what-is-raterelay)
- [How it works](#how-it-works)
- [Architecture](#architecture)
- [Tech stack](#tech-stack)
- [Features](#features)
- [Getting started](#getting-started)
- [Configuration](#configuration)
- [API overview](#api-overview)
- [Points economy](#points-economy)
- [Background jobs](#background-jobs)
- [Permissions](#permissions)
- [Project structure](#project-structure)
- [Deployment](#deployment)
- [Known limitations](#known-limitations)

---

## What is RateRelay?

RateRelay is the backend (REST API) of a platform where local business owners exchange honest
feedback. To receive reviews for your own business, you first have to review other businesses.
Every accepted review earns points, and points are what keeps your own business visible in other
users' review queue.

The API is a modular **ASP.NET Core 8** solution following Clean Architecture, with CQRS via
MediatR, MariaDB for persistence, Redis for caching and distributed locking, and Hangfire for
recurring background work.

---

## How it works

```
 1. Sign in with Google
        │
        ▼
 2. Onboarding: claim your business (Google Place ID)
        │
        ▼
 3. Verification challenge
    The API generates a random day + opening/closing hours.
    You set exactly those hours on your Google Business Profile,
    the API re-reads them through the Google Places API and confirms ownership.
        │
        ▼
 4. Review queue
    GET /api/user/reviewable-businesses/next assigns you a business
    (locked in Redis for 10 minutes so nobody else gets the same one).
        │
        ▼
 5. Submit a review (rating + comment, optionally a public Google Maps review)
    Points are locked on the reviewed owner's balance.
        │
        ▼
 6. The owner accepts or rejects the review
    Accepted  → the reviewer receives the points
    Rejected  → the points return to the owner
    No action within 7 days → auto-accepted by a nightly job
        │
        ▼
 7. Your own balance ≥ 2 points → your business appears in other users' queues
```

**Priority and boosts.** Every business carries a `Priority` byte; admins can boost a business so
it is served earlier in the queue. Businesses a user skips are hidden from them for 12 hours, and
a business rejected three times by a user is excluded from that user's queue entirely.

---

## Architecture

Clean Architecture across four projects, with a strictly inward dependency flow:

```
RateRelay.API             ← Controllers, middleware, filters, attributes, Swagger
        │
RateRelay.Application     ← CQRS (MediatR commands/queries/handlers), DTOs,
        │                   FluentValidation, AutoMapper profiles, Hangfire jobs
RateRelay.Infrastructure  ← EF Core + Pomelo/MySQL, repositories, Unit of Work,
        │                   Redis, services, e-mail, dbup migrations, Serilog
RateRelay.Domain          ← Entities, enums, constants, interfaces, exceptions
```

Key patterns in use:

| Pattern | Where |
| --- | --- |
| **CQRS** | `Features/{Area}/{Commands,Queries}/…` (one folder per use case) |
| **MediatR pipeline behaviors** | `LoggingBehavior`, `ValidationBehavior`, banned-account guard |
| **Repository + Unit of Work** | `IUnitOfWorkFactory` → `IRepository<T>` / `IExtendedRepository<T>` |
| **Envelope responses** | Every endpoint returns `ApiResponse<T>` / `PagedApiResponse<T>` |
| **Distributed locking** | `IRedisDistributedLockProvider` guards queue assignments |
| **Field-level encryption** | `[Encrypted]` attribute + EF value converter (e.g. account e-mail) |
| **Permission bitmask** | `ulong` flags on the account, checked by `[RequirePermission]` |

---

## Tech stack

| Area | Technology |
| --- | --- |
| Runtime | .NET 8 / ASP.NET Core |
| Database | MariaDB 10.11 (EF Core 9 + `Pomelo.EntityFrameworkCore.MySql`) |
| Schema migrations | **dbup-mysql** with embedded SQL scripts (`DataAccess/Migrations/2025/*.sql`) |
| Cache and locks | Redis 7.2 (`StackExchange.Redis`, `DistributedLock.Redis`) |
| Background jobs | Hangfire + `Hangfire.Redis.StackExchange` |
| Mediation / CQRS | MediatR 12 |
| Validation | FluentValidation 12 (preview) |
| Mapping | AutoMapper 14 |
| Auth | Google OAuth (`Google.Apis.Auth`) + JWT bearer with refresh tokens |
| E-mail | MailKit + **Fluid** (`.liquid`) templates, minified with WebMarkupMin |
| Logging | Serilog (console + rolling file sinks, custom enrichers) |
| Docs | Swashbuckle / Swagger UI |
| Hosting | Docker (multi-stage `Dockerfile`) + docker-compose |

---

## Features

### Authentication and accounts
- Google OAuth sign-in exchanged for a JWT access token (1 h) and a refresh token (14 days).
- Refresh-token rotation, with expired tokens purged every 30 minutes.
- Account bans (`AccountBanEntity`) enforced through a MediatR pipeline behavior; expired bans
  are lifted automatically every 5 minutes.
- Account flags and e-mail preferences stored as bit flags.
- E-mail addresses are encrypted at rest.

### Onboarding
A three-step flow (`BusinessVerification → Welcome → Completed`) enforced server-side by
`[RequireOnboardingStep]`, so a user cannot skip ahead.

### Business verification
Ownership is proven without manual review: the API picks a random day and random
opening/closing times, the owner applies them to their Google Business Profile, and
`POST /api/user/business/verification/process` re-fetches `currentOpeningHours` from the Google
Places API and compares. Challenges expire after 7 days; attempts are counted.

### Review queue
- Fair assignment backed by Redis distributed locks (10-minute hold per business).
- Excludes your own business, businesses you already reviewed, and businesses you rejected three
  times; skipped businesses stay hidden for 12 hours.
- Boosted businesses are served with higher priority.

### Reviews and disputes
Statuses: `Pending → Accepted / Rejected / UnderDispute`. Owners can accept or report a review;
anything left pending for 7 days is auto-accepted.

### Referral program
Referral codes, linked accounts, goal types (`ReviewsCompleted`, `BusinessVerified`,
`PointsEarned`, `OnboardingCompleted`), rewards for the referrer, and a welcome bonus
(2 points) for the referred user.

### Support tickets
User-facing ticket system with comments, status history, subjects, closing, and a 1-hour
cooldown between new tickets. Agent and admin operations are permission-gated.

### Operations
- **Rate limiting**: global and per-endpoint via `[RateLimit(limit, periodInSeconds)]`, exposing
  `X-RateLimit-Limit`, `X-RateLimit-Remaining` and `X-RateLimit-Reset`.
- **Maintenance mode**: persisted in the database; `[DisableDuringMaintenance]` blocks
  controllers, while holders of `BypassMaintenanceMode` get through.
- **Hangfire dashboard**: protected by the `AccessHangfireDashboard` permission.
- **Health check**: `GET /api/health`.
- **Transactional e-mail**: welcome, verification intro and incomplete-verification templates;
  `FakeEmailService` is used when e-mail is disabled.

---

## Getting started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- Docker and Docker Compose (for MariaDB and Redis)
- A Google Cloud project with the **Places API** enabled and an **OAuth 2.0 client ID**

### 1. Clone

```bash
git clone https://github.com/kkwlkk/RateRelay-Backend.git
```

> **Windows note:** paths in this repository are deep. If checkout fails with
> *"Filename too long"*, clone with `git -c core.longpaths=true clone …`.

### 2. Start the infrastructure

```bash
docker compose up -d
```

This starts MariaDB on `127.0.0.1:3306` (database `raterelay`) and Redis on `127.0.0.1:6379`,
matching the defaults in `appsettings.json`.

### 3. Provide Google credentials

Create `RateRelay.API/.env` from the template:

```bash
cp RateRelay.API/.env.example RateRelay.API/.env
```

```dotenv
GoogleOAuth__ClientId=your-google-oauth-client-id
GoogleApis__ApiKey=your-google-places-api-key
```

`.env`, `.env.local` and `.env.{Environment}` are loaded automatically in every non-production
environment. In production, use real environment variables instead.

### 4. Run

```bash
dotnet run --project RateRelay.API
```

The schema is created and upgraded by **dbup** on startup, so there is no manual migration step.
Swagger UI is then available at:

```
http://localhost:5206/swagger
```

### Running the API in Docker

```bash
docker build -t raterelay-backend .
```

```bash
docker run -p 5000:5000 --env-file RateRelay.API/.env raterelay-backend
```

---

## Configuration

All settings live in `appsettings.json` and can be overridden by `appsettings.{Environment}.json`,
environment variables, `.env` files and command-line arguments (in that order of precedence).

| Section | Purpose |
| --- | --- |
| `Database` | Connection string, password, migration timeout (default 8 min) |
| `Redis` | Connection string and password |
| `Hangfire` | Key prefix, server name, worker count (default 20) |
| `Jwt` | Secret, issuer, audience, access-token and refresh-token lifetimes |
| `Encryption` | Key used for encrypted entity fields |
| `RateLimit` | On/off switch, default and global limits, response header names |
| `GoogleOAuth` / `GoogleApis` | OAuth client ID and Places API key |
| `Email` / `EmailLinks` / `Company` | SMTP settings and branding used by e-mail templates |
| `AppLogger` | Log directory, rolling interval, console/file toggles |

> ⚠️ The `Jwt:Secret` and `Encryption:Key` values shipped in `appsettings.json` are placeholders.
> Replace them before any real deployment.

---

## API overview

Every response is wrapped in an envelope:

```jsonc
{
  "success": true,
  "data": { /* … */ },
  "error": { "message": "…", "code": "…", "validationErrors": [] },
  "metadata": { /* paging, extra context */ }
}
```

Routes are lower-cased and JSON is camelCase throughout.

### Public

| Method | Route | Description |
| --- | --- | --- |
| `POST` | `/api/auth/google` | Sign in with a Google ID token |
| `POST` | `/api/auth/refresh-token` | Exchange a refresh token for a new access token |
| `GET` | `/api/health` | Health probe |
| `GET` | `/api/maintenance` | Current maintenance-mode status |

### User: account and onboarding

| Method | Route | Description |
| --- | --- | --- |
| `GET` | `/api/user/account` | Current account |
| `PATCH` | `/api/user/account/settings` | Update settings and e-mail preferences |
| `GET` | `/api/user/account/stats` | Account statistics |
| `GET` | `/api/user/account/reviews` | Review history (paged) |
| `GET` | `/api/user/onboarding/status` | Onboarding progress |
| `POST` | `/api/user/onboarding/welcome` | Complete the welcome step |
| `POST` | `/api/user/onboarding/business-verification` | Complete the verification step |
| `POST` | `/api/user/onboarding/complete` | Finish onboarding |

### User: business and verification

| Method | Route | Description |
| --- | --- | --- |
| `POST` | `/api/user/business/verification/initiate` | Claim a business by Place ID |
| `GET` | `/api/user/business/verification/challenge` | Get the opening-hours challenge |
| `POST` | `/api/user/business/verification/process` | Validate the challenge |
| `GET` | `/api/user/business/verification/status` | Verification status |
| `GET` | `/api/user/business` | Your businesses |
| `GET` | `/api/user/business/{businessId}` | Business details |
| `GET` | `/api/user/business/{businessId}/reviews` | Reviews of your business |
| `POST` | `/api/user/business/{id}/reviews/{reviewId}/accept` | Accept a pending review |
| `POST` | `/api/user/business/{id}/reviews/{reviewId}/report` | Report / dispute a review |

### User: review queue, referrals, tickets

| Method | Route | Description |
| --- | --- | --- |
| `GET` | `/api/user/reviewable-businesses/next` | Get the next business to review |
| `GET` | `/api/user/reviewable-businesses/time-left` | Time left on the current assignment |
| `POST` | `/api/user/reviewable-businesses/submit` | Submit a review |
| `GET` | `/api/user/referral/stats` | Referral statistics |
| `GET` | `/api/user/referral/goals` | Referral goals and progress |
| `POST` | `/api/user/referral/generate-code` | Generate your referral code |
| `POST` | `/api/user/referral/link` | Link an account to a referral code |
| `GET` `POST` | `/api/user/tickets` | List / create tickets |
| `GET` | `/api/user/tickets/{id}` | Ticket details (`?includeComments=true`) |
| `GET` `POST` | `/api/user/tickets/{id}/comments` | Read / add comments |
| `PUT` | `/api/user/tickets/{id}/close` | Close a ticket |

### Admin

All admin routes require an admin account **and** the listed permission.

| Method | Route | Permission |
| --- | --- | --- |
| `GET` | `/api/admin/businesses` | `ViewAllBusinesses` |
| `POST` | `/api/admin/businesses` | `CreateBusiness` |
| `GET` | `/api/admin/businesses/{id}` | `ViewAllBusinesses` |
| `DELETE` | `/api/admin/businesses/{id}` | `DeleteBusiness` |
| `POST` | `/api/admin/businesses/{id}/boost` | `ManageBusinessPriority` |
| `POST` | `/api/admin/businesses/{id}/unboost` | `ManageBusinessPriority` |
| `GET` | `/api/admin/users` | `ViewAllUsers` |

---

## Points economy

| Constant | Value | Meaning |
| --- | --- | --- |
| `BasicReviewPoints` | 1 | Awarded for an accepted review |
| `GoogleMapsReviewPoints` | 1 | Bonus for also posting a public Google Maps review |
| `MinimumOwnerPointBalanceForBusinessVisibility` | 2 | Balance required to stay in the queue |
| `ReferralWelcomeBonusPoints` | 2 | Welcome bonus for a referred user |

Every movement is recorded in `point_transactions` with a typed reason (lock, reward, return,
referral, manual or system adjustment), so the balance keeps a full audit trail.

---

## Background jobs

Recurring Hangfire jobs are discovered automatically through `[HangfireRecurringJob]`:

| Job | Schedule | Purpose |
| --- | --- | --- |
| `AutoAcceptOverduePendingBusinessReviewsJob` | `5 0 * * *` (daily, 00:05) | Auto-accept reviews pending for over 7 days |
| `ExpiredBusinessVerificationsCleanupJob` | `5 */6 * * *` (every 6 h) | Remove expired verification challenges |
| `ExpiredRefreshTokensCleanupJob` | `*/30 * * * *` (every 30 min) | Purge expired refresh tokens |
| `ExpiredBansCleanupJob` | `*/5 * * * *` (every 5 min) | Lift expired account bans |

---

## Permissions

Permissions are `ulong` bit flags on the account, checked with `[RequirePermission(...)]`:

- **Tickets**: `ViewAllTickets`, `EditAllTickets`, `AssignTickets`, `ChangeTicketStatus`,
  `AddInternalComments`, `ViewInternalTicketData`, `HandleAssignedTickets`,
  `MarkTicketsObsolete`, `ViewTicketHistory`, `DeleteTickets`
- **Businesses**: `ViewAllBusinesses`, `CreateBusiness`, `DeleteBusiness`, `ManageBusinessPriority`
- **Users**: `ViewAllUsers`, `ManageUsers`
- **System**: `AccessHangfireDashboard`, `ManageHangfireJobs`, `BypassMaintenanceMode`

---

## Project structure

```
RateRelay-Backend/
├── RateRelay.API/                  # HTTP layer
│   ├── Controllers/                # Auth, User/*, Admin/*, Health, Maintenance
│   ├── Attributes/                 # RequireAdmin, RequirePermission, RateLimit, …
│   ├── Middleware/                 # Exception handling, rate limiting, IP logging
│   ├── Filters/                    # Maintenance mode, Swagger security
│   ├── Program.cs / Startup.cs
│   └── appsettings.json
├── RateRelay.Application/          # Use cases
│   ├── Features/{Admin,User,Auth,Shared}/…   # CQRS handlers and validators
│   ├── DTOs/                       # Input/output contracts
│   ├── BackgroundJobs/             # Hangfire recurring jobs
│   ├── MediatR/Behaviors/          # Logging, validation
│   └── Mapping/                    # AutoMapper profiles
├── RateRelay.Infrastructure/       # Technical implementations
│   ├── DataAccess/                 # DbContext, repositories, UoW, Redis, migrations
│   ├── Services/                   # Auth, queue, reviews, points, referrals, Google, e-mail
│   ├── Configuration/              # Strongly-typed options classes
│   ├── EmailTemplates/             # .liquid templates
│   └── Hangfire/ · Logging/ · Authorization/
├── RateRelay.Domain/               # Entities, enums, constants, interfaces
├── docker-compose.yml              # MariaDB + Redis
└── Dockerfile                      # Multi-stage build of the API
```

---

## Deployment

- **Container**: multi-stage `Dockerfile` (SDK build → `aspnet:8.0` runtime), exposing port
  `5000` and starting `dotnet RateRelay.API.dll`.
- **CI/CD**: `.github/workflows/deploy.yml` triggers on every push to `master`, runs on a
  self-hosted runner, syncs the working tree into the deployment directory, and rebuilds the
  `backend` and `nginx` compose services.
- **Migrations**: applied automatically at startup; if dbup fails, the host refuses to start.
- **HTTPS/HSTS**: HTTPS redirection and HSTS are enabled in the `Production` environment.
