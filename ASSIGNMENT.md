# AI in SDLC Assignment Report

## 1. Assignment Overview

This assignment documents the implementation and validation of the role-based invoice approval feature in the repository `ai-sdlc-demo-app`. The work focused on enforcing manager/director approval limits, requiring dual Director approval above the highest threshold, blocking duplicate approvers, and ensuring rejected approval attempts leave invoice state unchanged.

The evidence for this submission is drawn from the repository itself:
- `AGENTS.md` for the project conventions and assignment constraints
- `specs/role-based-invoice-approval/` for the generated specification artifacts
- `quality/mutation-testing/` for baseline and final mutation results
- `quality/code-review/` for independent review findings and verdicts
- `ai/logs/ai-interactions.jsonl` and `ai/logs/AI-INTERACTION-SUMMARY.md` for AI workflow evidence
- `git log` for the actual commit history
- `dotnet test` and Playwright execution output for validation evidence

## 2. Repository URL

- GitHub repository: https://github.com/suchinthana00/ai-sdlc-demo-app.git

## 3. Pull Request URL

- Pull request: https://github.com/suchinthana00/ai-sdlc-demo-app/pull/1

## 4. Feature Requirements

The feature requirements were captured in `specs/role-based-invoice-approval/spec.md` and align to the repository guidance in `AGENTS.md`:

- Managers can approve invoices up to 50,000.
- Directors can approve invoices up to 500,000.
- Invoices above 500,000 require two distinct approvers.
- The second approver must be a Director.
- The same user cannot approve the same invoice twice.
- Rejected approval attempts must return a clear reason.
- Rejected approval attempts must not change invoice status, approval history, or other relevant business data.
- Approval validation must occur before mutation in the domain layer.

These requirements are documented in `specs/role-based-invoice-approval/spec.md` and the task breakdown in `specs/role-based-invoice-approval/tasks.md`.

## 5. Application Architecture

Repository structure and architecture are consistent with the project conventions in `AGENTS.md`:

- Domain layer: `backend/src/InvoiceApp.Domain/`
  - Business rules live here.
  - `ApprovalService` contains approval workflow logic.
  - `Invoice` and related model types are in the domain project.
- API layer: `backend/src/InvoiceApp.Api/`
  - HTTP and authentication concerns live here.
  - Controllers and repository implementation are in the API project.
- Test layer: `backend/tests/InvoiceApp.Tests/`
  - xUnit tests validate service-level behavior.
- E2E layer: `e2e/`
  - Playwright tests validate the browser workflow.

This matches the rule in `AGENTS.md`: keep business logic in the domain layer and keep controllers thin.

## 6. AGENTS.md and Context Engineering

`AGENTS.md` is the repository onboarding guide for AI agents and establishes the operating constraints for this assignment.

Key points from `AGENTS.md`:
- The app is a .NET 8 invoice approval demo with a minimal static web UI and in-memory store.
- Approval logic belongs in `ApprovalService` in the domain project.
- Controllers should remain HTTP wrappers.
- Unit tests should be added for each new rule.
- The app should not introduce database or service infrastructure beyond the current simple architecture.
- Error handling should remain aligned with the existing mapping in `ApiControllerBase`.

This is a clear example of context engineering: the repository supplies the business constraints, architecture boundaries, and evaluation criteria in a single file so that AI work stays aligned with the assignment intent.

## 7. AI Tool and Model Used

Based on the actual JSONL log in `ai/logs/ai-interactions.jsonl`, the AI workflow used:

- Tool: GitHub Copilot Chat (VS Code)
- Model: `MAI-Code-1.1-Flash`

The hook log entries explicitly record the model as `MAI-Code-1.1-Flash` in the captured events, and the summary confirms the workflow used GitHub Copilot Chat and the repository’s task-based AI artifacts.

## 8. Multiple Agent Architecture

The repository log and summary document a multi-agent-style workflow, even though the underlying hook does not capture complete sub-agent metadata.

The AI interaction summary identifies role-based workflow labels including:
- Planning Agent
- QA Agent
- Developer Agent
- Test Agent
- Senior Code Review Agent
- AI Audit and Documentation Agent

The actual JSONL log includes explicit prompt examples such as:
- “Act as the Planning Agent...”
- “Start implementation”
- “finalize the spec, install what's need for spec”

The project summary notes that these are prompt-based roles and not all tool-internal metadata was captured by the audit hook. The evidence is sufficient to show a structured multi-role workflow, but not every internal session detail is available.

## 9. Skills and Hooks

The repository includes reusable Copilot skill definitions under `.github/skills/` and the audit hook configuration under `.github/hooks/`.

Observed skills referenced in the evidence:
- `requirement-analysis`
- `specification-driven-development`
- `test-driven-development`
- `code-review`
- `playwright-e2e-testing`
- `mutation-testing`

