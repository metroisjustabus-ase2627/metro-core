# ADR-003 — Canonical Roles (`Passenger` / `Staff`) Across Spec, API, JWT, and Database

- **Status:** Accepted
- **Date:** 2026-11-24
- **Traces to:** `FR-ACC-01`, `FR-ACC-02`, `FR-ACC-03`, `NFR-SEC-02`, `D-06`, `CON-PRC-04`

## Context

An audit of `metro-core` and `metro-web` revealed a three-way vocabulary conflict violating SDD Rule #1 (`T04B`: *"When code and specification disagree, the code must change"*):
- System requirements (`docs/spec/requirements.md`, `FR-ACC-02`, `D-06`) specified roles **`Passenger`** and **`Staff`**.
- Database rows, validators, controllers, and steering files inherited starter-template role names **`master`** and **`client`** (with an ASP.NET policy named **`user`** matching both), while informal discussions referred to `user` and `admin`.
- The PostgreSQL `"User"."Role"` column had no `CHECK` constraint, allowing invalid role strings to persist unchecked.

Such mismatches cause AI agents and developers to generate mismatched JWT claims, broken authorization policies, and inconsistent frontend route guards.

## Decision

Standardize on **one canonical ubiquitous role vocabulary** matching `docs/spec/requirements.md` across every layer of both repositories:
1. **Domain (`Metro.Domain.Users.UserRoles`):** `Passenger` and `Staff` (no magic strings; reject `master`, `client`, `admin`, `user`, or wrong casing).
2. **Database (PostgreSQL):** EF Core migration `20261124120000_CanonicalUserRoles` migrates existing rows (`master/admin -> Staff`, `client/user -> Passenger`) and enforces `CHECK ("Role" IN ('Passenger', 'Staff'))`.
3. **API & JWT (`Metro.Api`):** JWT `ClaimTypes.Role` emits `Passenger` or `Staff`; authorization policies are named `Staff`, `Passenger`, and `Authenticated`; self-registration always forces `Passenger` unless caller holds `Staff`.
4. **Frontend (`metro-web`):** Route guards and UI role checks use `Passenger` and `Staff` directly without translation tables.
5. **Language alignment (`requirements.md` §7):** User-facing validation and conflict messages in `metro-core` use English.

## Alternatives Considered

| Option | Consistency with Spec | Risk of Agent Hallucination | Migration Effort | Verdict |
| --- | --- | --- | --- | --- |
| Keep `master`/`client` in DB and map in UI | Violates `T04B` Rule #1 | High (agents mix names across layers) | None | Rejected |
| Rename spec to `user`/`admin` | Conflicts with transit domain (`Passenger`/`Staff` in statement) | Medium | Medium | Rejected |
| **Enforce canonical `Passenger`/`Staff` in Domain, API, JWT, DB (`CHECK`), and UI** | **100% aligned** | **Minimal** | **Low (1 migration + refactor)** | **Selected** |

## Consequences

- Existing development databases migrate automatically on startup via `context.Database.Migrate()`.
- Environment variables for optional initial seed are renamed to `SEED_STAFF_EMAIL` and `SEED_STAFF_PASSWORD`.
- Every staff endpoint must be verified by automated tests confirming `403 Forbidden` for `Passenger` tokens (`NFR-SEC-02`).
