# AGENTS.md — Universal Appointment Scheduler Backend

This document provides context and conventions for AI agents working on this .NET 10 appointment scheduling backend API.

---

## 1. Project Overview

A .NET 10 Web API for managing automotive service appointments at dealerships. The system tracks customers, vehicles, technicians, service bays, and appointments with role-based authentication using JWT and EF Core with PostgreSQL.

---

## 2. Build & Development Commands

```bash
# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run the development server (HTTP: 5210, HTTPS: 7025)
dotnet run

# Run with watch mode for hot reload
dotnet watch run

# Run unit tests
dotnet test universal-scheduler-be.Tests/universal-scheduler-be.Tests.csproj

# Entity Framework Core commands
# Add a new migration
dotnet ef migrations add <MigrationName>

# Update database with latest migrations
dotnet ef database update

# Remove last migration (if not applied)
dotnet ef migrations remove

# Generate SQL script for migrations
dotnet ef migrations script
```

---

## 3. Code Style & Conventions

### Language & Framework
- **Target Framework**: .NET 10.0
- **Language**: C# 13 with nullable reference types enabled (`<Nullable>enable</Nullable>`)
- **Implicit Usings**: Enabled for cleaner code

### Naming Conventions
- **Classes/Public Members**: PascalCase
- **Private Fields**: `_camelCase`
- **Local Variables**: camelCase
- **Constants**: UPPER_SNAKE_CASE
- **Interfaces**: Prefix with `I` (e.g., `IAppointmentService`)
- **Files**: Match class name exactly

### Architecture Patterns
- **ASP.NET Core API Controllers**: Expose HTTP APIs via `[ApiController]` classes in `Controllers/` — do not use minimal APIs (`MapGet` / `MapPost`) for application endpoints
- **Entity Framework Core**: Code-first with explicit entity configurations
- **Repository Pattern**: DbContext serves as the unit of work
- **Configuration**: Use `IConfiguration` for settings, connection strings in `appsettings.json`

### Code Organization
- API controllers in `Controllers/` (e.g. `AuthController.cs`, `DealershipController.cs`)
- Application services and their interfaces in `Infrastructure/Services/`
- Feature DTOs in `Infrastructure/<Feature>/Dtos/` (e.g. `Infrastructure/Dealerships/Dtos/`)
- Shared non-auth result types in `Infrastructure/Common/` (e.g. `ServiceResult<T>`)
- Auth support types (DTOs, options, authorization helpers) in `Infrastructure/Auth/`
- Place entity configurations in `Infrastructure/Persistence/Configurations/`
- Entity classes in `Infrastructure/Entities/`
- Seed data in `Infrastructure/Persistence/AuthSeedData.cs`
- Migrations auto-generated in `Migrations/`

### CRUD Endpoint Pattern (Phase 3+)
- Service returns `ServiceResult<T>` with `Data`, `Error`, and `StatusCode` — controller maps to HTTP via `Problem()` for errors
- Controllers use class-level `[Authorize]` plus action-level permission policies (e.g. `dealerships:read`, `dealerships:write`)
- Register services in `Program.cs`; add permission policies in `AuthorizationExtensions.AddPermissionPolicies`
- Seed new permissions in `AuthSeedData` and add an EF migration when introducing policies

