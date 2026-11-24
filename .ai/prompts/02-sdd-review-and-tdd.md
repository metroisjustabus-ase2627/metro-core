# Prompt Template 02 — Specification Review & TDD Implementation (`PL6` / `PL7`)

- **Version:** 1.0
- **Phase:** `[review]` / `[test]` / `[code]`
- **Aligned with:** `PL6` Part 5, `PL7` Parts 2–5, `T09`

```text
[review][test][code] Review specification, establish Red test baseline, implement, and record validation for <feature-name> (<REQ-IDS>)

[ROLE / CONTEXT]
Act as a QA & Senior Software Engineer on MetroIsJustABus.
Read: .ai/steering/*, .ai/sdd/specs/<feature-name>/{requirements,design,tasks}.md, and existing test suites.

[TASK DEFINITION]
1. Specification Review (PL6): Create review.md identifying unsupported assumptions, ambiguities, missing failure cases, and spec/design/task inconsistencies; include a Finding/Decision/Justification table, a feature traceability matrix, and a 3-scenario testability challenge.
2. Executable Tests First (PL7 Red): Derive executable tests mapped to requirement IDs ([Trait("Requirement", "<REQ-ID>")] / tagged specs). Execute them before changing production code and record the exact failing baseline command and output.
3. Incremental Implementation (PL7 Green): Implement one task at a time strictly within scope. Never weaken assertions or alter acceptance criteria to force green tests.
4. Validation Record (PL7): Create validation.md containing scope/traceability table, exact commands executed (initial Red + final Green), observed output counts, human review decisions, and remaining limitations.

[CONSTRAINTS]
- Treat all AI-generated code and tests as unverified until executed and reviewed by a human (CON-PRC-01).
- Report test results only if actually executed; never fabricate command output.

[SUCCESS CRITERIA]
All unit/integration/lint/format checks pass with 0 warnings and full requirement traceability in review.md, validation.md, and docs/traceability.json.
```
