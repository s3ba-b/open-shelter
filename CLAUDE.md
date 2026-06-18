# CLAUDE.md

## What this project is

Open Shelter (OSMP) — a multi-tenant SaaS platform for animal shelters and rescue
organizations (animals, intake, adoptions, fostering, medical scheduling), built
on .NET 10 / .NET Aspire to demonstrate distributed-systems architecture and
multi-tenant data isolation. The milestone breakdown lives in
[ROADMAP.md](ROADMAP.md) in this repo. Full objectives, success measures,
constraints, and licensing rationale live in the project charter this repo was
bootstrapped from (kept in the `project-ideas` idea-backlog repo, not here).

## Stack

- .NET 10, orchestrated with .NET Aspire — `src/OpenShelter.AppHost` is the
  orchestration entry point, `src/OpenShelter.ServiceDefaults` carries shared
  OpenTelemetry/health/resilience wiring used by every service.
- PostgreSQL, Redis, and a message broker (RabbitMQ / Azure Service Bus) as
  Aspire-orchestrated backing resources (added as services land).
- Docker Compose is the primary deployment target; Azure Container Apps (`azd`)
  is an optional, documented secondary path.

## Non-negotiable architectural rule: tenant isolation

Every tenant-scoped entity and endpoint must enforce isolation at the data layer
via EF Core global query filters over a resolved `ITenantContext`, and must ship
with an automated cross-tenant isolation test. The isolation suite is a CI
release gate — a failing isolation test blocks merge. This is not optional
hardening; it is the project's core technical premise (see the charter's Risks
section: cross-tenant data leak is the highest-severity risk).

## Workflow

issue → branch (`feat/`, `fix/`, `chore/`, `docs/`) → PR (`Closes #N`) → CI green
→ squash merge. Direct commits to `main` are not allowed; `main` is branch
protected. See [CONTRIBUTING.md](CONTRIBUTING.md).

## Licensing

AGPL-3.0. Any party that runs this software as a network service must offer the
complete corresponding source, including modifications, to users of that
service (§13). Sign off commits (`git commit -s`, DCO) to contribute.

## Current milestone

M0 — Walking skeleton: Aspire AppHost orchestrating a gateway + one business
service + PostgreSQL/Redis/broker, all healthy in the Aspire dashboard;
`ITenantContext` resolution and EF Core global query filters wired; first
automated cross-tenant isolation test passing. See the repo's GitHub milestones
and issues for the current breakdown.

Only the current milestone has issues filed against it. When its issues are all
closed, break down the next milestone from [ROADMAP.md](ROADMAP.md) into issues
before starting work on it — don't assume someone else has already done this.