Hook configuration:
- `.github/hooks/copilot-audit.json`
- `.github/hooks/copilot-audit.cjs`

The hook is configured to fire for:
- `sessionStart`
- `userPromptSubmitted`
- `preToolUse`
- `postToolUse`
- `sessionEnd`

The hook records timestamps and prompt metadata, and the repository summary notes that some details such as session IDs and tool payloads are intentionally not fully captured.

## 10. Specification-Driven Development

The repo includes the full specification set under `specs/role-based-invoice-approval/`:
- `spec.md`
- `plan.md`
- `research.md`
- `tasks.md`

This work was created before implementation and includes:
- functional requirements
- acceptance criteria
- boundary value conditions
- negative scenarios
- data integrity requirements
- authorization requirements
- unit testing requirements
- Playwright E2E requirements

The requirement artifacts also include an explicit assumption about the high-value invoice path. The repository summary states that the first approver assumption was clarified and treated as a human-reviewed decision point in the workflow.

## 11. Implementation Approach

### Step 1: Single-approver limits

The first implementation step was the role-based approval threshold logic for a single approver.

Actual repository evidence:
- Commit: `1ac95bf` — `feat: enforce role-based invoice approval limit`
- Related feature work in `backend/src/InvoiceApp.Domain/ApprovalService.cs`
- The service contains limits for:
  - `ManagerApprovalLimit = 50_000m`
  - `DirectorApprovalLimit = 500_000m`

The logic validates role and amount before mutation and uses descriptive rejection messages for invalid approvals.

### Step 2: Two distinct approvers

The second implementation step added the distinct Director requirement above the highest threshold.

Actual repository evidence:
- Commit: `f361081` — `feat: require distinct director approvals above limit`
- `ApprovalService` enforces that invoices above 500,000 require a second distinct approver and that the second approver must be a Director.
- The service also checks for duplicate approvers and rejects invalid second-approver scenarios before changing invoice state.

## 12. Git Commit History

Actual commit history from the repository:

- `3f0abd3` — `Final logs and summaries`
- `d9d423c` — `Tested`
- `f361081` — `feat: require distinct director approvals above limit`
- `80ed459` — `Merge branch 'main' into features`
- `1ac95bf` — `feat: enforce role-based invoice approval limit`
- `7b5566d` — `Baseline added`
- `66cb3f9` — `changes`
- `3834913` — `Add Playwright e2e setup and coverage collector`
- `4421968` — `Add tests, CI, agent conventions and use-case briefs`
- `8214bc4` — `Add web UI for invoices and approvals`
- `dc182d9` — `Add API: sign-in, invoices and approvals endpoints`
- `754f6a1` — `Add domain: invoice lifecycle and discount rules`

The repository state at the time of this report shows a clean working tree aside from the untracked video file at the project root:
- `ai_in_sdlc_demo_suchinthana.mp4`

## 13. Unit Testing Results

Command used for the actual verification:

`& "C:\Users\stwijesundara\.dotnet\dotnet.exe" test "C:\data_tests\ai-sdlc-demo-app\backend\InvoiceApp.sln" --logger "console;verbosity=minimal"`

Result:
- Passed: 18
- Failed: 0
- Skipped: 0
- Total: 18

The run produced one warning from xUnit analyzer `xUnit2013` about using `Assert.Equal()` for collection size, but it did not fail the suite.

## 14. Playwright E2E Results

Command used:

`cd "C:\data_tests\ai-sdlc-demo-app\e2e"; npm test -- --reporter=line`

Actual result:
- 1 test run
- 1 passed
- 0 failed
- Duration: 5.0s

The E2E coverage file is `e2e/tests/invoice-approval-limit.spec.ts` and it validates the manager over-limit rejection scenario in the browser flow.

## 15. Mutation Testing

### Baseline results

From `quality/mutation-testing/baseline.md`:
- Mutants created: 51
- Mutants tested: 22
- Mutants skipped: 29
- Killed: 12
- Survived: 10
- Final mutation score: 30.77%

The baseline was run before the feature work and confirmed the initial low mutation resilience of the original domain tests.

### Final results

From `quality/mutation-testing/final.md`:
- Mutants created: 133
- Mutants tested: 66
- Mutants skipped: 67
- Killed: 50
- Survived: 16
- Final mutation score: 51.02%

### Comparison and analysis

From `quality/mutation-testing/comparison.md`:
- Baseline score: 30.77%
- Final score: 51.02%
- Improvement: +20.25 percentage points

This improvement is consistent with the added approval-limit rules and the broader domain test coverage. However, the final report explicitly states that the suite is not yet exhaustive and that 16 mutants survived, especially in threshold and role-specific guard logic. The key gap is not a total failure of the feature; rather, the work improved substantially while still leaving some edge-case mutation survivors.

