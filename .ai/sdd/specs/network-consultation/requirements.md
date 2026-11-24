# Feature Specification — `network-consultation` (`metro-core`)

- **Feature folder:** `.ai/sdd/specs/network-consultation/`
- **Parent system spec:** `docs/spec/requirements.md` v2 (`FR-NET-01`, `FR-NET-02`, `FR-NET-03`; decisions `D-04`, `A-01`, `A-04`)
- **Related ADRs:** `docs/adr/ADR-001-distributed-architecture-and-communication.md`
- **Status:** Draft (PL6) — proposed 2026-11-24, pending team review on 29 Nov. `design.md` and `tasks.md` follow after review.

## 1. Scope and Boundaries

Visitors and passengers read the bus network: lines, the ordered stops of each line, and the trips planned for a given date with their expected times. This feature is **read-only**.

- **In scope:** list lines with their ordered stops (`FR-NET-01`); trips of a line or of a stop on a date (`FR-NET-02`); detail of one trip (`FR-NET-03`). No login needed (`D-04`).
- **Out of scope:**
  - Creating or editing lines, stops and schedules → `network-maintenance` (`FR-OPS-01..03`).
  - Showing disruptions on the results → `disruptions` (`FR-NET-04`). This feature must leave room for it but does not implement it.
  - Occupancy shown to passengers → `ticket-validation-occupancy` (`FR-OCC-03`).
  - Journey planning, transfers, real-time positions (`docs/spec/requirements.md` §7).
  - Old schedule versions for past dates → `history-reporting` (`FR-HIS-03`).

**Why kept small:** every other feature (disruptions, tickets, validation, notifications) refers to lines, stops and trips. A simple, stable read model here makes the rest easier to build and to explain.

## 2. Domain terms used here

From the glossary in `docs/spec/requirements.md` §3: **Stop**, **Line** (ordered sequence of at least two stops), **Service day type**, **Schedule** (planned trips of a line for a service day type), **Scheduled trip** (one planned run of a line on a given date, with an expected time at each stop).

## 3. Requirements (EARS + Origin)

| ID | Statement (EARS) | Source | Origin | Pri |
| --- | --- | --- | :---: | :---: |
| `FR-NET-01` | The system shall let any caller, with or without an account, list the active lines and, for each line, its active stops in the configured order. | SRC-01, SRC-02 | S + T (D-04) | Must |
| `FR-NET-01.1` | The system shall not list lines or stops that staff have deactivated. | SRC-08 | D | Must |
| `FR-NET-02` | When a caller selects a line and a date, the system shall return the scheduled trips of that line running on that date, each with its expected time at every stop, ordered by departure time. | SRC-01, SRC-02 | S | Must |
| `FR-NET-02.1` | When a caller selects a stop and a date, the system shall return every scheduled trip that serves that stop on that date, with the line and the expected time at that stop, ordered by that time. | SRC-02 | S | Must |
| `FR-NET-02.2` | The system shall decide which schedule applies to a date from its service day type: weekday (Monday–Friday), Saturday, or Sunday. | SRC-01 | D + T (A-01, NC-D2) | Must |
| `FR-NET-03` | The system shall let any caller consult one scheduled trip with its line, date and expected time at every stop, in stop order. | SRC-02 | S | Must |
| `FR-NET-03.1` | If the requested line, stop or trip does not exist or is inactive, then the system shall return `404 Not Found`; if the date is not a valid calendar date, then it shall return `400 Bad Request`. | — | D | Must |

## 4. Acceptance Scenarios (Given / When / Then)

Test data for all scenarios (team decision NC-D3): line **L1** with stops **A → B → C**; line **L2** with stops **D → B → E**; line **L3** deactivated. Weekday schedule of L1: trips at 08:00 and 09:00 (A 08:00, B 08:10, C 08:25). Sunday schedule of L1: one trip at 10:00. Weekday schedule of L2: trip at 08:05 (D 08:05, B 08:15, E 08:30).

```gherkin
Scenario: NET-S1 — Anyone can list lines with ordered stops (FR-NET-01, FR-NET-01.1)
  Given the test network
  When an anonymous caller sends GET /api/v1/lines
  Then the response is 200 OK with lines L1 and L2 only
  And the stops of L1 are A, B, C in this order

Scenario: NET-S2 — Trips of a line depend on the day type (FR-NET-02, FR-NET-02.2)
  Given the test network
  When an anonymous caller asks for the trips of L1 on Monday 2026-11-30
  Then the response contains 2 trips, departing 08:00 and 09:00, each with times at A, B, C
  When the caller asks for the trips of L1 on Sunday 2026-11-29
  Then the response contains 1 trip, departing 10:00

Scenario: NET-S3 — Trips through a stop come from all lines (FR-NET-02.1)
  Given the test network
  When an anonymous caller asks for the trips at stop B on Monday 2026-11-30
  Then the response lists, in this order: L1 at 08:10, L2 at 08:15, L1 at 09:10

Scenario: NET-S4 — Trip detail (FR-NET-03)
  Given the 08:00 weekday trip of L1 on Monday 2026-11-30
  When an anonymous caller requests its detail
  Then the response shows line L1, date 2026-11-30 and stops A 08:00, B 08:10, C 08:25

Scenario: NET-S5 — Unknown or deactivated items (FR-NET-03.1)
  When an anonymous caller asks for the trips of L3 on 2026-11-30
  Then the response is 404 Not Found
  When the caller asks for the trips of L1 on date "2026-02-30"
  Then the response is 400 Bad Request
```

## 5. Assumptions, Decisions and Open Questions

| ID | Type | Statement | Reason | Status |
| --- | --- | --- | --- | --- |
| A-04 | Assumption | All times are local time Europe/Lisbon. | System-level assumption. | From v2 |
| NC-A1 | Assumption | A trip starts and ends on the same day (no trips past midnight). | Keeps date logic simple; the statement does not mention night services. | Proposed |
| NC-D1 | Decision | A scheduled trip is **not stored per date**. It is computed when read: (schedule trip) + (date). Its ID is the schedule-trip ID plus the date, e.g. `{tripId}/2026-11-30`. | No daily job to generate trips; one place for the timetable. Other features (disruptions, validation, occupancy) refer to a trip by this pair. **Affects other features: decide together.** | Proposed — to confirm 29 Nov |
| NC-D2 | Decision | Service day types are only weekday, Saturday and Sunday. Holidays are treated as the weekday they fall on. | v2 `A-01` mentions holidays, but a holiday calendar adds data and rules no other requirement needs. Can be added later as a disruption ("period" scope). | Proposed — to confirm 29 Nov |
| NC-D3 | Decision | A small fixed test network (above) is loaded by the integration tests, not by the production database. | Tests must not depend on data entered by hand; production data comes from `network-maintenance`. | Proposed |
| NC-Q1 | Question | Do we need consultation by stop (`FR-NET-02.1`) in the first version, or only by line? | The statement says "consult the network, lines, stops, schedules"; by-stop is the natural reading, but it doubles the query work. | Open |
| NC-Q2 | Question | Should the API answer for any date, or only from today up to N days ahead? | Past dates touch `FR-HIS-03` (schedule versions). Proposed: today and future dates only; past dates return 400. | Open |
