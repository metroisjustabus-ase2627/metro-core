# ADR-005 — Engineering Specifications and Artifacts vs. Prompts and Audit Logs

- **Status:** Accepted
- **Date:** 2026-11-24
- **Traces to:** `CON-PRC-01`, `CON-PRC-02`, `CON-PRC-03`, `CON-PRC-04`, Lectures `T04B`, `T06`, `T07`, `T09`, Labs `PL5`–`PL7`

## Context

In Automated Software Engineering, the primary evaluation focus is **how the software is engineered, specified, verified, and measured** (Process & Agentic Engineering 30%, Measurement & Critical Analysis 30%, Reporting & Demonstration 10%, plus design/testing/CI/CD inside Technical Implementation 30%). A recurring pitfall is treating conversational prompts or raw chat logs as engineering specifications or deliverables. As taught in `T04B` (slides 7 and 13): *"A specification is more than a detailed prompt because the team reviews, stores, and maintains it."*

## Decision

Enforce a strict separation between **Engineering Artifacts** and **Process Inputs / Telemetry**:

| Category | Repository Location | Role | Authority |
| --- | --- | --- | --- |
| **System & Steering Specifications** | `docs/spec/requirements.md`, `.ai/steering/*` | Shared source of truth for humans and agents (`PL5`, `T04B`) | **Authoritative artifact** |
| **Feature SDD Specifications & Validation** | `.ai/sdd/specs/<feature>/{requirements,design,tasks,review,validation}.md` | Bounded feature specs, design, tasks, AI review decisions, traceability, and real Red→Green test evidence (`PL6`, `PL7`) | **Authoritative artifact** |
| **Architecture Decisions & Traceability** | `docs/adr/*.md`, `docs/architecture/*`, `docs/traceability.json` | Justified trade-offs, system models, and end-to-end traceability (`CON-ARC-05`, `CON-PRC-03`) | **Authoritative artifact** |
| **Reusable Prompt Templates** | `.ai/prompts/*.md` | Standardized, ISO 29148-structured prompt templates for reproducible agent invocation (`T06` slide 34 — Prompt Version Control) | **Process tooling (NOT product specs)** |
| **AI Usage Audit Logs & Metrics** | `.ai/logs/sessions/*.jsonl`, `.ai/logs/summary.md` | Empirical telemetry for effort, autonomy, rework, and defect measurement (`CON-PRC-02`) | **Process evidence (NOT product specs)** |

## Consequences

- No feature implementation may be merged without reviewed SDD artifacts (`requirements.md`, `design.md`, `tasks.md`, `review.md`, `validation.md`) and requirement-tagged executable tests.
- Reusable prompts in `.ai/prompts/` must stay clean, structured, and free of conversational filler, referencing authoritative specs rather than duplicating requirements.
- Raw AI claims of success are never accepted as validation evidence; `validation.md` must record real executed commands and observed outputs (`PL7`).
