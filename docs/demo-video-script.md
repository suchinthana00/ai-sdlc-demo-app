# AI in SDLC Demo Video Script

This script is designed for an 8–10 minute live demo that shows real repo activity, software execution, and project evidence. It prioritizes live validation over static reading and avoids inventing results or screenshots that are not available.

Target runtime: 8 minutes 30 seconds to 9 minutes 30 seconds.

## Recording setup

Keep these visible while recording:
- VS Code workspace root
- Terminal 1: repo status and git log
- Terminal 2: unit tests
- Terminal 3: Playwright E2E run
- Terminal 4: mutation testing run
- Browser with the running app
- Source control / Git panel

Recommended windows on screen:
- Explorer with repo folders
- Editor with `AGENTS.md` or `ApprovalService.cs`
- Terminal output from tests, Playwright, and mutation runs
- Browser tab with app login or approval flow

## 1) Introduction to the assignment

- Duration: 0:35–0:45
- What to show on screen:
  - Title / intro slide or repo root with README
  - Browser opened to the app login page
  - Quick repo tree: `backend/`, `specs/`, `quality/`, `e2e/`, `ai/`
- What to say:
  - “This demo shows an AI-assisted SDLC workflow on a small invoice approval application. The project is intentionally simple: a .NET 8 API, static web UI, and in-memory data, which makes it easy to run locally and easy to show the full lifecycle.”
  - “The assignment is about adding role-aware approval limits while preserving a clean architecture. The business rules belong in the domain layer, not in controllers or the UI.”
- Files or commands to demonstrate:
  - `README.md`
  - `dotnet run --project backend/src/InvoiceApp.Api`
  - `git status --short`
- Expected evidence:
  - App running on localhost
  - Repo structure visible in the Explorer
  - README or app startup screen visible

## 2) Repository architecture and AGENTS.md

- Duration: 0:45–1:05
- What to show on screen:
  - VS Code Explorer with `backend/src/InvoiceApp.Domain`, `backend/src/InvoiceApp.Api`, `backend/tests/InvoiceApp.Tests`
  - `AGENTS.md` open in the editor
- What to say:
  - “This repo follows a layered architecture. The domain project owns the business logic and approval rules; the API project owns controllers, auth, session management, and the web UI; the test project checks the behavior.”
  - “`AGENTS.md` is the onboarding contract for AI agents. It explains the architecture, important files, testing commands, and the design boundaries we must respect.”
- Files or commands to demonstrate:
  - `AGENTS.md`
  - `backend/src/InvoiceApp.Domain/ApprovalService.cs`
  - `backend/src/InvoiceApp.Api/Program.cs`
  - `backend/tests/InvoiceApp.Tests/ApprovalServiceTests.cs`
- Expected evidence:
  - Repo structure clearly demonstrates domain/API/test separation
  - AGENTS.md references the approval requirements and implementation guardrails

## 3) Multiple AI agents and their responsibilities

- Duration: 0:45–1:00
- What to show on screen:
  - `ai/logs/AI-INTERACTION-SUMMARY.md`
  - `ai/logs/ai-interactions.jsonl`
  - Optional: git log or commit graph
- What to say:
  - “This workflow used multiple AI roles rather than a single generic assistant. The Planning Agent framed the scope, the Developer Agent implemented the rules, the QA/Test Agent verified behavior, and the review agent checked risk and gaps.”
  - “The value of this pattern is that the work is separated into planning, implementation, verification, and review, which makes the development process auditable.”
- Files or commands to demonstrate:
  - `ai/logs/AI-INTERACTION-SUMMARY.md`
  - `ai/logs/ai-interactions.jsonl`
  - `git log --oneline --decorate --graph`
- Expected evidence:
  - AI log references to Planning, QA, Developer, Test, and review roles
  - Artifact names and commit references showing staged workflow activity

## 4) Skills and hooks

- Duration: 0:40–0:55
- What to show on screen:
  - `.github/skills/` directory or relevant skills list
  - `.github/hooks/` if present in the repo
  - A short file list or terminal output showing the skill names
- What to say:
  - “The repo includes reusable Copilot skills for requirement analysis, specification-driven development, TDD, code review, Playwright E2E testing, and mutation testing. Hook automation captures prompts and workflow events so the AI process leaves traceable evidence.”
  - “This is important because it turns AI assistance into an auditable workflow instead of a black box.”
- Files or commands to demonstrate:
  - `.github/skills/`
  - `.github/hooks/`
  - `ls .github/skills` or equivalent file listing
- Expected evidence:
  - Skills visible in the repo
  - Hook or log metadata showing captured prompts or workflow metadata

## 5) Specification-driven development using Spec Kit

- Duration: 1:05–1:25
- What to show on screen:
  - Terminal with the Spec Kit initialization command, if available
  - `specs/role-based-invoice-approval/spec.md`
  - `specs/role-based-invoice-approval/plan.md`
  - `specs/role-based-invoice-approval/research.md`
  - `specs/role-based-invoice-approval/tasks.md`
