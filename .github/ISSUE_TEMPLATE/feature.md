---
name: Feature
about: A unit of work to deliver — ideally one issue → one PR → merge
title: "[Feature] "
labels: ["type:feature"]
assignees: []
---

## Why

<!-- The user/charter value this delivers. Link to the charter objective or
     success measure it advances. -->

## What

<!-- A concise description of the change. For a user-facing capability, frame it
     as a user story:
     As a <role: admin / staff / volunteer> I want <capability> so that <benefit>. -->

## Priority

<!-- MoSCoW — keep one. Add the matching label: priority:high (Must),
     priority:medium (Should), priority:low (Could). "Won't (this release)"
     means defer, don't file. -->

- **Must** / **Should** / **Could**

## Acceptance criteria

<!-- Measurable, testable conditions. Prefer concrete numbers/states over vague
     adjectives ("available 24/7", not "highly available"). -->

- [ ] 
- [ ] 
- [ ] 

## Tenant isolation

<!-- Required for any tenant-scoped entity or endpoint — this is a CI release
     gate (see CHARTER.md / CLAUDE.md). Tick N/A only if the change touches no
     tenant-scoped data. -->

- [ ] Adds/updates a cross-tenant isolation test for every tenant-scoped entity or endpoint touched
- [ ] N/A — no tenant-scoped data involved

## Related issues / dependencies

<!-- Blocking or related issues by #number, and the milestone this belongs to. -->

## Notes / out of scope

<!-- Anything explicitly not part of this issue, or open questions. -->
