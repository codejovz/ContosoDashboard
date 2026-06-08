<!--
Sync Impact Report
Version change: unset → 1.0.0
Added principles: Training Integrity, Spec-Driven Development, Security Discipline, Offline-First Simplicity, Documentation & Review
Added sections: Additional Constraints, Development Workflow
Templates reviewed: .specify/templates/plan-template.md (✅ no change needed), .specify/templates/spec-template.md (✅ no change needed), .specify/templates/tasks-template.md (✅ no change needed)
Follow-up TODOs: none
-->

# ContosoDashboard Constitution

## Core Principles

### I. Training Integrity
All design decisions and implementation guides MUST be shaped by the repository’s training purpose. Every code change, document update, and example MUST preserve educational clarity and avoid production-only assumptions.

### II. Spec-Driven Development
All feature work MUST begin with explicit plans, documented specifications, and task-level decomposition. Code MUST be implemented only after user scenarios, acceptance criteria, and test intentions are defined.

### III. Security Discipline
Security is a first-class requirement for this training application. Authorization, IDOR protection, and service-level access checks MUST be enforced before any feature is accepted.

### IV. Offline-First Simplicity
The repository MUST favor local, self-contained implementations that work without external services. Complexity is only acceptable when it directly supports training goals or a clear migration path to production-ready alternatives.

### V. Documentation & Review
Every change MUST include clearly written guidance, update documentation, and subject itself to review. Pull requests MUST explain the problem, the design decision, and any trade-offs made.

## Additional Constraints
Technology and implementation choices MUST align with the project’s training context: ASP.NET Core 8.0, Blazor Server, Entity Framework Core, and local SQL Server LocalDB. External cloud services MUST be avoided unless they are explicitly represented as a documented production migration path.

## Development Workflow
Work MUST follow feature branches, incremental delivery, and explicit review. Each feature branch MUST include a linked spec and task plan, and every pull request MUST verify that tests, documentation, and security checks are complete.

## Governance
This constitution supersedes informal habits and local preferences for this repository. All development work MUST comply with the principles above.

- Amendments MUST be made through documented changes in this file and reviewed in a pull request.
- Version updates MUST reflect meaningful governance or policy changes.
- Compliance reviews MUST be part of every feature evaluation and PR checklist.
- Any deviation from these principles MUST be justified in the PR description and approved by a reviewer.

**Version**: 1.0.0 | **Ratified**: 2026-06-08 | **Last Amended**: 2026-06-08