- What to say:
  - “The feature started from requirements, not from guessing. We used Spec Kit to create the specification, implementation plan, research notes, and task breakdown.”
  - “The important ambiguity was clarified explicitly: the first approver may be a Manager or Director, while the second approver must be a different Director. We captured that as a human-approval assumption before implementation.”
- Files or commands to demonstrate:
  - `specify init . --integration copilot` or equivalent
  - `specs/role-based-invoice-approval/spec.md`
  - `specs/role-based-invoice-approval/plan.md`
  - `specs/role-based-invoice-approval/tasks.md`
- Expected evidence:
  - `specs/` artifacts visible in the repo
  - Requirements, acceptance criteria, edge cases, negative scenarios, and testing requirements recorded
  - If the command did not run successfully in this environment, say: “Placeholder: Spec Kit initialization output here, or use the repo-generated spec artifacts as the evidence record.”

## 6) Two independent implementation steps and commits

- Duration: 1:15–1:35
- What to show on screen:
  - `git log --oneline --decorate --graph`
  - `backend/src/InvoiceApp.Domain/ApprovalService.cs`
  - `backend/src/InvoiceApp.Domain/Invoice.cs`
  - Source control panel showing two feature-related commits or equivalent history
- What to say:
  - “The implementation was intentionally split into two focused steps so reviewers could see the domain changes clearly and trace the risk reduction.”
  - “Step one covered the base approval policy and role-aware limits. Step two covered the stricter dual-director requirement, duplicate approval prevention, and immutability for rejected approval attempts.”
  - “The repo history includes commits such as `feat: enforce role-based invoice approval limit` and `feat: require distinct director approvals above limit`, which is the evidence trail for this staged implementation.”
- Files or commands to demonstrate:
  - `git log --oneline --decorate --graph`
  - `backend/src/InvoiceApp.Domain/ApprovalService.cs`
  - `backend/tests/InvoiceApp.Tests/ApprovalServiceTests.cs`
- Expected evidence:
  - Separate commit history or staged changes showing two implementation steps
  - Domain logic clearly updated in the service layer
  - If the repo already contains only the final implementation, label it as a reconstructed workflow and show the relevant files as the evidence trail

## 7) Unit test execution

- Duration: 0:45–0:55
- What to show on screen:
  - Terminal executing `dotnet test backend/InvoiceApp.sln`
  - Test summary output with pass/fail result
- What to say:
  - “Now I run the unit tests to validate the approval logic in isolation. This is the fastest feedback loop for thresholds, duplicate approver checks, and state immutability.”
  - “The key question is not whether the tests compile, but whether they detect the wrong approval decision before any invoice mutation happens.”
- Files or commands to demonstrate:
  - `dotnet test backend/InvoiceApp.sln`
  - `backend/tests/InvoiceApp.Tests/ApprovalServiceTests.cs`
  - Optional: `dotnet test backend/InvoiceApp.sln --configuration Release --no-build`
- Expected evidence:
  - Terminal output showing `Passed` or `Failed`
  - Summary counts from xUnit test execution
  - Placeholder if not available: “[Placeholder: live unit test output here.]”

## 8) Playwright rejected approval E2E test execution

- Duration: 0:55–1:10
- What to show on screen:
  - Terminal running `cd e2e && npm test`
  - Browser flow: sign in, create invoice, submit, attempt over-limit approval, observe error and unchanged status
  - Optional screenshot or Playwright report
- What to say:
  - “This is the browser-level proof that the user-facing flow behaves correctly. I am validating the exact scenario that must not mutate invoice data: an over-limit approval attempt is rejected, the user sees a clear reason, and the invoice remains pending.”
  - “This is the best evidence that the business rule is enforced end-to-end.”
- Files or commands to demonstrate:
  - `cd e2e && npm test`
  - `e2e/tests/invoice-approval-limit.spec.ts`
  - `e2e/playwright.config.ts`
  - `e2e/test-results/...`
- Expected evidence:
  - Playwright terminal summary showing pass/fail
  - Browser output with clear rejection text
  - Screenshot or report path in `e2e/test-results/`
  - Placeholder if not available: “[Placeholder: E2E screenshot or report output here.]”

## 9) Live mutation testing execution

- Duration: 0:55–1:10
- What to show on screen:
  - Terminal running Stryker against the domain project
  - Mutation report output or HTML report path under `quality/mutation-testing/`
  - Browser preview of the generated HTML report if available
- What to say:
  - “I now run mutation testing to see whether the tests are strong enough to catch logic regressions. A passing suite is not enough if the implementation can be mutated without the tests failing.”
  - “The mutation score tells us how hard it is to break the business rules intentionally.”
- Files or commands to demonstrate:
  - `dotnet-stryker --project 'backend/src/InvoiceApp.Domain/InvoiceApp.Domain.csproj' --test-project 'backend/tests/InvoiceApp.Tests/InvoiceApp.Tests.csproj' --reporter 'progress' --reporter 'html' --reporter 'json' --output "quality/mutation-testing"`
  - `quality/mutation-testing/final.md`
  - `quality/mutation-testing/comparison.md`
