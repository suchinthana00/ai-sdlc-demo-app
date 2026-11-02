# Review decision log

This document records the review outcome for the findings in `quality/code-review/initial-review.md`.

The table below reflects the current repository state and is intended to support human final adjudication. No production fix has been implemented in this branch for the findings below, so any `FIXED` status would be inaccurate.

## Findings and verdicts

| Finding ID | Verdict | Reasoning | Corrective commit |
| --- | --- | --- | --- |
| `CR-001` | `DEFERRED` | This is a valid business-rule gap in the current implementation: the controller still permits `Approver` and `Admin`, while the domain logic only enforces manager/director thresholds by role name. The issue remains relevant to the approval-policy feature, but it appears to be intentionally deferred in the current minimal demo scope rather than a patch that was silently ignored. | No corrective commit in the current branch. |
| `CR-002` | `DEFERRED` | This is a real concurrency and race-condition concern for a shared in-memory approval flow: validation and mutation are not performed atomically, and the repository lock guards reads/writes separately rather than the full decision sequence. The risk is genuine, but it is also a product-scope decision whether this demo needs a stronger per-invoice lock or a transactional domain boundary before the feature is considered complete. | No corrective commit in the current branch. |
| `CR-003` | `DISMISSED` | This is an optional maintainability concern rather than a confirmed defect. The current `ApprovalHistory` list is consistent with the repository’s intentionally simple architecture, and the project explicitly uses a lightweight in-memory domain model for demo scenarios. The observation is valid as a future refactoring candidate, but it does not represent a current correctness failure under the demonstrated design. | Not applicable. |

## Notes on the decisions

- `CR-001` is the strongest genuine issue from the review. It is not a speculative concern; it is directly evidenced by the controller role list and the service logic.
- `CR-002` is also a genuine risk, but it is more architectural and concurrency-oriented than a direct functional failure in a single-threaded demo flow. It is therefore best categorized as deferred unless stronger concurrency requirements are added.
- `CR-003` is a maintainability observation, not a bug. It should be tracked separately as a refactoring follow-up rather than treated as a blocker for the feature in its current scope.

## Proposed pull request description

### Title
Role-based invoice approval review: policy gap and concurrency follow-up

### Summary
This PR includes the approval-flow review and the documented outcomes from the independent code review. The review examined the domain approval logic, controller authorization, repository behavior, unit tests, and the relevant end-to-end coverage.

### Review findings and verdicts
- `CR-001`: `DEFERRED` — Genuine approval-policy gap remains in the current role model.
- `CR-002`: `DEFERRED` — Genuine concurrency risk remains because the approval decision is not fully atomic.
- `CR-003`: `DISMISSED` — Maintainability concern only; not a current correctness defect in the demo architecture.

### Scope of the review
- Checked the acceptance criteria for manager and director approval limits.
- Reviewed duplicate approver handling and state immutability expectations.
- Confirmed whether approval rules remain in the domain layer and whether the API layer stayed thin.
- Evaluated transaction and concurrency behavior in the in-memory repository.
- Reviewed test coverage and the relevance of the Playwright validation for the approval workflow.

### Review outcome
The review did not identify a patch to claim as complete in the current branch. The findings remain valid as either deferred follow-ups or a dismissed maintainability concern. This PR should be treated as a review artifact rather than a code-fix submission unless the feature scope is explicitly expanded.

### Recommendation
Proceed only after human review confirms whether the approval-role policy and concurrency requirements are in scope for the next iteration. The current review does not support a claim that the issues were fixed or resolved in code.
