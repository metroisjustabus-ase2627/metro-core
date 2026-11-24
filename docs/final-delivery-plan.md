# Final Delivery Plan — Backend (`metro-core`) (due December 11)

Backend counterpart of [`metro-web/docs/final-delivery-plan.md`](../../metro-web/docs/final-delivery-plan.md). It uses the **same four owners (A, B, C, D)**, so every backend task is linked to the frontend task of the same person. Each person owns one vertical slice (web + core) and part of the report. Requirement IDs refer to [`docs/spec/requirements.md`](spec/requirements.md).

> Rule for every task: spec first (`.ai/sdd/specs/<feature>/`), then code, tests and an AI log entry in `.ai/logs/` (CON-PRC-02, CON-PRC-04). Follow the Clean Architecture module pattern in `.ai/steering/conventions.md`.

## 1. Current state of `metro-core`

| Area | Status | Notes |
|---|---|---|
| Auth: login, refresh token, password recovery | Done | `LoginController`, `PasswordRecoveryController` (FR-ACC-02, NFR-SEC-01 partly) |
| Users: create / get / update | Done | `UserController`; canonical roles `Passenger` / `Staff` enforced with DB `CHECK` constraint (`FR-ACC-02`, `ADR-003`) |
| Health endpoint | Done | `/health` reports PostgreSQL dependency and deployed commit SHA (`APP_VERSION`, `NFR-OBS-01`, `CON-CI-02`) |
| Docker image + compose (local, dev, prod) | Done | `docker-compose.{yml,dev.yml,prod.yml}` + `infra/bootstrap-vm.sh`; RabbitMQ and notifications service still to add |
| CI / CD-Dev / CDE-Prod workflows | Done (base) | Split into `ci.yml`, `cd-dev.yml`, `cd-prod.yml` with Gitleaks + rollback (`ADR-004`); integration/system test jobs & DEI VMs pending |
| Network, disruptions, tickets, validation, occupancy, subscriptions, history | Not started | Split below |

## 2. Split by person

### Person A: Network, operations and history
- **Backend modules:** `Network` (stops, lines, ordered stops), `Schedules` (service day types, trips, times per stop), public consultation queries, history queries.
- **Features:** FR-NET-01 to 04; FR-OPS-01 to 04; FR-HIS-01 to 03 (also FR-OCC-04, capacity per line and trip override, with B).
- **Backend tasks:**
  1. Domain entities and repositories for Stop, Line, Schedule, ScheduledTrip; EF migration.
  2. Staff commands: create/edit/deactivate stops and lines, define schedules, with FluentValidation rules of FR-OPS-03 (at least 2 stops, times increasing).
  3. Public queries (no login, D-04): lines with ordered stops, trips by line/stop and date, trip detail (Dapper read side).
  4. Schedule versioning so past dates keep the old schedule (FR-HIS-03).
  5. `NetworkChanged` event on changes that affect future trips (FR-OPS-04), written to the outbox (see shared work).
  6. Staff history queries by period (FR-HIS-01/02, with data from B and C).
  7. OpenAPI contract and unit + contract tests; staff endpoints return 403 for passengers (NFR-SEC-02).
- **Frontend link:** consultation screens (lines, stops, timetables), staff line/schedule forms, staff history screens consume these endpoints.
- **Report:** design and data model (5.2).

### Person B: Tickets, validation and occupancy
- **Backend modules:** `TicketTypes`, `Tickets`, `Validations`, `Occupancy`; hardening of existing `Auth`/`Users`.
- **Features:** FR-TKT-01 to 06; FR-VAL-01 to 04; FR-OCC-01 to 04; FR-ACC-01 to 03.
- **Backend tasks:**
  1. Confirm roles Passenger / Staff: self-registration always creates Passenger, staff created only by staff, `[Authorize]` policies per role (FR-ACC-01 to 03, NFR-SEC-01/02).
  2. Ticket types: staff CRUD and withdrawal; list types on sale (FR-TKT-01/02).
  3. Ticket acquisition with simulated payment and an idempotency key (request ID, unique constraint) so repeats return the same ticket (FR-TKT-03/04).
  4. Ticket states Active / Exhausted / Expired and passenger "my tickets" query (FR-TKT-05/06); ownership check (FR-ACC-03).
  5. Validator endpoint (ticket ID, trip ID, time): the 4 acceptance conditions, rejection reason codes, one validation per ticket + trip, one journey consumed (FR-VAL-01 to 04). Needs trips from A and cancelled trips from C.
  6. Occupancy: increment on accepted validation in the same transaction; staff exact numbers, passenger level Low/Medium/High/Full (FR-OCC-01 to 03).
  7. Tests: the 4 cases of FR-VAL-01, idempotency (FR-TKT-04, FR-VAL-04), concurrency on duplicate validation, NFR-SEC authorization tests.
- **Frontend link:** passenger ticket purchase and wallet, simulated validator screen, staff ticket-type and occupancy screens, login/register screens.
- **Report:** testing and verification strategy (5.5), including validation of AI-generated code.

