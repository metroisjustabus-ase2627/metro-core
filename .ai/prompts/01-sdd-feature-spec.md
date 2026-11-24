# Prompt Template 01 — Feature SDD Specification (`PL6`)

- **Version:** 1.0
- **Phase:** `[spec]` / `[design]`
- **Aligned with:** ISO/IEC/IEEE 29148 (`T06`/`T07`), SDD (`T04B`, `PL6`)

```text
[spec] Create reviewed SDD feature specification for <feature-name> (<REQ-IDS>)

[ROLE / CONTEXT]
Act as a senior software engineer applying Specification-Driven Development (SDD) on MetroIsJustABus.
Read first: AGENTS.md, .ai/steering/*, docs/spec/requirements.md, and docs/adr/*.

[TASK DEFINITION]
Create .ai/sdd/specs/<feature-name>/ containing:
1. requirements.md — bounded scope, EARS requirements with Origin tags (S/D/T), Given/When/Then acceptance scenarios, assumptions, and open questions.
2. design.md — component responsibilities, domain entities/data structures, API/event contracts, sequence/state diagram, design trade-offs, and failure/security/consistency handling.
3. tasks.md — ordered atomic implementation and validation tasks with unique IDs, requirement traceability, dependencies, and verifiable completion criteria.

[CONSTRAINTS]
- Do not invent business rules or present AI assumptions as stakeholder facts.
- Flag any unresolved open question (Q-01..Q-04) explicitly instead of guessing.
- Respect Clean Architecture (metro-core) / standalone Signals-first client-only architecture (metro-web).
- Canonical roles are strictly Passenger and Staff (ADR-003). All user-facing text is in English.
- Do not write implementation code yet (PL6 separation).

[OUTPUT FORMAT]
Structured Markdown files under .ai/sdd/specs/<feature-name>/ plus a concise list of ambiguities requiring human confirmation.

[SUCCESS CRITERIA]
Every requirement maps to a statement source or team decision, has at least one testable Given/When/Then scenario, and every design element serves a traced requirement.
```