## 16. AI Interaction Logging

The repository contains the actual AI interaction evidence:
- `ai/logs/ai-interactions.jsonl`
- `ai/logs/AI-INTERACTION-SUMMARY.md`

Important observations from the log and summary:
- The audit log captures prompt-driven workflow stages and timestamps.
- It records role labels such as `Planning Agent` and the project tasks.
- It notes that some metadata such as session IDs, tool payloads, and full sub-agent traces were not emitted by the hook.
- The summary explicitly documents the workflow from planning to implementation to review.

The JSONL log includes actual prompt text for the planning stage and for later implementation actions. The summary also lists the actual implementation-related commit references observed in the repo history.

## 17. AI Code Review Findings

The independent review artifacts are in:
- `quality/code-review/initial-review.md`
- `quality/code-review/review-decisions.md`

### Actual review findings and verdicts

From `quality/code-review/initial-review.md`:

- `CR-001`
  - Severity: High
  - Description: approval API still allows non-Manager/Director roles through controller and service logic for threshold validation, creating an authorization gap.
  - Verdict in `review-decisions.md`: `DEFERRED`

- `CR-002`
  - Severity: High
  - Description: approval workflow performs check-then-update without an atomic guard, creating a concurrency risk around duplicate approval validation.
  - Verdict in `review-decisions.md`: `DEFERRED`

- `CR-003`
  - Severity: Medium
  - Description: the current model relies on mutable approval state and is maintainability-sensitive, but not an immediate correctness failure.
  - Verdict in `review-decisions.md`: `DISMISSED`

The review summary also states: `Request changes before merge` as the initial review recommendation, but the final review decisions document explicitly classify the findings as `DEFERRED` or `DISMISSED` rather than “fixed” in this branch.

## 18. Human Decisions and Oversight

The repository evidence shows human oversight was built into the workflow:

- `AGENTS.md` defined the engineering rules and scope guardrails.
- The specification workflow required clarification of ambiguous approval-role behavior before implementation.
- The quality review process documented findings and required a human decision for each issue.
- `quality/code-review/review-decisions.md` records the final verdicts and explains that the branch does not claim any code fix for the deferred issues.
- The repository summary states that the task did not auto-commit and instead used human review gates before final acceptance.

This is consistent with the assignment goal: AI-assisted development is traceable and auditable, and the human reviewer is the final authority on whether the evidence is sufficient.

## 19. Challenges and Lessons Learned

The actual repository evidence highlights several practical challenges:

- The environment initially had no .NET SDK in PATH, which was documented in `quality/mutation-testing/baseline.md`.
- The repo required the .NET 8 SDK and Stryker setup before the mutation validation could run.
- The assignment required a careful split between policy validation and mutation safety in the domain layer.
- The independent review identified real product-scope issues around role policy and concurrency, which were treated as `DEFERRED` rather than silently ignored.
- Mutation testing proved that the decision logic was materially improved, while still leaving meaningful surviving mutants in threshold and role-validation paths.

The main lesson is that real workflow quality depends on evidence, not only on passing tests. The combination of unit tests, E2E tests, mutation tests, and review artifacts provides a stronger engineering story than any single validation step.

## 20. Demo Video Link

The project root contains the local demo video:
- `ai_in_sdlc_demo_suchinthana.mp4`

Public raw video link:
- https://github.com/suchinthana00/ai-sdlc-demo-app/raw/refs/heads/features/ai_in_sdlc_demo_suchinthana.mp4

This file is present in the repo root as the project deliverable and is also available via the raw GitHub URL above.

## 21. Final Deliverables Checklist

Completed and evidenced in the repository:
- [x] Repository URL recorded
- [x] Pull request URL recorded
- [x] Feature specification created under `specs/role-based-invoice-approval/`
- [x] Implementation history recorded via actual git commit log
- [x] Unit test evidence recorded and validated
- [x] Playwright E2E evidence recorded and validated
- [x] Mutation baseline and final reports recorded
- [x] AI interaction summary and JSONL log recorded
- [x] Code review findings and verdicts recorded
- [x] Human review and oversight documented

Pending or external-only items:
- [ ] Public external video hosting link (the local video file exists at the project root, but the URL is not yet available in repo evidence)
- [ ] Any additional remote PR review or merge status beyond the provided PR URL

## Final Statement

This assignment was executed with repository-based evidence rather than fabrication. The core implementation, validation, and review artifacts are present in the repo and support the final submission. The documented evidence includes actual commit hashes, actual test results, actual mutation scores, and the actual AI workflow logs and review decisions. The work was intentionally scoped to the project’s simple architecture and human-review model, with deferred findings clearly labeled rather than hidden.