### Key Rules
- Use API controllers for all HTTP endpoints; register with `AddControllers()` and `MapControllers()` in `Program.cs`
- Always use explicit entity configurations for EF Core relationships
- Keep entities POCO (plain C# objects) - avoid logic in entity classes
- Use `Guid` for primary keys (configured in EF to auto-generate)
- Enable nullability compiler checks - avoid `null!` suppression when possible

---

## 4. Repository Structure

```
.
├── Controllers/                  # ASP.NET Core API controllers
│   ├── AuthController.cs
│   └── DealershipController.cs
├── Infrastructure/
│   ├── Auth/                     # Auth DTOs, JWT options, authorization helpers
│   │   ├── Dtos/
│   │   ├── AuthResult.cs
│   │   ├── AuthorizationExtensions.cs
│   │   └── JwtOptions.cs
│   ├── Common/                   # Shared types (e.g. ServiceResult<T>)
│   ├── Dealerships/Dtos/         # Dealership request/response DTOs
│   ├── Services/                 # Application services
│   │   ├── AuthService.cs
│   │   ├── IAuthService.cs
│   │   ├── DealershipService.cs
│   │   ├── IDealershipService.cs
│   │   ├── JwtTokenService.cs
│   │   └── IJwtTokenService.cs
│   ├── Entities/                 # EF Core entity classes
│   │   ├── Appointment.cs
│   │   ├── Customer.cs
│   │   ├── Dealership.cs
│   │   ├── Permission.cs
│   │   ├── Role.cs
│   │   ├── RolePermission.cs
│   │   ├── ServiceBay.cs
│   │   ├── ServiceType.cs
│   │   ├── Skill.cs
│   │   ├── Technician.cs
│   │   ├── TechnicianSkill.cs
│   │   ├── User.cs
│   │   └── Vehicle.cs
│   └── Persistence/
│       ├── ApplicationDbContext.cs      # Main EF Core DbContext
│       ├── AdminUserSeeder.cs           # Runtime admin user seed
│       ├── AuthSeedData.cs              # Role/permission seed data
│       ├── DesignTimeDbContextFactory.cs # EF CLI support
│       └── Configurations/              # EF entity configurations
│           ├── UserConfiguration.cs
│           ├── RoleConfiguration.cs
│           ├── PermissionConfiguration.cs
│           ├── RolePermissionConfiguration.cs
│           └── TechnicianSkillConfiguration.cs
├── Migrations/                   # EF Core auto-generated migrations
│   ├── 20260529185128_InitialCreate.cs
│   ├── 20260605061000_AddAuthentication.cs
│   ├── 20260611135736_AddDealershipPermissions.cs
│   ├── 20260611144910_RestrictDealershipPermissionsToAdmin.cs
│   └── ApplicationDbContextModelSnapshot.cs
├── docs/
│   ├── IMPLEMENTATION_PLAN.md           # Phased progress tracker
│   ├── entity-relationship-diagram.md   # Full ERD documentation
│   └── relationship-map.md
├── .vscode/
│   ├── launch.json              # Debug configurations (set ASPNETCORE_URLS — launchSettings not used when launching DLL)
│   └── tasks.json               # Build task
├── universal-scheduler-be.Tests/ # xUnit unit tests (Moq, EF InMemory)
│   ├── Controllers/
│   ├── Services/
│   └── Helpers/
├── reflections/                 # Post-session reflection records
├── Program.cs                   # Application entry point
├── appsettings.json            # Production configuration
├── appsettings.Development.json # Development configuration
├── universal-scheduler-be.csproj
└── universal-scheduler-be.http  # HTTP request samples for testing
```

---

## 5. Environment Setup

### Required Environment Variables
Copy connection string and settings to `appsettings.Development.json` for local development:

| Variable | Description | Default |
|----------|-------------|---------|
| `ConnectionStrings:DefaultConnection` | PostgreSQL connection string | `Host=localhost;Port=5432;Database=universal_scheduler;Username=postgres;Password=TestPassword` |
| `ASPNETCORE_ENVIRONMENT` | Runtime environment | `Development` |
| `ASPNETCORE_URLS` | Server endpoints | `https://localhost:7025;http://localhost:5210` |

### Database Setup
1. Install PostgreSQL locally or use a Docker container
2. Create database `universal_scheduler`
3. Run migrations: `dotnet ef database update`
4. Seed data for auth entities is applied automatically via migrations

### External Services
- **PostgreSQL**: Required for data persistence

### JWT & Admin Seed (User Secrets in development)
```bash
dotnet user-secrets set "Jwt:Key" "<32+ character secret>"
dotnet user-secrets set "AdminSeed:Password" "<admin password>"
```
Non-secret defaults live in `appsettings.json` (`Jwt:Issuer`, `Jwt:Audience`, `AdminSeed:Email`, etc.).

---

## 6. Important Constraints

### Security & Secrets
- **NEVER commit `appsettings.Production.json` or any file containing real passwords**
- Use environment variables or Azure Key Vault for production secrets
- Connection strings with passwords should only exist in `appsettings.Development.json` (already gitignored via standard .NET patterns)

### Version Control
- **Open a PR rather than pushing directly to `main`**
- Commit messages follow: `Add feature X`, `Fix bug in Y`, `Update Z configuration`
- Include migration files in commits when schema changes

### Database Migrations
- Always review auto-generated migrations before committing
- For production, generate SQL scripts and apply manually rather than using `EnsureCreated()`
- Test migrations on a copy of production data when possible

### Entity Framework Conventions
- Use explicit configurations in `Configurations/` folder rather than data annotations
- Configure all relationships (one-to-many, many-to-many) explicitly
- Use `HasData()` only for seed data that must exist (roles, permissions)

---

## 7. Domain Context for Agents

### Core Domain Concepts

**Appointment Scheduling Flow:**
1. A `Customer` owns one or more `Vehicle`s
2. `Dealership` has `Technician`s and `ServiceBay`s
3. Each `Technician` has `Skill`s via `TechnicianSkill` junction table
4. `ServiceType` requires a specific `Skill` and defines duration/price
5. `Appointment` links customer, vehicle, technician, service bay, and service type

**Authentication Model (Separate from Scheduling):**
- `User` entities handle login (not linked to `Customer` currently)
- `Role` → `Permission` many-to-many via `RolePermission`
- JWT auth via `AuthController` (`POST /api/auth/login`, `POST /api/auth/register`, `GET /api/auth/me`)
- Register returns `RegisterResponse` (email, role); login returns `AuthResponse` (token, expiresAt, email, role)
- Login errors: unknown/wrong credentials → `"Invalid email or password."`; inactive account → `"Account is inactive."`
- Admin user seeded at startup when `AdminSeed:Email` does not exist
- Business endpoints use permission policies (`dealerships:read`, `dealerships:write`, etc.) — claims are embedded in the JWT at login; users must re-login after permission migrations
- `dealerships:read` and `dealerships:write` are **Admin only** (Staff/User do not receive these claims)

### Key Relationships
- `Dealership` 1:* `Technician`, `ServiceBay`, `ServiceType`
- `Customer` 1:* `Vehicle`, `Appointment`
- `Technician` *:* `Skill` (via `TechnicianSkill`)
- `Role` *:* `Permission` (via `RolePermission`)
- `Appointment` links multiple entities for scheduling

### Current State
- **Progress tracker**: [docs/IMPLEMENTATION_PLAN.md](docs/IMPLEMENTATION_PLAN.md) — Phases 1–3 complete; Phase 4 (Appointment Booking) next
- Database schema established with migrations
- Entity Framework configured with PostgreSQL
- Authentication entities (User/Role/Permission) migrated with role/permission seed data
- JWT login, register, and `/me` implemented in `Controllers/AuthController.cs`
- **Dealership CRUD** implemented — `GET/POST/PUT /api/dealerships` with `dealerships:read` / `dealerships:write` policies (`Controllers/DealershipController.cs`, `Infrastructure/Services/DealershipService.cs`)
- **Skill CRUD** implemented — `GET/POST/DELETE /api/skills` with `skills:read` / `skills:write` policies
- **ServiceType CRUD** implemented — nested under `/api/dealerships/{id}/service-types` with `servicetypes:read` / `servicetypes:write`
- **ServiceBay CRUD** implemented — nested under `/api/dealerships/{id}/service-bays` with `servicebays:read` / `servicebays:write`
- **Technician CRUD** implemented — nested under `/api/dealerships/{id}/technicians` with `technicians:read` / `technicians:write`; skills assigned via optional `skillIds` on create/update
- **Customer CRUD** implemented — `GET/POST/PUT /api/customers` with `customers:read` / `customers:write` (Admin + Staff)
- **Vehicle CRUD** implemented — nested under `/api/customers/{id}/vehicles` with `vehicles:read` / `vehicles:write` (Admin + Staff)
- Unit test project with **110 tests** (`universal-scheduler-be.Tests`)
- VS Code debug config for .NET 10 (`.vscode/launch.json`)
- Cursor rule `.cursor/rules/agents-md.mdc` points agents to this file each session
- Session notes: [reflections/2026-06-11_dealership_crud.md](reflections/2026-06-11_dealership_crud.md)
- **Next priority**: Phase 4 — **Appointment Booking** (availability, create, list endpoints)

---

## 8. Post-Session Reflection Protocol

This protocol ensures every agent conversation becomes a learning opportunity. At the end of each session, the agent must run through this reflection loop.

### When Reflection Runs

The post-session reflection runs whenever:
- A coding or research task is completed
- A session ends with partial progress but a natural stopping point (PR opened, failing test isolated, design draft complete)
- **The reflection must run before closing the session or moving to an unrelated task**

### Agent Responsibilities

At the end of each session, execute a "Reflect" phase:

1. **Summarize the Session**
   - What we attempted
   - What we actually implemented or concluded
   - What changed (files, APIs, schemas, configs, docs)

2. **Analyze Decisions and Trade-offs**
   - Why specific approaches were chosen
   - Alternatives considered and rejected
   - Known limitations or shortcuts taken

3. **Identify Issues and Follow-ups**
   - Bugs, failing tests, or unclear behavior discovered
   - Open questions and assumptions
   - Concrete next steps and their expected difficulty

4. **Prepare Questions for the Human**
   - Ask targeted questions about requirements, edge cases, architecture constraints, or domain rules
   - Ask what was confusing or surprising from the engineer's perspective
   - Avoid generic "what do you think?" prompts

### Human Engineer Responsibilities

Participate in the reflection loop:
- Answer agent questions clearly, especially around domain rules and "why" behind decisions
- Correct any misunderstandings in the agent's summary
- Mark follow-ups as: Must-do before release / Nice-to-have later / Won't-do (with rationale)
- Optionally add perspective on workflow friction or missing context

### Reflection Format

At session end, output:

```markdown
## [Session Reflection]

### Session Summary
- **Task**: <1-2 sentence description>
- **What we tried**: <approaches attempted>
- **What we implemented**: <what actually changed>
- **Files touched**: <main files, endpoints, services, or schemas>

### Key Decisions
- **Decision**: <what we decided>
  - **Rationale**: <why this was chosen>
  - **Alternatives**: <what was rejected>
- ...

### Issues and Next Steps
- [ ] <open bug or failing test> — notes
- [ ] <missing test or validation> — notes
- [ ] <design/API/UX question> — notes

### Questions for You
1. <question about requirements or domain rules>
2. <question about architecture or constraints>
3. <question about edge cases or error handling>
4. <question about what was confusing or surprising>
5. <question about workflow improvements>

### Reflection Saved
- **Stored as**: `reflections/YYYY-MM-DD_HHMM_<brief-description>.md`
- **Next session focus**: <1-2 sentences>
```

### Memory and Artifacts

After the human responds:
1. Create a reflection record in the `reflections/` directory (create if doesn't exist)
2. Include: date, task ID, summary, key decisions, issues, Q&A, and important code pointers
3. Link relevant reflections from docs or architecture notes for future sessions

### Example Reflection Record

```json
{
  "date": "2026-06-10T14:30:00Z",
  "task_id": "feature/add-jwt-auth-endpoints",
  "summary": "Implemented JWT authentication endpoints...",
  "key_decisions": [
    {
      "decision": "Used ASP.NET Core Identity",
      "rationale": "Reduces boilerplate for auth",
      "alternatives": ["Custom JWT implementation"]
    }
  ],
  "issues_and_next_steps": [
    "Add refresh token endpoint",
    "Write integration tests for auth flow"
  ],
  "human_answers": {
    "q1": "Prefer short-lived access tokens (15min)",
    "q2": "Yes, support both web and mobile clients"
  },
  "important_pointers": [
    "Program.cs: JWT configuration",
    "Infrastructure/Services/JwtTokenService.cs",
    "Controllers/AuthController.cs"
  ]
}
```

---

<!-- TODO: Fill in as project evolves — see docs/IMPLEMENTATION_PLAN.md for phased tracker -->
- [x] Phase 3–8 items tracked in [docs/IMPLEMENTATION_PLAN.md](docs/IMPLEMENTATION_PLAN.md)
- [x] Phase 3 feature 1 (Dealership CRUD) — see [reflections/2026-06-11_dealership_crud.md](reflections/2026-06-11_dealership_crud.md)
- [ ] Document deployment process
- [ ] Add CI/CD pipeline configuration
