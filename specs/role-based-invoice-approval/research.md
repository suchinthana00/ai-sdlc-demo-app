# Research and Clarification: Role-Based Invoice Approval

## Status

This feature is mostly well-defined, but the requirement for invoices above 500,000 leaves one important ambiguity:

- The requirement states the second approver must be a Director.
- It does not explicitly state the role of the first approver for invoices above 500,000.

## Clarification Decision

The implementation assumption for this feature is:

- The first approver on invoices above 500,000 may be either a Manager or a Director.
- The second approver must be a different Director.
- The same user cannot approve the same invoice twice.
- Rejected approval attempts must not mutate invoice data.

This assumption must be explicitly reviewed before implementation to ensure it matches stakeholder expectations.

## Domain Assumptions

- The app uses role membership via the session middleware and role attribute patterns already present in the repo.
- Domain business rules belong in `ApprovalService`, not `Program.cs` or API controllers.
- The in-memory repository is sufficient for validation and tests.
- Approval history or distinct approver tracking is required to implement duplicate detection reliably.

## Risk Areas

- Duplicate approver detection may require additional state beyond `DecidedBy`.
- Invalid approval attempts must be atomic to prevent data mutation.
- Large invoice approvals require strict validation of both role and distinctness.

## Decision Record

- Decision: large invoice approval requires two distinct approvers.
- Additional rule: second approver must be a different Director.
- Human approval required: confirm first approver may be Manager or Director for large invoices.