### Person C: Disruptions and notifications
- **Backend modules:** `Disruptions` in core; new `src/Notifications/Metro.Notifications` (separate host, own Dockerfile and compose service); messaging contracts project; RabbitMQ container.
- **Features:** FR-DIS-01 to 03; FR-SUB-01 to 06; CON-ARC-02 to 04; NFR-AV-01, NFR-REL-02/03, NFR-CON-01, NFR-PERF-02.
- **Backend tasks:**
  1. Close open question Q-02 (who owns subscriptions) and write the decision record (CON-ARC-05) before coding.
  2. Disruptions: staff register/update/close with type, scope, interval, delay minutes (FR-DIS-01 to 03); expose active disruptions to A's consultation (FR-NET-04) and cancelled trips to B's validation.
  3. Transactional outbox in core and publisher to RabbitMQ; events defined by schema (NFR-CON-01, CON-ARC-04). Shared with A (`NetworkChanged`).
  4. `Metro.Notifications`: consume events idempotently by message ID, resolve affected subscribers, notify via e-mail and in-app, deduplicate (FR-SUB-04/06, NFR-REL-02).
  5. Subscription and preference APIs; list/read notifications (FR-SUB-01/02/03/05).
  6. Failure and recovery tests: core keeps working with broker or notifications down; pending events processed after restart (NFR-AV-01, NFR-REL-03).
- **Frontend link:** staff disruption screens, passenger subscription and notification screens, degraded-mode messages (NFR-AV-02).
- **Report:** architecture, communication and decision records (5.2, CON-ARC-05).

### Person D: CI/CD, infrastructure and evidence
- **Pipeline (CON-CI-01 to 04), builds on branch `feature/ci-tests-quality`:**
  1. Merge the build + tests + quality + image pipeline to `main` (done on branch).
  2. Deploy to development from the pipeline; self-hosted runner in dev (CON-DEP-03).
  3. Integration and system tests job using compose (API, PostgreSQL, RabbitMQ, notifications); contract tests for OpenAPI (CON-TST-01).
  4. Production gate, health and smoke tests after deploy, automatic rollback, commit → image → deployed version visible on `/health` (CON-CI-02 to 04).
  5. Secret scan and dependency update: `System.Security.Cryptography.Pkcs` 10.0.0 is reported vulnerable (NU1902/NU1903).
- **Infrastructure (CON-DEP-01 to 05, NFR-OBS):** dev and prod compose files, deployment strategy, secrets, volumes, networks; health with dependencies; structured logs with correlation ID; metrics, dashboard and one alert.
- **AI evidence:** `.ai/logs/` completeness (CON-PRC-02); metrics (effort, rework, CI failures).
- **Frontend link:** same pipeline for `metro-web` (build, tests, image, deploy), plus end-to-end tests against dev.
- **Report:** CI/CD, deployment and operation (5.6); evidence and critical analysis (5.7).

## 3. Dependencies between people

| Needs | From → To | Why |
|---|---|---|
| Trips and stops | A → B | Validation and occupancy refer to scheduled trips |
| Cancelled trips / active disruptions | C → A, C → B | FR-NET-04 display; FR-VAL-01 rejects cancelled trips |
| Outbox + event schemas | C → A, C → B | `NetworkChanged`, disruption and ticket events |
| Validations and tickets data | B → A | Staff history (FR-HIS-01/02) |
| Compose with RabbitMQ and notifications | C → D | System tests and deployment |
| Health endpoint, version, logs | D → everyone | Gates and monitoring |

Suggested order: accounts hardening (B) and CI merge (D) → network + ticket types (A, B) → disruptions + outbox (C) → validation/occupancy (B) → notifications (C) → history (A) → system tests and production (D).

## 4. Shared deliverables

Same as the frontend plan (problem analysis Dec 3, traceability Dec 5, AI process Dec 6, contributions Dec 8, video Dec 9, production and repo review Dec 10, evaluation Dec 11). Backend-specific:

| Deliverable | Who | Due |
|---|---|---|
| SDD spec folder per feature in `.ai/sdd/specs/` | each person for their features | before coding each feature |
| OpenAPI + event schemas published and versioned | A, B, C for their APIs; D checks in the pipeline | Nov 27 (first version) |
| Decision records (communication, consistency, deployment) | C (communication, consistency), D (deployment) | Dec 4 |
| `requirements.md` and `metro-is-just-a-bus.md` kept identical in both repos | whoever edits them | on every change |

## 5. Milestones

- **Nov 27:** base features per slice working locally; dev environment and pipeline deploy ready; OpenAPI v1 and first event schemas.
- **Dec 4:** full integration, failure scenarios, production deploy.
- **Dec 5 to 8:** report.
- **Dec 9 to 11:** video and wrap-up.

## 6. Risks

- Person D is overloaded (as in the frontend plan): move production infrastructure (CON-DEP) to B, or have C help with system tests.
- C is on the critical path (outbox, broker, disruptions feed A and B): start with the outbox and schemas first.
- Open questions Q-01 to Q-04 in `requirements.md` block FR-VAL-01, FR-SUB-04 and FR-OPS-04: decide them in the first week.
- Principles: no single REST process with everything, notifications in a separate container, writes with EF Core + UoW, reads with Dapper (`.ai/steering/principles.md`).
