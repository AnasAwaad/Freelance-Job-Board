# Contributing to FreelanceJobBoard

Thanks for your interest in contributing! This guide will help you set up your environment, follow the workflow, and submit great pull requests.

## Development workflow

1. Fork and clone
```bash
git clone https://github.com/AnasAwaad/Freelance-Job-Board.git
cd Freelance-Job-Board
git checkout -b docs/readme
```

2. Setup
```bash
dotnet restore
# Configure local secrets for API
dotnet user-secrets init -p src/FreelanceJobBoard.API/FreelanceJobBoard.API.csproj
# Set required secrets (examples)
dotnet user-secrets set -p src/FreelanceJobBoard.API/FreelanceJobBoard.API.csproj "ConnectionStrings:DefaultConnection" "<conn>"
dotnet user-secrets set -p src/FreelanceJobBoard.API/FreelanceJobBoard.API.csproj "JwtSettings:SecretKey" "<secret>"
```

3. Branching model
- main — stable
- develop — integration (optional)
- feature/* — new features
- fix/* — bug fixes
- docs/* — documentation updates

4. Commit messages
- Use Conventional Commits
  - feat: new feature
  - fix: bug fix
  - docs: documentation
  - test: tests
  - chore: build/tooling

5. Build & test locally
```bash
dotnet build -c Debug
dotnet test --collect:"XPlat Code Coverage"
```

6. Run locally (two terminals)
```bash
# API
dotnet run --project src/FreelanceJobBoard.API/FreelanceJobBoard.API.csproj
# MVC
dotnet run --project src/FreelanceJobBoard.Presentation/FreelanceJobBoard.Presentation.csproj
```

7. Pull request checklist
- [ ] Code follows existing patterns (CQRS, MediatR, UoW)
- [ ] Unit tests added or updated
- [ ] All tests pass locally
- [ ] README/docs updated if needed
- [ ] No secrets committed; appsettings.* sanitized

## Code style
- C# 12 + .NET 8
- Prefer explicit DTOs and AutoMapper profiles
- Keep controllers thin; business logic in Application layer
- Use ILogger<T> and structured logging
- Validate inputs with FluentValidation/validators where applicable

## Security
- Do not commit secrets or real credentials
- Use user-secrets locally; use managed secrets in CI/CD

## Reporting issues
- Use GitHub issues with a minimal reproduction and logs if available

## Reviewing process
- At least one approval required
- CI should pass (when configured)
- Keep PRs scoped and focused
