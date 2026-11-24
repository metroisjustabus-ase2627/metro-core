# Implementation and Validation Tasks — `accounts-access` (`metro-core`)

- **Feature folder:** `.ai/sdd/specs/accounts-access/`
- **Traces to:** `FR-ACC-01`, `FR-ACC-02`, `FR-ACC-03`, `NFR-SEC-01`, `NFR-SEC-02`, `ADR-003`

| Task ID | Description | Requirement(s) | Area / Files | Depends On | Verifiable Completion Condition | Status |
| --- | --- | --- | --- | --- | --- | :---: |
| `ACC-T01` | Introduce canonical `UserRoles` (`Passenger`, `Staff`) in Domain and translate `PasswordPolicy` message to English | `FR-ACC-02`, `NFR-SEC-01` | `Metro.Domain/Users/UserRoles.cs`, `Auth/PasswordPolicy.cs` | — | `UserRolesTests` and `PasswordPolicyTests` pass | Done |
| `ACC-T02` | Enforce `CK_User_Role` in EF Core mapping, add data+schema migration `20261124120000_CanonicalUserRoles`, and update seed initializer (`SeedStaffUser`) | `FR-ACC-02`, `ADR-003` | `Metro.Infrastructure.PostgreSQL/` | `ACC-T01` | Migration compiles cleanly; snapshot contains `CK_User_Role`; no hardcoded seed password | Done |
| `ACC-T03` | Align commands, validators, and handlers to canonical roles, optional `Name`/`Document`/`Phone`, and English messages | `FR-ACC-01`, `FR-ACC-02` | `Metro.Application/Users/`, `Auth/` | `ACC-T01` | `CommandValidatorTests` and handler unit tests pass with `Passenger`/`Staff` | Done |
| `ACC-T04` | Replace legacy `master`/`client`/`user` policies in `Program.cs` and `UserController.cs` with `Staff`/`Passenger`/`Authenticated` and `SEED_STAFF_*` config | `FR-ACC-02`, `FR-ACC-03`, `NFR-SEC-02` | `Metro.Api/Program.cs`, `Controllers/UserController.cs`, compose/env files | `ACC-T01`, `ACC-T02`, `ACC-T03` | Solution builds with 0 warnings; no legacy role strings remain in `src/` | Done |
| `ACC-T05` | Update existing unit tests, add `UserRolesTests` + minimal registration test, and tag tests with `[Trait("Requirement", "...")]` | `FR-ACC-01..02`, `NFR-SEC-01..02`, `CON-PRC-03` | `tests/Metro.Domain.Tests/`, `tests/Metro.Application.Tests/` | `ACC-T01..04` | `dotnet test -c Release` passes 100% with requirement traits | Done |
| `ACC-T06` | Update steering (`conventions.md`) in both `metro-core` and `metro-web` and publish `ADR-003` | `CON-PRC-04`, `CON-ARC-05` | `.ai/steering/conventions.md`, `docs/adr/ADR-003*` | `ACC-T04` | Grep confirms 0 `master`/`client` role specs left in steering/code | Done |
| `ACC-T07` | Add HTTP integration test matrix hitting every Staff endpoint with a `Passenger` JWT (`403`) and anonymous call (`401`) against Testcontainers PostgreSQL | `NFR-SEC-02`, `CON-TST-01` | `tests/Metro.Integration.Tests/` | `ACC-T05` | Integration test suite green in CI/Dev VM | Pending |
