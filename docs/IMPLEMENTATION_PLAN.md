# Implementation Plan & Progress Tracker

**Last updated:** 2026-06-11  
**Current phase:** Phase 3 — Core CRUD (in progress; Dealership complete)

Use this document to track what is done, in progress, and pending. Update checkboxes and the phase summary when tasks land.

### Status legend

| Symbol | Meaning |
|--------|---------|
| ✅ | Done |
| 🔶 | Partial — started but not fully meeting the phase goal |
| ⬜ | Not started |
| ↪ | Deferred or out of original scope (note explains) |

---

## Progress overview

| Phase | Name | Status | Notes |
|-------|------|--------|-------|
| 1 | Infrastructure Setup | ✅ Complete | EF Core + PostgreSQL + migrations |
| 2 | Authentication | ✅ Complete | JWT, User, register/login; `[Authorize]` on `/api/auth/me` only until Phase 3 |
| 3 | Core CRUD Endpoints | 🔶 In progress | Dealership CRUD done; 7 features remaining |
| 4 | Appointment Booking | ⬜ Not started | `Appointments` table exists; no API or scheduling logic |
| 5 | Appointment Lifecycle | ⬜ Not started | `AppointmentStatus` enum in code only |
| 6 | Validation & Error Handling | ⬜ Not started | Manual validation in auth only |
| 7 | Testing | 🔶 Partial | 18 auth unit tests; no integration tests |
| 8 | API Documentation | 🔶 Partial | `MapOpenApi()` in dev; no Swashbuckle / JWT UI |

---

## Phase 1 — Infrastructure Setup ✅

| Task | Status | Evidence |
|------|--------|----------|
| Install `Npgsql.EntityFrameworkCore.PostgreSQL`, `Microsoft.EntityFrameworkCore.Design` | ✅ | `universal-scheduler-be.csproj` |
| Connection string in `appsettings.json` + register `ApplicationDbContext` in `Program.cs` | ✅ | `appsettings.json`, `Program.cs` |
| First migration: `InitialCreate` | ✅ | `Migrations/20260529185128_InitialCreate.cs` |
| Apply migration + verify tables in PostgreSQL | ✅ | `db.Database.MigrateAsync()` on startup |

**Extras beyond original plan:** `DesignTimeDbContextFactory`, explicit entity configurations, auth migration `AddAuthentication`.

---

## Phase 2 — Authentication ✅

| Task | Status | Evidence |
|------|--------|----------|
| Install `Microsoft.AspNetCore.Authentication.JwtBearer` | ✅ | `universal-scheduler-be.csproj` |
| `User` entity (email, password hash, role) — separate from `Customer` | ✅ | `Infrastructure/Entities/User.cs` + `Role` / `Permission` / `RolePermission` |
| Register and login endpoints returning JWT access token | ✅ | `Controllers/AuthController.cs`, `Infrastructure/Services/AuthService.cs` |
| JWT middleware in `Program.cs` | ✅ | `AddAuthentication` + `AddJwtBearer` in `Program.cs` |
| Protect routes with `[Authorize]` | 🔶 | `[Authorize]` on `GET /api/auth/me`; permission policies seeded — business routes in Phase 3 |

**Design notes (intentional deviations):**

- Register returns `{ email, role }` only (no token); clients must call login — see `reflections/2026-06-10_auth_implementation.md`.
- Role/permission system is richer than a single `role` string; policies like `appointments:read` are ready for Phase 3+.
- 18 unit tests cover auth services and controller (`universal-scheduler-be.Tests/`).

**Phase 2 exit criteria met.** Apply `[Authorize]` to new endpoints as they are built in Phase 3.

---

## Phase 3 — Core CRUD Endpoints 🔶

Build in this order — each feature should be shippable on its own.

| # | Feature | Endpoints | Status |
|---|---------|-----------|--------|
| 1 | **Dealership** | GET all, GET by id, POST, PUT | ✅ |
| 2 | **Skill** | GET all, POST, DELETE | ⬜ |
| 3 | **ServiceBay** | GET by dealership, POST, PUT, soft delete | ⬜ |
| 4 | **ServiceType** | GET by dealership, POST, PUT, soft delete | ⬜ |
| 5 | **Technician** | GET by dealership, POST, PUT, soft delete | ⬜ |
| 6 | **TechnicianSkill** | POST (assign), DELETE (remove) | ⬜ |
| 7 | **Customer** | GET, POST, PUT | ⬜ |
| 8 | **Vehicle** | GET by customer, POST, PUT, DELETE | ⬜ |

