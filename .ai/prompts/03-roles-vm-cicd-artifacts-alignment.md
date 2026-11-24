# Prompt Template 03 — Canonical Roles, DEI VM Deployment, CI/CD/CDE Separation & SDD Artifacts

- **Version:** 1.0
- **Date:** 2026-11-24
- **Phase:** `[spec]` `[design]` `[code]` `[ci]` `[deploy]`
- **Traces to:** `FR-ACC-01`, `FR-ACC-02`, `NFR-SEC-01`, `NFR-SEC-02`, `CON-CI-01`, `CON-CI-02`, `CON-CI-04`, `CON-DEP-01`, `CON-DEP-02`, `CON-DEP-03`, `CON-DEP-05`, `CON-ARC-05`, `CON-PRC-02`, `CON-PRC-03`, `CON-PRC-04`

```text
[spec][code][ci][deploy] Align roles, VM deployment, CI/CDE/CD separation, and SDD/ADR artifacts across metro-core and metro-web

[ROLE / CONTEXT]
Act as a Senior Software Architect and DevOps Engineer on MetroIsJustABus (ASE MEI 2026/27).
Audit and resolve five engineering compliance issues raised during project review across metro-core and metro-web:
1. Canonical role alignment: resolve mismatch between database/code roles (master/client, policy user) and specification roles (Passenger/Staff, FR-ACC-02).
2. Containerized DEI VM deployment: remove GitHub Pages deployment from metro-web and enforce Build Only Once (T03) with immutable GHCR images deployed to Development and Production VMs.
3. Explicit CI vs CD vs CDE separation: split monolithic ci.yml workflows into dedicated CI (ci.yml), CD to Development VM (cd-dev.yml), and CDE to Production VM behind a manual approval gate (cd-prod.yml).
4. Engineering specifications and artifacts first: establish reviewed SDD feature artifacts (.ai/sdd/specs/accounts-access/), Architecture Decision Records (docs/adr/), and traceability (docs/traceability.json + requirement-tagged tests).
5. Separate prompt version control from specifications: store clean, ISO 29148-structured reusable prompt templates under .ai/prompts/ (T06) and audit logs/metrics under .ai/logs/ (CON-PRC-02), explicitly documenting that prompts are process inputs rather than product specifications (T04B).

[CONSTRAINTS]
- Specification prevails over code (T04B Rule #1): code, PostgreSQL CHECK constraints, EF Core migration, seed configuration, API policies, unit tests, and steering conventions must use Passenger and Staff exclusively.
- All user-facing API validation and conflict messages must be in English (docs/spec/requirements.md §7).
- Never rebuild application artifacts during deployment (Build Only Once); verify /health reports Healthy + exact commit SHA with automatic rollback on failure.
- Harden CODEOWNERS for CI/CD workflows, deployment configs, SDD artifacts, and agent rule/hook directories (.cursor/, .claude/).
- Validate every change via real build, test, formatting, type-check, and docker compose config executions before recording validation.md and committing.

[OUTPUT FORMAT]
- Updated source code, EF Core migration, requirement-tagged tests, Docker/Nginx/Compose VM configs, separated CI/CD/CDE workflows, ADRs, SDD feature folder, traceability.json, reusable prompt templates, and regenerated .ai/logs/summary.md.
- Clean Conventional Commits referencing requirement IDs in both repositories.

[SUCCESS CRITERIA]
- metro-core: dotnet build -c Release (0 warnings), dotnet test -c Release (100% pass), dotnet format whitespace --verify-no-changes (exit 0), 0 NuGet vulnerabilities, docker compose config valid for local/dev/prod.
- metro-web: npm run build, npm run test:ci, npm run format:check, npx tsc --noEmit, npm audit --omit=dev --audit-level=high (0 vulns), 0 GitHub Pages references, docker compose config valid for local/dev/prod.
- Zero legacy master/client role definitions left in code, steering, or tests.
```
