# Validation — `accounts-access` (`metro-core`, PL7)

- **Feature folder:** `.ai/sdd/specs/accounts-access/`
- **Date:** 2026-11-24

## Scope and Traceability

Selected complete acceptance scenarios:
- `ACC-S1`: Minimal passenger registration (`email` + `password`) defaults to `Passenger` (`FR-ACC-01`, `FR-ACC-02`).
- `ACC-S2`: Duplicate e-mail returns `409 Conflict` with English message (`FR-ACC-01`).
- `ACC-S3`: Canonical roles (`Passenger`, `Staff`) accepted; legacy/invalid roles (`master`, `client`, `admin`, `user`, wrong casing, empty) rejected (`FR-ACC-02`, `NFR-SEC-02`, `ADR-003`).
- `ACC-S4`: Self-registration enforces `Passenger` unless caller holds `Staff` (`FR-ACC-02`, `NFR-SEC-02`).

| Requirement / Scenario | Task | Code Location | Test Identifier | Final Result |
| --- | --- | --- | --- | :---: |
| `FR-ACC-01` (`ACC-S1`) | `ACC-T03`, `ACC-T05` | `CreateUserCommand.cs`, `CreateUserCommandValidator.cs`, `CreateUserHandler.cs` | `CommandValidatorTests.CreateUserCommand_WithEmailAndPasswordOnly_DefaultsToPassengerAndIsValid` | **Pass** |
| `FR-ACC-01` (`ACC-S2`) | `ACC-T03`, `ACC-T05` | `CreateUserHandler.cs` | `CreateUserHandlerTests.Handle_WhenEmailAlreadyExists_ReturnsConflict_AndDoesNotPersist` | **Pass** |
| `FR-ACC-02` (`ACC-S3`) | `ACC-T01`, `ACC-T05` | `UserRoles.cs`, `CreateUserCommandValidator.cs`, `UpdateUserCommandValidator.cs` | `UserRolesTests.IsValid_WhenCanonicalRole_ReturnsTrue`, `UserRolesTests.IsValid_WhenLegacyOrUnknownRole_ReturnsFalse`, `CommandValidatorTests.CreateUserCommand_WhenRoleInvalid_HasError` | **Pass** |
| `FR-ACC-02` (DB `CK_User_Role`) | `ACC-T02` | `UserMap.cs`, `20261124120000_CanonicalUserRoles.cs`, `PostgreSQLInitializer.cs` | Build + migration compilation check (live PostgreSQL integration test tracked in `ACC-T07`) | **Pass (compile) / Pending (live DB test)** |
| `NFR-SEC-01` | `ACC-T01`, `ACC-T05` | `PasswordPolicy.cs`, `PasswordService.cs` | `PasswordPolicyTests.*` (12 cases) | **Pass** |
| `NFR-SEC-02` (`ACC-S4`) | `ACC-T04`, `ACC-T07` | `Program.cs`, `UserController.cs` | Unit coverage on `UserRoles` + validators; full HTTP `403`/`401` matrix pending in `ACC-T07` | **Partial (HTTP integration pending)** |

## Execution Evidence

Working directory: `metro-core` repository root (`Metro.sln`, .NET 10 SDK).

- **Initial pre-fix audit baseline (Red / Spec Disagreement):**
  - Inspection command: `Select-String -Pattern '"master"|"client"' src/`
  - Result: Found legacy roles `"master"` and `"client"` across `Program.cs`, `UserController.cs`, `PostgreSQLInitializer.cs`, `CreateUserCommand.cs`, `UpdateUserCommand.cs`, `CreateUserCommandValidator.cs`, `UpdateUserCommandValidator.cs`, and Portuguese validation strings — directly contradicting `docs/spec/requirements.md` (`FR-ACC-02`, §7 English interface).
- **Final build & analyzers command:**
  - Command: `dotnet build Metro.sln -c Release -p:EnableNETAnalyzers=true -p:AnalysisLevel=latest`
  - Result: `Build succeeded. 0 Warning(s), 0 Error(s)` (exit code `0`).
- **Final unit tests & coverage command:**
  - Command: `dotnet test Metro.sln --no-build -c Release --collect:"XPlat Code Coverage"`
  - Result:
    - `Metro.Domain.Tests.dll`: **22 passed, 0 failed, 0 skipped** (235 ms).
    - `Metro.Application.Tests.dll`: **47 passed, 0 failed, 0 skipped** (1 s).
    - **Total: 69 / 69 passed**, Cobertura coverage XML generated.
- **Formatting & SCA vulnerability commands:**
  - `dotnet format whitespace Metro.sln --no-restore --verify-no-changes` → exit code `0`.
  - `dotnet list Metro.sln package --vulnerable --include-transitive` → **0 vulnerable packages** across all 11 projects (exit code `0`).
- **Compose deployment validation:**
  - `docker compose -f docker-compose.yml config -q` → `0`
  - `docker compose -f docker-compose.dev.yml config -q` → `0`
  - `docker compose -f docker-compose.prod.yml config -q` → `0`

## AI Assistance and Human Decisions

- **Tool/model and representative prompt:** Antigravity agent following `.ai/prompts/03-roles-vm-cicd-artifacts-alignment.md`.
- **Human review and significant decisions:**
  - Caught and resolved the 3-way role conflict (`user`/`admin` vs `master`/`client` vs `Passenger`/`Staff`) by adding a PostgreSQL data+schema migration with `CK_User_Role` (`ADR-003`), preventing silent DB drift.
  - Caught that `gitleaks/gitleaks-action@v2` requires a paid license on GitHub Organization repos and switched CI secret scanning to the open-source `ghcr.io/gitleaks/gitleaks:latest` CLI container.
  - Updated `CLAUDE.md` to drop unquoted `#` before requirement IDs because PowerShell treats `#` as a comment.

## Limitations and Reflection

- Task `ACC-T07` (HTTP integration test suite with Testcontainers PostgreSQL verifying live `CK_User_Role` enforcement and `403 Forbidden` on every `Staff` endpoint) is not yet implemented and must be completed before Production promotion.
- Having explicit PL6 artifacts (`requirements.md`, `design.md`, `tasks.md`, `review.md`) exposed three spec-vs-code defects (roles, required PII fields, Portuguese error text) that unit tests alone did not catch because the legacy tests asserted the wrong legacy behavior.
