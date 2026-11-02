# Initial review report

## Summary

This review covers the role-based invoice approval implementation, the domain logic, the repository behavior, the unit tests, and the Playwright validation. The code is mostly aligned with the intended approval flow, but I do not consider the change ready for approval because there are genuine authorization and concurrency risks that remain in the current implementation.

I would classify this as a request-changes review rather than an approve-as-is review.

## Findings

### CR-001
- File and line reference: `backend/src/InvoiceApp.Api/Controllers/ApprovalsController.cs:7-27`, `backend/src/InvoiceApp.Domain/ApprovalService.cs:39-72`, `backend/src/InvoiceApp.Domain/ApprovalService.cs:95-140`
- Severity: High
- Description: The approval API still allows non-Manager/Director roles through the controller and service, but the threshold validation is only enforced for the literal role names `Manager` and `Director`. In practice, any role admitted by the controller (`Approver`, `Admin`, `Manager`, `Director`) can bypass the amount policy unless it is explicitly one of those two strings.
- Why it matters: The intent of the feature is role-aware approval enforcement, and the repo guidance explicitly calls for role-based approval limits and “authorization MUST be based on actual user role and invoice amount.” The current code does not enforce the policy for `Approver` or `Admin` roles, so those identities can approve invoices without the intended authorization checks.
- Recommended correction: Either remove non-manager/director roles from the approval authorization list or add an explicit policy check that applies to every allowed approval role. The safest model is to allow only `Manager` and `Director` as approval roles for the invoice workflow, with an explicit error if a caller does not meet the policy for the requested invoice amount.

### CR-002
- File and line reference: `backend/src/InvoiceApp.Domain/ApprovalService.cs:39-72`, `backend/src/InvoiceApp.Domain/ApprovalService.cs:95-140`, `backend/src/InvoiceApp.Api/InMemoryInvoiceRepository.cs:8-42`
- Severity: High
- Description: The approval workflow performs a check-then-update sequence without a service-level atomic guard, and the repository only serializes individual `GetById` and `Update` calls rather than the full validate-plus-persist decision. That creates a race window for concurrent approval attempts against the same pending invoice.
- Why it matters: Two requests can pass the duplicate-approval check at the same time, both read the same history list, and both proceed to mutate the invoice. The in-memory repository is not a transaction boundary for the domain rule, so the duplicate prevention is vulnerable under concurrent load or repeated API calls in parallel.
- Recommended correction: Serialize approval operations at the domain/service boundary for the same invoice (for example, with a per-invoice lock or a repository operation that performs the validation and update atomically), and keep the duplicate check and state mutation in the same critical section. The repository lock should protect the full decision path, not just isolated read/write operations.

### CR-003
- File and line reference: `backend/src/InvoiceApp.Domain/ApprovalService.cs:39-72`, `backend/src/InvoiceApp.Domain/ApprovalService.cs:95-140`
- Severity: Medium
- Description: The method relies on the invoice’s mutable `ApprovalHistory` list to represent both partial approval state and duplicate detection, but it does not model approval intent or a separate approval-record lifecycle. This is workable for the demo, but the current design makes it easy to accidentally treat a partially-approved invoice as a completed approval in future changes.
- Why it matters: The feature is explicitly meant to guard duplicate approvers, second approver authorization, and exact boundary behavior. With the current state model, future changes can accidentally add or drop entries without a clear approval record semantics or without ensuring the rejected attempt leaves all business fields untouched. This is maintainability risk rather than an immediate functional defect, but it is a real design weakness for a rule-heavy workflow.
- Recommended correction: Keep the current list-based history if the repo intentionally stays simple, but add a clear domain invariant and test coverage that defines exactly when the history is mutated, when a partial approval is valid, and how a rejected attempt is prevented from changing the entity. If the workflow grows, introduce a dedicated approval record object instead of a raw string list.

## Final recommendation

Request changes before merge. The authorization gap and the concurrency window are both serious enough to warrant a fix before the feature is considered safe for use in a shared approval workflow.
