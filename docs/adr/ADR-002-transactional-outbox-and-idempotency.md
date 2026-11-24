# ADR-002 — Cross-Component Consistency via Transactional Outbox and Idempotent Consumers

- **Status:** Accepted
- **Date:** 2026-11-24
- **Traces to:** `NFR-CON-01`, `NFR-REL-02`, `NFR-REL-03`, `NFR-AV-01`, `FR-TKT-04`, `FR-VAL-04`, `FR-SUB-06`, `CON-ARC-05`

## Context

Operations such as registering disruptions (`FR-DIS-01`), modifying schedules (`FR-OPS-04`), acquiring tickets (`FR-TKT-03`), and validating tickets (`FR-VAL-01`) mutate core state and trigger cross-component side effects. Distributed failures, retries, or crashes between a database commit and a broker publish must never lose events or produce duplicate tickets, validations, or notifications.

## Decision

1. **Transactional Outbox in `metro-core`:** Core commands persist domain changes and an `OutboxMessage` record (carrying a unique `MessageId`, event type, schema version, `CorrelationId`, and JSON payload) inside the **same PostgreSQL transaction** (`IUnityOfWork`). A background publisher relays unpublished messages to RabbitMQ with at-least-once delivery.
2. **Idempotent Command Handling in `metro-core`:**
   - Ticket acquisition (`FR-TKT-04`): client-supplied `Idempotency-Key` enforced by a database unique constraint.
   - Ticket validation (`FR-VAL-04`): unique constraint on `(TicketId, ScheduledTripId)` ensuring at most one accepted validation and one journey consumed per trip.
3. **Idempotent Event Consumption in `metro-notifications`:** Every consumed AMQP message checks and records its `MessageId` inside a deduplication table (`ProcessedMessages`) transactionally with notification creation (`FR-SUB-06`, `NFR-REL-02`).

## Alternatives Considered

| Option | Atomic Commit + Publish | Tolerates Broker Outage | Operational Overhead | Verdict |
| --- | --- | --- | --- | --- |
| Dual write (DB commit then direct AMQP publish) | No — crash between steps loses events | Poor | Low | Rejected |
| Distributed two-phase commit (XA / 2PC) | Yes | Poor (blocks on unavailable participant) | High | Rejected |
| **Transactional Outbox + Idempotent Consumer** | **Yes (local ACID transaction + eventual convergence)** | **High (events queue in DB until broker returns)** | **Low/Moderate** | **Selected** |

## Consequences

- Consistency across `metro-core` and `metro-notifications` is eventually consistent, bounded by the `NFR-PERF-02` target (95% within 60 s when healthy).
- Automated failure-and-recovery tests must verify crash-between-commit-and-publish, duplicate redelivery, and broker outage recovery.
