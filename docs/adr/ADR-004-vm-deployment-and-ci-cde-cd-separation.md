# ADR-004 — DEI Virtual Machine Deployment (No GitHub Pages) and CI / CD / CDE Separation

- **Status:** Accepted
- **Date:** 2026-11-24
- **Traces to:** `CON-DEP-01`, `CON-DEP-02`, `CON-DEP-03`, `CON-DEP-04`, `CON-DEP-05`, `CON-CI-01`, `CON-CI-02`, `CON-CI-03`, `CON-CI-04`, `NFR-OBS-01`

## Context

Previously:
1. `metro-web` rebuilt the Angular bundle with `--base-href "/metro-web/"` and deployed statically to GitHub Pages, violating **Build Only Once (`T03`)**, container-only deployment (`CON-DEP-01`), and VM-hosted dev/prod separation (`CON-DEP-02`, `CON-DEP-03`).
2. Both repositories placed CI and deployment jobs inside a single workflow named `CI`, blurring the distinction between Continuous Integration (CI), Continuous Delivery (CDE), and Continuous Deployment (CD).

## Decision

1. **Remove GitHub Pages completely:** All application components (`metro-web`, `metro-core`, `metro-notifications`, `PostgreSQL`, `RabbitMQ`) deploy exclusively as Docker containers on two **DEI Virtual Machines** (`Development` and `Production`, requested via `my.dei.uc.pt`), provisioned reproducibly via `metro-core/infra/bootstrap-vm.sh` and Docker Compose files.
2. **Enforce Build Only Once (`T03`):**
   - CI builds each container image **once** and publishes `ghcr.io/metroisjustabus-ase2627/<service>:<commit-sha>` with `APP_VERSION=<commit-sha>` exposed on `GET /health`.
   - `metro-web` configures its upstream API at container startup via Nginx `envsubst` (`API_UPSTREAM`), allowing the exact same immutable image to run locally, on Dev VM, and on Prod VM without recompilation.
3. **Separate CI, CD (Development), and CDE (Production) into distinct workflows (`T03`):**
   - **CI (`.github/workflows/ci.yml`):** Automated build, unit tests + coverage, formatting, static analysis, SCA vulnerability check (blocking), Gitleaks secret scan (`CON-DEP-05`), and packaging of the immutable GHCR image.
   - **CD — Development VM (`.github/workflows/cd-dev.yml`):** Automatically deploys the exact `<commit-sha>` image to the DEI Dev VM after green CI on `main` (via self-hosted runner `CON-DEP-03` or SSH fallback), validates `/health` (`Healthy` + exact SHA), and automatically rolls back on failure.
   - **CDE — Production VM (`.github/workflows/cd-prod.yml`):** Controlled promotion (`workflow_dispatch` + GitHub `production` environment **manual approval gate** — human-in-the-loop per `T03` CDE) deploying the exact Dev-verified `<commit-sha>` image to the DEI Production VM, followed by `/health` + SHA verification and automatic rollback (`CON-CI-04`).

## Alternatives Considered

| Option | Build Only Once | Meets `CON-DEP-01..03` | Separation of CI / CDE / CD | Verdict |
| --- | --- | --- | --- | --- |
| GitHub Pages for `metro-web` | Violated (rebuilds bundle) | Violated (no container/VM/health SHA) | Blurred | Rejected |
| Full auto-CD straight to Production without approval | Yes | Yes | High risk before full E2E/load suite matures | Deferred (use CDE now) |
| **DEI VMs (Dev + Prod) + Self-Hosted Runner + Separated CI / CD-Dev / CDE-Prod** | **Strict** | **Full** | **Explicit and auditable** | **Selected** |

## Consequences

- GitHub Actions self-hosted runners on DEI VMs connect outbound over HTTPS (443), enabling automated VM deployments behind the DEI firewall without exposing public SSH.
- Production promotion requires configuring `production` environment required reviewers in GitHub repository settings.
