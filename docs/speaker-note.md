# Speaker Notes for the AI in SDLC Demo

This version is shorter and more realistic for a live demo. It keeps the focus on the app running in the browser and on the two live test executions.

## 1) Opening: start the app and show the browser

Show:
- VS Code workspace open
- PowerShell terminal with the app running
- Browser opened to http://localhost:5000

Say:
- “This is a small invoice approval app used for an AI-in-SDLC demo. I’m starting it locally and showing the real user flow in the browser.”
- “The point of the demo is to show how AI-assisted engineering works across planning, implementation, testing, and review, not just how to generate code.”

Files/commands:
- `dotnet run --project backend/src/InvoiceApp.Api`
- `http://localhost:5000`

Evidence:
- app is live in the browser
- login page is visible

## 2) Quick repo structure

Show:
- Explorer view with `backend/`, `specs/`, `quality/`, `e2e/`, `ai/`

Say:
- “The repo is intentionally small and layered. The domain code owns the business logic, the API layer owns HTTP and auth, and the test folders validate the behavior.”
- “This makes the workflow easy to explain and easy to audit.”

Files:
- `backend/src/InvoiceApp.Domain`
- `backend/src/InvoiceApp.Api`
- `backend/tests/InvoiceApp.Tests`

Evidence:
- folder structure is visible and understandable

## 3) AGENTS.md and repo guidance

Show:
- `AGENTS.md` open in the editor

Say:
- “This repo includes `AGENTS.md`, which acts as the project onboarding guide for AI agents. It explains the architecture, key files, business rules, and technical boundaries.”
- “That matters because AI work is only useful if it respects the repo structure and layer boundaries.”

Files:
- `AGENTS.md`
- `backend/src/InvoiceApp.Domain/ApprovalService.cs`

Evidence:
- role and architecture expectations are visible in the file

## 4) Multiple AI agents and responsibilities

Show:
- `ai/logs/AI-INTERACTION-SUMMARY.md`
- `ai/logs/ai-interactions.jsonl`

Say:
- “This workflow uses multiple AI roles instead of one generic assistant.”
- “We have planning, implementation, testing, QA, and review responsibilities, which makes the work traceable and reviewable.”
- “The idea is not to let AI operate in a black box; it is to make each step visible and auditable.”

Evidence:
- AI log and summary files show workflow stages

## 5) Spec Kit and requirement capture

Show:
- `specs/role-based-invoice-approval/spec.md`
- `plan.md` and `tasks.md`

Say:
- “The feature started from a specification, not from ad hoc coding. We captured the requirements, acceptance criteria, and edge cases before implementation.”
- “This is the part where we clarify the approval rules, especially the high-value approval ambiguity and the required human sign-off.”

Files:
- `specs/role-based-invoice-approval/spec.md`
- `specs/role-based-invoice-approval/tasks.md`

Evidence:
- specification artifacts are visible in the repo

## 6) Implementation steps and commit flow

Show:
- Git log or source control panel
- `ApprovalService.cs`

Say:
- “The implementation was split into a couple of focused steps. One addressed the base approval policy; the next covered the stricter high-value approval and duplicate/immutability checks.”
- “This makes the change easier to review and makes the risk more understandable.”

Files/commands:
- `git log --oneline --decorate --graph`
- `backend/src/InvoiceApp.Domain/ApprovalService.cs`

Evidence:
- commit history and service logic show staged implementation

## 7) Unit test execution in console

Show:
- terminal with `dotnet test backend/InvoiceApp.sln`
- pass/fail test output

Say:
- “Now I’m running the unit tests. This validates the core approval rules in isolation.”
- “We are checking the thresholds, duplicate approver prevention, and failed approval immutability.”

Files/commands:
- `dotnet test backend/InvoiceApp.sln`
- `backend/tests/InvoiceApp.Tests/ApprovalServiceTests.cs`

Evidence:
- test summary is visible on screen
- output shows whether rules pass or fail

## 8) Playwright E2E execution and final wrap-up

Show:
- terminal running Playwright
- browser showing the rejected approval flow
- maybe a screenshot or report from `e2e/test-results`

Say:
- “Finally, I’m running the browser-level validation. This confirms the approval flow behaves correctly from the user’s perspective.”
- “The important proof is that the invoice remains unchanged when an invalid approval is rejected.”
- “This closes the loop: requirements, implementation, unit tests, browser validation, and human review.”

Files/commands:
- `cd e2e && npm test`
- `e2e/tests/invoice-approval-limit.spec.ts`

Evidence:
- Playwright summary and browser flow are visible
- rejection message and unchanged invoice state are shown

## Closing line

- “This demo shows how AI-assisted SDLC works in practice: we define requirements, implement the code, run tests, validate the user flow, and keep human review in the loop.”

This is enough for a clean 8-part demo, and it stays focused on the live app and the two key test executions.
