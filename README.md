# FreelanceJobBoard

A modern, full-stack freelance marketplace built with ASP.NET Core MVC (Razor Views) + a clean API, featuring CQRS with MediatR, Entity Framework Core (SQL Server), SignalR realtime notifications, Serilog structured logging, JWT + Cookies authentication, and Cloudinary file storage.

[![Build](https://github.com/AnasAwaad/Freelance-Job-Board/actions/workflows/ci.yml/badge.svg)](https://github.com/AnasAwaad/Freelance-Job-Board/actions) [![Coverage](https://img.shields.io/badge/coverage-TODO-brightgreen)](#) [![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](#license) [![Release](https://img.shields.io/github/v/release/AnasAwaad/Freelance-Job-Board?display_name=release)](https://github.com/AnasAwaad/Freelance-Job-Board/releases)

- Project repo: https://github.com/AnasAwaad/Freelance-Job-Board
- Team Leader — Mohamed Khalid; Team — Anas, Usama; Mentor — Moustafa Mousa

## Table of contents
- [Demo / Screenshots](#demo--screenshots)
- [Quick start](#quick-start)
- [Features](#features)
- [Architecture & design](#architecture--design)
- [Configuration](#configuration--environment-variables)
- [Database & migrations](#database--migrations)
- [Logging & monitoring](#logging--monitoring)
- [Testing](#testing)
- [CI / CD](#ci--cd)
- [Deployment](#deployment)
- [Contribution](#contribution)
- [Security & secrets](#security--secrets)
- [License & authors](#license--authors)
- [Troubleshooting & FAQ](#troubleshooting--faq)
- [Contact](#contact)

## Demo / Screenshots
- Hero image
  - assets/project-hero-linkedin.jpg — 1200×627 recommended (LinkedIn/Twitter preview)
- Screenshot
  - assets/screenshot-proposals.jpg — proposals workflow

If no images load:
- TODO: Replace assets/project-hero-linkedin.jpg with a real hero image (1200×627)
- TODO: Replace assets/screenshot-proposals.jpg with a real screenshot captured from the app

## Quick start

Prerequisites
- .NET 8 SDK
- SQL Server (LocalDB or Docker)
- Node/npm (optional for frontend tooling)
- Docker Desktop (optional)

Repository layout
- API: src/FreelanceJobBoard.API
- MVC (Razor Views): src/FreelanceJobBoard.Presentation
- Core libraries: src/FreelanceJobBoard.Domain, src/FreelanceJobBoard.Application, src/FreelanceJobBoard.Infrastructure
- Tests: tests/FreelanceJobBoard.Application.Tests

Clone & restore

```bash
git clone https://github.com/AnasAwaad/Freelance-Job-Board.git
cd Freelance-Job-Board
dotnet restore
```

Database (Entity Framework Core)

```bash
# Update database using API as startup (migration assembly lives in Infrastructure)
dotnet ef database update \
  -p src/FreelanceJobBoard.Infrastructure/FreelanceJobBoard.Infrastructure.csproj \
  -s src/FreelanceJobBoard.API/FreelanceJobBoard.API.csproj
```

Run locally (two terminals)

```bash
# Terminal 1 — API (default: https://localhost:7000)
dotnet run --project src/FreelanceJobBoard.API/FreelanceJobBoard.API.csproj

# Terminal 2 — MVC (default: https://localhost:7117)
dotnet run --project src/FreelanceJobBoard.Presentation/FreelanceJobBoard.Presentation.csproj
```

Docker (optional)

```bash
# TODO: Add Dockerfile(s) and docker-compose.yml for API + MVC + SQL Server
# Example (once added):
docker-compose up --build
```

Run tests

```bash
dotnet test ./tests/FreelanceJobBoard.Application.Tests
# With coverage (built-in collector):
dotnet test ./tests/FreelanceJobBoard.Application.Tests --collect:"XPlat Code Coverage"
```

## Features
- Realtime notifications and user presence via SignalR (NotificationHub at /hubs/notifications)
- Proposals workflow, job lifecycle (create, update, approve, assign, complete)
- Contract change requests & versioning (API + Application layer features)
- Reviews & quick-review summaries
- Authentication: JWT (API) + Cookie auth (MVC) with roles (Admin, Client, Freelancer)
- File uploads via Cloudinary (ICloudinaryService)
- Structured logging with Serilog (console + rolling files, correlation)
- Middleware-based exception and request/response logging (API)
- Clean architecture with CQRS + MediatR in Application layer
- Unit of Work pattern over EF Core repositories
- Two projects separation (API and MVC) with HttpClient integration from MVC to API
- Testing stack: xUnit, Moq, FluentAssertions (and FakeItEasy used in some tests)

## Architecture & design
- MVC + API split:
  - MVC (Presentation) consumes API via typed HttpClient services (e.g., CategoryService, JobService).
  - API exposes REST endpoints secured by JWT; CORS allows MVC origin.
- CQRS with MediatR: Commands/Queries under src/FreelanceJobBoard.Application/Features/* handled by IRequestHandler implementations.
- EF Core + UnitOfWork: ApplicationDbContext in Infrastructure, repositories accessed via IUnitOfWork.
- SignalR: Realtime updates through NotificationHub (groups per user: User_{userId}).
- Logging & middleware: Serilog everywhere; API adds ErrorHandlingMiddleware and RequestResponseLoggingMiddleware.

See docs/ARCHITECTURE.md for diagrams and deeper explanations.

## Configuration / Environment variables
Use appsettings.json + User Secrets (local) or environment variables in production.

Example (API appsettings.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=FreelanceJobBoard;Trusted_Connection=True;"
  },
  "JwtSettings": {
    "SecretKey": "<very-strong-secret>",
    "Issuer": "FreelanceJobBoard",
    "Audience": "FreelanceJobBoard-Users",
    "ExpirationInHours": "24"
  },
  "Cloudinary": {
    "CloudName": "<cloud-name>",
    "ApiKey": "<api-key>",
    "ApiSecret": "<api-secret>"
  },
  "Serilog": {
    "WriteTo": [
      { "Name": "Console" },
      { "Name": "File", "Args": { "path": "logs/api-.log", "rollingInterval": "Day" } }
    ]
  }
}
```

Common keys
- ConnectionStrings__DefaultConnection
- JwtSettings__SecretKey, JwtSettings__Issuer, JwtSettings__Audience
- Cloudinary__CloudName, Cloudinary__ApiKey, Cloudinary__ApiSecret
- Serilog sinks (console, file, optional Seq)

Secrets
- Use dotnet user-secrets for local development:

```bash
cd src/FreelanceJobBoard.API
# Set your real values
dotnet user-secrets init
 dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<your-connection-string>"
 dotnet user-secrets set "JwtSettings:SecretKey" "<your-secret>"
 dotnet user-secrets set "Cloudinary:CloudName" "<cloud-name>"
 dotnet user-secrets set "Cloudinary:ApiKey" "<api-key>"
 dotnet user-secrets set "Cloudinary:ApiSecret" "<api-secret>"
```

## Database & migrations
- Migration assembly: Infrastructure project
- Typical commands

```bash
# Add a migration (example)
dotnet ef migrations add InitialCreate \
  -p src/FreelanceJobBoard.Infrastructure/FreelanceJobBoard.Infrastructure.csproj \
  -s src/FreelanceJobBoard.API/FreelanceJobBoard.API.csproj \
  -o src/FreelanceJobBoard.Infrastructure/Migrations

# Apply migrations
dotnet ef database update \
  -p src/FreelanceJobBoard.Infrastructure/FreelanceJobBoard.Infrastructure.csproj \
  -s src/FreelanceJobBoard.API/FreelanceJobBoard.API.csproj

# Reset database (local only!)
dotnet ef database drop -f -p src/FreelanceJobBoard.Infrastructure/FreelanceJobBoard.Infrastructure.csproj -s src/FreelanceJobBoard.API/FreelanceJobBoard.API.csproj
```

Seeding
- TODO: Enable/verify data seeding (Presentation Program.cs notes reseeding placeholder)

## Logging & monitoring
- Serilog configured via appsettings in both API and MVC
  - Console + rolling file logs (logs/api-.log, logs/api-errors-.log, logs/api-performance-.log)
  - Enrichers: FromLogContext, EnvironmentName, ThreadId, CorrelationId (MVC)
- API middlewares add request/response logging and error handling
- Change sinks (e.g., Seq) by updating Serilog configuration

## Testing
- Test project: tests/FreelanceJobBoard.Application.Tests
- Libraries: xUnit, Moq, FluentAssertions (and FakeItEasy in some cases)

Commands
```bash
dotnet test
# or target only application tests:
dotnet test ./tests/FreelanceJobBoard.Application.Tests
# coverage
dotnet test --collect:"XPlat Code Coverage"
```

See docs/TESTING.md for examples and patterns.

## CI / CD
- GitHub Actions: no workflows committed yet
  - TODO: Add .github/workflows/ci.yml for build + test + (optional) Docker publish

Recommended starter workflow (add as .github/workflows/ci.yml)

```yaml
name: CI
on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]
jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'
      - name: Restore
        run: dotnet restore
      - name: Build
        run: dotnet build --no-restore -c Release
      - name: Test
        run: dotnet test --no-build --collect:"XPlat Code Coverage"
```

## Deployment
- Option A: Azure App Service (deploy API and MVC separately)
- Option B: Docker Compose (API, MVC, SQL Server)
  - TODO: Provide docker-compose.yml with services: api, web, db
- Option C: Azure Container Apps or Azure Container Instances

## Contribution
We welcome issues and PRs! Read docs/CONTRIBUTING.md.

- Branches: feature/*, fix/*, docs/*
- Conventional commits (e.g., feat:, fix:, docs:, test:, chore:)
- PR checklist:
  - [ ] Unit tests added/updated
  - [ ] Docs updated
  - [ ] Builds & tests green

## Security & secrets
- Never commit secrets. Use User Secrets locally and Key Vault/Secret Manager in cloud.
- Rotate keys periodically (JWT, Cloudinary, DB).
- Review CORS and authentication policies before going live.

## License & authors
- License: MIT (default)
  - TODO: Add LICENSE file if different license is intended
- Credits:
  - Team Leader — Mohamed Khalid
  - Team — Anas, Usama
  - Mentor — Moustafa Mousa
- Repository: https://github.com/AnasAwaad/Freelance-Job-Board

## Troubleshooting & FAQ
- Port conflicts
  - If API 7000 or MVC 7117 are in use, change launchSettings.json or command line ports
  - TODO: Confirm exact ports configured by your launch settings
- DB connection errors
  - Verify ConnectionStrings:DefaultConnection and SQL Server is running
  - If using LocalDB, ensure (localdb) instance exists
- EF migration errors
  - Ensure -p points to Infrastructure and -s to API
- Cloudinary upload issues
  - Verify credentials; ensure files are public if needed
- SignalR connection
  - Ensure MVC connects to /hubs/notifications and auth cookies are present

## Contact
- Issues: https://github.com/AnasAwaad/Freelance-Job-Board/issues
- Maintainers: open an issue and tag the team
