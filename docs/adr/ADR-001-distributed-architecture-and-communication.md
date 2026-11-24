# ADR-001 — Distributed Architecture and Communication Mechanisms

- **Status:** Accepted
- **Date:** 2026-11-24
- **Traces to:** `CON-ARC-01`, `CON-ARC-02`, `CON-ARC-03`, `CON-ARC-05`, `NFR-AV-01`, `NFR-REL-01`

## Context

MetroIsJustABus must be implemented as a distributed, multi-component system (`CON-ARC-01`). A single REST monolith exposing all functionality is explicitly prohibited. Furthermore, temporary failures of supporting capabilities such as notifications must not prevent passengers from consulting lines, stops, schedules, or validating tickets (`NFR-AV-01`).

## Decision

Decompose the system across independent deployable container boundaries:
1. **`metro-web` (Angular 22 + Nginx):** Single role-based SPA for visitors, passengers, and operational staff. Communicates synchronously with `metro-core` over HTTP/JSON (`/api/v1/...`). Never connects directly to PostgreSQL or RabbitMQ.
2. **`metro-core` (.NET 10, Clean Architecture + CQRS):** Authoritative domain service owning network, schedules, disruptions, ticket types, tickets, validations, occupancy, accounts, and history, backed by PostgreSQL 16.
3. **`metro-notifications` (.NET 10, separate process and container):** Independent notification boundary consuming domain events from RabbitMQ (AMQP), resolving affected subscribers, and delivering e-mail/in-app notifications idempotently.
4. **Supporting infrastructure containers:** `PostgreSQL 16` (persistent storage) and `RabbitMQ` (asynchronous durable message broker).

## Alternatives Considered (Comparative Analysis)

| Option | Fault Isolation (`NFR-AV-01`) | Complexity | Latency / Coupling | Verdict |
| --- | --- | --- | --- | --- |
| Single monolithic REST API | Fails (`CON-ARC-01` violation) | Low | Tight | Rejected |
| Synchronous HTTP calls `core -> notifications` | Poor: outage or latency spikes block core writes | Low | Tight temporal coupling | Rejected |
| **Hybrid: Sync HTTP (`web -> core`) + Async AMQP (`core -> RabbitMQ -> notifications`)** | **High: core writes succeed even when broker/notifications are down** | **Moderate** | **Decoupled across boundaries** | **Selected** |

## Consequences

- `metro-core` must persist outgoing domain events reliably (see `ADR-002`) so broker downtime causes zero event loss (`NFR-REL-03`).
- Every HTTP and AMQP interface must carry versioned schemas (`CON-ARC-04`) and a propagated `Correlation-Id` (`NFR-OBS-02`).
