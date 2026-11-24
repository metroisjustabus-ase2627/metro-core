# Specification Review — `accounts-access` (`metro-core`, PL6)

- **Feature folder:** `.ai/sdd/specs/accounts-access/`
- **Reviewed artifacts:** `requirements.md`, `design.md`, `tasks.md`, `docs/spec/requirements.md` v2, `.ai/steering/*`

## 1. Review Findings and Human Decisions

| Finding | Identified by (AI/Human) | Decision (accept/reject/defer) | Justification |
| --- | :---: | :---: | --- |
| **F1 — Role vocabulary conflict:** Database, `Program.cs` policies (`master`/`client`/`user`), `UserController`, validators, and both steering `conventions.md` files disagreed with `docs/spec/requirements.md` (`Passenger`/`Staff`), violating `T04B` Rule #1. | Human + AI | **Accept** | Enforced canonical `Passenger`/`Staff` via `UserRoles`, PostgreSQL `CK_User_Role` + migration `20261124120000_CanonicalUserRoles`, and updated steering (`ADR-003`). |
| **F2 — Over-constrained registration PII:** `CreateUserCommand` required `Name`, `Document`, and `Phone`, contradicting `FR-ACC-01` (*"e-mail and password"*) and §7 Out of Scope (*"Only the data needed for the functions"*). | AI | **Accept** | Made `Name`, `Document`, and `Phone` optional (default `""`) on commands and validators while preserving DB column compatibility. |
| **F3 — Language drift:** Validation, conflict, and password-recovery messages were in Portuguese while `docs/spec/requirements.md` §7 mandates an English interface. | AI | **Accept** | Standardized all Application/Domain/Swagger user-facing messages to English and updated `.ai/steering/conventions.md`. |
| **F4 — Undocumented starter template modules:** `PasswordRecoveryController` and `Metro.Infrastructure.File` exist in code without corresponding v2 FRs. | AI | **Defer** | Kept intact for now to avoid breaking existing passing tests; team will decide before Final Delivery whether to promote password recovery to a derived FR or remove unused file storage. |
| **F5 — E-mail uniqueness is case-sensitive:** `GetByEmail` and the unique index compare exactly, so `passenger@example.com` and `Passenger@Example.com` can become two accounts, against `FR-ACC-01` ("same e-mail"). | AI (Claude Opus 5.5) | **Proposed: accept** — pending team review | E-mail addresses are treated case-insensitively in practice; fix by storing lower-case (`ACC-T09`, `ACC-S5`). |
| **F6 — Concurrent duplicate registration returns 500:** the pre-check and the insert are not atomic; the second request hits the unique index and the exception is not mapped. | AI (Claude Opus 5.5) | **Proposed: accept** — pending team review | `FR-ACC-01` requires `409` for a duplicate e-mail in every case (`ACC-T10`). |
| **F7 — Hard delete of users:** `DELETE /api/v1/users/{id}` removes the row; once tickets and validations reference users this would break history (`FR-HIS-02`). | AI (Claude Opus 5.5) | **Defer** — team decision (Q-ACC-01) | Not urgent until the `tickets` feature exists (`ACC-T11`). |
| **F8 — Login, refresh and logout have no acceptance scenarios:** the spec covers registration and roles, but not the authentication flow that `FR-ACC-02` requires. | AI (Claude Opus 5.5) | **Proposed: accept** — pending team review | Added `ACC-S6` (identical failed-login response) and `ACC-S7` (refresh rotation), derived from existing behaviour without new business rules. |
| **F9 — Tests written after the code, unit-only:** the role changes were implemented first and verified with mocked unit tests; no failing baseline was recorded and HTTP/database behaviour is not exercised (`ACC-T07` pending). PL7 asks for reviewed tests run before implementation. | AI (Claude Opus 5.5) | **Proposed: accept** — pending team review | From now on, write integration tests first and record the baseline (`ACC-T08`); apply the same order to every new feature. |

## 2. Feature-Level Traceability Matrix

| Project source or documented assumption | Requirement ID | Design element | Task ID | Acceptance criterion |
| --- | --- | --- | --- | --- |
| `SRC-07` | `FR-ACC-01` | `CreateUserCommand`, `CreateUserCommandValidator`, `CreateUserHandler`, unique `User.Email` index | `ACC-T03`, `ACC-T05` | Minimal email+password creates active `Passenger`; duplicate email returns `409 Conflict` |
| `SRC-07`, `SRC-08`, `D-06`, `ADR-003` | `FR-ACC-02` | `UserRoles`, `UserMap` (`CK_User_Role`), `CanonicalUserRoles` migration, `TokenService`, `UserController.Create` | `ACC-T01`, `ACC-T02`, `ACC-T04`, `ACC-T05` | Only `Passenger` and `Staff` accepted; self-registration always forces `Passenger` |
| `SRC-04`, `SRC-07` | `FR-ACC-03` | `UserController.Me` (`[Authorize("Authenticated")]` + `ClaimTypes.Name`) | `ACC-T04` | Authenticated caller retrieves only own profile; future resource ownership enforced in feature specs |
| `SRC-07` | `NFR-SEC-01` | `PasswordPolicy`, `PasswordService` (PBKDF2-SHA256 salted hash) | `ACC-T01`, `ACC-T05` | Weak passwords rejected; stored password is salted hash |
| `SRC-08`, `ADR-003` | `NFR-SEC-02` | `Program.cs` `Staff` policy, `[Authorize("Staff")]` on `UserController` | `ACC-T04`, `ACC-T07` | All `Staff` endpoints refuse anonymous (`401`) and `Passenger` (`403`) requests |

## 3. Testability Challenge (PL6 Part 5 Step 4)

Target requirement: **`FR-ACC-02`** (canonical roles and privilege boundary) — 4 verifiable acceptance scenarios derived without inventing new business rules:

1. **Canonical role acceptance:** Validating `CreateUserCommand` with `Role = "Passenger"` or `Role = "Staff"` succeeds with 0 errors.
2. **Legacy/invalid role rejection:** Validating `CreateUserCommand` or `UpdateUserCommand` with `Role` in `{"master", "client", "admin", "user", "STAFF", ""}` fails with exact message `"Role must be Passenger or Staff."`.
3. **Self-registration privilege enforcement:** An unauthenticated or `Passenger` caller submitting `Role = "Staff"` to `POST /api/v1/users` is coerced to `Role = "Passenger"` before persistence.
4. **Staff-only authorization boundary (`NFR-SEC-02`):** Calling `GET /api/v1/users`, `GET /api/v1/users/{id}`, `PUT /api/v1/users/{id}`, or `DELETE /api/v1/users/{id}` with a `Passenger` JWT returns `403 Forbidden`.

## 4. Short AI-Use Record

- **Tool/model:** Antigravity agent (automated audit + SDD drafting).
- **Human verification:** Cross-checked professor feedback (`user/admin != master/client in DB`, `T04B` Rule #1, `PL6`/`PL7` structure) against actual source code, migrations, and unit tests before approving changes.
- **Second review (2026-11-24):** Claude Opus 5.5 (claude.ai agent) compared this spec with the source code and the PL6/PL7 handouts and proposed F5–F9, scenarios `ACC-S5..S7` and tasks `ACC-T08..T11`. These are proposals until a team member reviews them.