**Groundwork already in place:** entity classes under `Infrastructure/Entities/`, tables in `InitialCreate` migration, EF configurations for some join tables.

**Suggested per-feature checklist:**

- [ ] Request/response DTOs
- [ ] Service interface + implementation
- [ ] Controller with `[Authorize]` + permission policy where appropriate
- [ ] Manual smoke test via `universal-scheduler-be.http`

---

## Phase 4 — Appointment Booking ⬜

Most important phase — implement carefully after Phase 3 CRUD is stable.

| Task | Status |
|------|--------|
| `GET /availability?dealershipId=&serviceTypeId=&date=` — qualified technicians, slot generation, conflict subtraction | ⬜ |
| `POST /appointments` — validate skill, technician + bay conflict check, `EndAt = StartAt + DurationMinutes`, transaction | ⬜ |
| `GET /appointments/:id` | ⬜ |
| `GET /customers/:id/appointments` — customer history | ⬜ |
| `GET /dealerships/:id/appointments?date=` — daily staff schedule | ⬜ |

**Known schema gaps to resolve during Phase 4:**

- `Appointment.Status` exists on the C# entity but may need a migration column.
- `VehicleId` FK exists in DB; confirm entity property alignment before booking.

---

## Phase 5 — Appointment Lifecycle ⬜

| Task | Status |
|------|--------|
| `PATCH /appointments/:id/status` — valid transitions only | ⬜ |
| Enforce transition rules (e.g. no `Completed` → `Confirmed`) | ⬜ |
| `POST /appointments/:id/cancel` — cancel with reason | ⬜ |

---

## Phase 6 — Validation & Error Handling ⬜

| Task | Status |
|------|--------|
| Install `FluentValidation.AspNetCore` | ⬜ |
| Validators for all request DTOs | ⬜ |
| Global exception handler middleware — consistent error shapes | ⬜ |
| Proper HTTP status codes (400, 404, 409) | ⬜ |

**Note:** Auth currently uses inline validation in `AuthService` and `Problem()` in `AuthController`. Refactor to FluentValidation when this phase starts, or add validators incrementally per controller.

---

## Phase 7 — Testing 🔶

| Task | Status | Evidence |
|------|--------|----------|
| Install xUnit, Moq, `Microsoft.EntityFrameworkCore.InMemory` | ✅ | `universal-scheduler-be.Tests/universal-scheduler-be.Tests.csproj` |
| Unit test availability engine (slots + conflicts) | ⬜ | Blocked on Phase 4 |
| Unit test status transition rules | ⬜ | Blocked on Phase 5 |
| Integration test booking (happy path + double-booking) | ⬜ | No `WebApplicationFactory` yet |
| Auth unit tests | ✅ | 18 tests in `AuthServiceTests`, `JwtTokenServiceTests`, `AuthControllerTests` |
| Auth HTTP integration tests | ↪ | Deferred — see `reflections/2026-06-10_auth_implementation.md` |

---

## Phase 8 — API Documentation 🔶

| Task | Status | Evidence |
|------|--------|----------|
| Install `Swashbuckle.AspNetCore` | ⬜ | — |
| `[ProducesResponseType]` on controllers | 🔶 | Auth controller only |
| Swagger UI in `Program.cs` | ⬜ | — |
| JWT auth support in Swagger UI | ⬜ | — |
| OpenAPI document (non-Swagger) | ✅ | `AddOpenApi()` + `MapOpenApi()` in Development |

---

## Recommended next session

1. **Phase 3, feature 2** — **Skill** CRUD (GET all, POST, DELETE).
2. Reuse `ServiceResult<T>`, `[Authorize]` + permission policies pattern from Dealership.
3. Keep `universal-scheduler-be.http` updated for manual testing.

After Phase 3 entities exist via API, Phase 4 availability/booking can use real data instead of seed-only DB rows.

---

## Related docs

- [AGENTS.md](../AGENTS.md) — conventions and architecture
- [entity-relationship-diagram.md](./entity-relationship-diagram.md) — domain model
- [reflections/2026-06-10_auth_implementation.md](../reflections/2026-06-10_auth_implementation.md) — auth session notes