- Expected evidence:
  - Mutation summary showing created, killed, survived mutants and final score
  - Report file path or HTML output
  - Placeholder if not available: “[Placeholder: mutation score and report path here.]”

## 10) AI interaction JSONL logs

- Duration: 0:35–0:45
- What to show on screen:
  - `ai/logs/ai-interactions.jsonl`
  - `ai/logs/AI-INTERACTION-SUMMARY.md`
  - maybe a few lines from the log showing timestamped prompts and metadata
- What to say:
  - “This assignment leaves evidence behind in the AI interaction logs. The JSONL records prompts, workflow stages, and task context, while the summary explains what the workflow was doing at each phase.”
  - “This is how we maintain traceability for AI-assisted work, even when session IDs or tool payloads are not fully captured by the hook.”
- Files or commands to demonstrate:
  - `ai/logs/ai-interactions.jsonl`
  - `ai/logs/AI-INTERACTION-SUMMARY.md`
  - `tail -n 20 ai/logs/ai-interactions.jsonl` or equivalent
- Expected evidence:
  - JSONL entries with timestamps, prompts, roles, and captured workflow metadata
  - Summary file explains the sequence from planning to review

## 11) Pull request and AI review findings

- Duration: 0:45–1:00
- What to show on screen:
  - `quality/code-review/initial-review.md`
  - `quality/code-review/review-decisions.md`
  - Optional PR description or GitHub review summary
- What to say:
  - “The review step is critical. AI review findings are not automatically accepted as truth; they are documented, categorized, and reviewed by a human.”
  - “The repo contains review outcomes such as `DEFERRED`, `DISMISSED`, and `FIXED` as a record of whether the issue is a real defect, a product-scope question, or a maintainability concern.”
  - “This is the human judgment layer that prevents the workflow from becoming unreviewed automation.”
- Files or commands to demonstrate:
  - `quality/code-review/initial-review.md`
  - `quality/code-review/review-decisions.md`
  - `git diff --stat` or PR summary page if available
- Expected evidence:
  - Review findings and verdicts visible
  - Evidence of human decisions recorded in the review doc

## 12) Human decisions and final submission

- Duration: 0:40–0:55
- What to show on screen:
  - Final repo state or last git status output
  - Final artifacts under `specs/`, `quality/`, and `ai/logs/`
  - Optional final commit or branch summary near the end
- What to say:
  - “The final step is human sign-off. The AI does not decide whether the change is complete; the human reviews the specification, the test results, the mutation evidence, and the code review findings.”
  - “This assignment is complete when the business rules are validated, the evidence is recorded, and the human accepts the outcome.”
- Files or commands to demonstrate:
  - `git status --short`
  - `git log --oneline -n 10`
  - final artifact files in `specs/`, `quality/`, and `ai/logs/`
- Expected evidence:
  - Clean final state or clear record of final artifacts
  - Human decision notes visible in the review documents
  - Final submission is supported by objective evidence, not a narrative alone

## Closing line for the video

“AI-assisted SDLC works best when the process is traceable: requirements become specs, specs become implementation tasks, implementation is tested in multiple ways, and human review decides whether the evidence is sufficient.”

## Final recording checklist

Before clicking record or before uploading the final video, confirm all of these:

- [ ] Project root is open in VS Code and the repo is clearly visible
- [ ] App can be started locally and is ready for live demo
- [ ] Browser is open and ready for sign-in or invoice flow
- [ ] Git panel or terminal is visible for commit history
- [ ] Relevant files are ready to open quickly: `AGENTS.md`, `ApprovalService.cs`, `specs/...`, `quality/...`, `ai/logs/...`
- [ ] Unit testing command is ready: `dotnet test backend/InvoiceApp.sln`
- [ ] Playwright command is ready: `cd e2e && npm test`
- [ ] Mutation testing command is ready: Stryker command against the domain project
- [ ] At least one AI evidence file is visible: `AI-INTERACTION-SUMMARY.md` or `ai-interactions.jsonl`
- [ ] Review artifacts are visible: `initial-review.md` and `review-decisions.md`
- [ ] Placeholders are used instead of invented results when the exact output is unavailable
- [ ] Audio is clear and the pace stays close to 8–10 minutes total
- [ ] Final segment clearly explains human decision and final submission
- [ ] No unsupported claims are made about pass/fail results without evidence visible on screen
- [ ] The video emphasizes live execution, not documentation notes alone

## Notes for the presenter

- Prefer live execution over talking about files alone.
- If a command fails or a result is unavailable, say so plainly and show the relevant placeholder or evidence file.
- Keep the narration anchored to evidence files and commands that are actually in the repo.
- The goal is not to fake a perfect sequence; it is to show a credible, traceable AI-assisted SDLC workflow with clear human oversight.