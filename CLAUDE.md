# CLAUDE.md

## What this project is

Open Shelter (OSMP) — a multi-tenant SaaS platform for animal shelters and rescue
organizations (animals, intake, adoptions, fostering, medical scheduling), built
on .NET 10 / .NET Aspire to demonstrate distributed-systems architecture and
multi-tenant data isolation. The milestone breakdown lives in
[ROADMAP.md](ROADMAP.md) in this repo. Full objectives, success measures,
constraints, and licensing rationale live in the project charter
([CHARTER.md](CHARTER.md)) in this repo.

## Stack

- .NET 10, orchestrated with .NET Aspire — `src/OpenShelter.AppHost` is the
  orchestration entry point, `src/OpenShelter.ServiceDefaults` carries shared
  OpenTelemetry/health/resilience wiring used by every service.
- PostgreSQL, Redis, and a message broker (RabbitMQ / Azure Service Bus) as
  Aspire-orchestrated backing resources (added as services land).
- A staff-facing frontend is now in scope: a Blazor web app (`src/OpenShelter.Web`,
  .NET 10, Aspire-orchestrated) is planned as milestone M3 in [ROADMAP.md](ROADMAP.md).
  Don't treat the frontend as out of scope — only the public-facing adopter portal
  and mobile apps remain excluded.
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

## Project website

`docs/` is the GitHub Pages source for the public landing page (served from
`main` at https://s3ba-b.github.io/open-shelter/) — it is not a general docs
folder, don't repurpose it. It's a hand-written static page (no build step);
see the "Current milestone" section below for when to update it.

The page must stay understandable to non-technical visitors (shelter staff,
volunteers), not only developers, while keeping its value as a technical
reference. Follow "simple first, technical second": the upper sections (hero,
"why this exists", feature cards, roadmap, license) use plain, benefit-focused
language with no jargon; technical terms (Aspire, EF Core, OpenTelemetry,
multi-tenancy, OpenTelemetry spans, etc.) stay confined to the architecture
section, which is explicitly marked "For developers" and flagged as skippable.
Page language is English by default, with an optional Polish translation: an
in-page EN/PL toggle (top-right) swaps copy via a `docs/translations.js`
dictionary and remembers the choice. English ships in the HTML (so no-JS
visitors and crawlers get a complete page) and Polish is applied on top — keep
both languages at key parity when editing copy. The mock-up screenshots stay
English regardless of the toggle.

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
At the same time, update the roadmap section of `docs/index.html` (the
`#roadmap` list and the hero badge): mark the finished milestone done and move
the "in progress" marker to the next one. The page can't fetch live milestone
status itself (the repo is private, and GitHub's milestones API needs auth a
public static page can't hold), so this has to be a manual step done at the
same time as the issue breakdown above.
