# Reusable Prompt Templates (Prompt Version Control — Lecture T06)

> **Important distinction (`T04B` slide 13, `ADR-005`):**
> **Prompts are NOT engineering specifications or deliverables.**
> These files are version-controlled, ISO/IEC/IEEE 29148-structured **process templates** used to invoke LLMs/agents reproducibly (`T06` slide 34).
>
> Authoritative engineering artifacts live in:
> - `docs/spec/requirements.md` and `.ai/steering/` (system-level specifications & steering)
> - `.ai/sdd/specs/<feature>/{requirements,design,tasks,review,validation}.md` (feature-level SDD specs & validation evidence)
> - `docs/adr/` (Architecture Decision Records)
> - `docs/traceability.json` and requirement-tagged executable tests

## Templates

| File | Lifecycle Phase | Purpose |
| --- | --- | --- |
| `01-sdd-feature-spec.md` | Specify & Plan (`PL6`) | Draft feature `requirements.md`, `design.md`, and `tasks.md` grounded in `docs/spec/requirements.md` and `.ai/steering/` |
| `02-sdd-review-and-tdd.md` | Review & Validate (`PL6`/`PL7`) | Run AI specification review (`review.md`), generate Red-baseline tests, and record executable verification (`validation.md`) |
| `03-roles-vm-cicd-artifacts-alignment.md` | Refactor / Governance | Structured specification-alignment prompt (canonical `Passenger`/`Staff` roles, VM deployment replacing GitHub Pages, CI/CD/CDE separation, and SDD/ADR artifact creation) |

## Usage Rules

1. Never paste secrets or PII into prompts (`CON-DEP-05`).
2. Keep prompts declarative, concise, and free of conversational filler.
3. Always record the human decision after every significant AI task:
   `node .ai/logs/tools/review.mjs <accepted|modified|rejected> "note" <REQ-ID> --minutes <N> --defects <N>`
