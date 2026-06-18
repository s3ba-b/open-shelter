# Roadmap

Open Shelter is delivered **iteratively-incrementally**: each milestone is a working, demoable slice of the system — API → service → data → tests — rather than a horizontal layer built in isolation. Milestones are defined by *working software*, not calendar dates, and are ordered by dependency.

- **Walking skeleton first** (M0). Tenant isolation is a cross-cutting concern, so the architectural spine — Aspire orchestration, tenant resolution, data-layer isolation — is built before any business feature. Every later feature is tenant-scoped from birth instead of being retrofitted.
- **Vertical slices** (M2–M4). Each domain area is built end-to-end as a self-contained increment, tenant-scoped and isolation-tested as it lands.
- **Isolation as a continuous gate.** The cross-tenant isolation test suite grows with every increment and runs in CI; a failing isolation test blocks a milestone from being considered done.

## Milestones

| Milestone | Definition of done |
|---|---|
| **M0 — Walking skeleton** | Aspire AppHost orchestrates a gateway + one business service + PostgreSQL/Redis/broker, all healthy in the dashboard. `ITenantContext` resolution and EF Core global query filters are wired, and the first automated cross-tenant isolation test passes. |
| **M1 — Identity & access** | Token-based auth with tenant claims; roles within an organization (admin, staff, volunteer); isolation enforced through the auth pipeline. |
| **M2 — Animals (first vertical slice)** | Animal records, intake history, and status tracking working end-to-end (API → service → data), fully tenant-scoped, with isolation tests covering the new entities. |
| **M3 — Adoption & fostering** | Adoption applications + approval flow and foster placements, end-to-end and tenant-scoped. |
| **M4 — Medical scheduling & background worker** | Vaccination/treatment schedules with due dates; the worker service generates reminders, adoption follow-ups, and per-tenant reporting; OpenTelemetry spans tagged by tenant are visible in the dashboard. |
| **M5 — Hardening & release** | Full isolation suite green in CI as a release gate; Docker Compose deployment with documentation; architecture diagram and README; optional Azure deployment documented. |

Each milestone above also exists as a [GitHub milestone](https://github.com/s3ba-b/open-shelter/milestones) with this same definition of done.

## How to start the next milestone

Only the **current** milestone has issues filed against it — the backlog is deliberately not pre-populated end to end. When the current milestone's issues are all closed:

1. Take the next milestone's row from the table above.
2. Break it down into a small set of issues, each deliverable as a single issue → branch → PR → merge unit (see [CONTRIBUTING.md](CONTRIBUTING.md)). Order them by dependency — foundation/infra before features.
3. Every tenant-scoped entity or endpoint introduced needs its own cross-tenant isolation test (non-negotiable — see [CLAUDE.md](CLAUDE.md)).
4. File the issues against the milestone in GitHub before starting work on it.

This is the same process used to break down M0 into its initial five issues.

## Out of scope (this version)

Payment processing, public-facing adopter portals, and mobile apps are explicitly out of scope. See the project charter's constraints for the full list and rationale.
