# Project conventions for AI agents

This file is the onboarding guide for AI coding agents working in this repository.

## 1) Project overview

Confirmed current repository state:
- This is a small invoice approval demo app built to support live AI-in-SDLC demos.
- The application is deliberately simple: a .NET 8 API, a minimal static web UI, and an in-memory store.
- It is designed to run locally without a database and without external infrastructure.
- The purpose is to model invoice lifecycle management, approval decisions, and role-based access.

Proposed implementation scope:
- Extend the approval workflow to support manager and director approval limits.
- Keep the solution aligned with the existing layered architecture and avoid adding database or service infrastructure unless explicitly required.

## 2) Technology stack

Confirmed current repository state:
- Language: C#
- Framework: ASP.NET Core 8 Web API
- Frontend: static HTML, CSS, and JavaScript served from `wwwroot/`
- Testing: xUnit for .NET tests
- Browser automation: Playwright for end-to-end tests
- CI/CD: GitHub Actions
- Runtime: .NET 8

Important project files:
- `backend/InvoiceApp.sln` - solution file for .NET projects
- `backend/src/InvoiceApp.Api/InvoiceApp.Api.csproj` - ASP.NET Core app
- `backend/src/InvoiceApp.Domain/InvoiceApp.Domain.csproj` - domain library
- `backend/tests/InvoiceApp.Tests/InvoiceApp.Tests.csproj` - xUnit test project
- `e2e/package.json` - Playwright project config and scripts

## 3) Application architecture and layering

Confirmed current repository state:
- Domain layer: `backend/src/InvoiceApp.Domain`
  - Business rules and domain model live here.
  - `ApprovalService` owns the invoice lifecycle.
  - No web or storage concerns belong here.
- API layer: `backend/src/InvoiceApp.Api`
  - Controllers, auth middleware, session management, in-memory repository, and static UI.
- Test layer: `backend/tests/InvoiceApp.Tests`
  - xUnit tests for service logic.
- E2E layer: `e2e`
  - Browser tests with Playwright.

Core architectural expectations:
- Controllers are responsible for HTTP concerns only.
- Business rules belong in the domain project, not in controllers or `Program.cs`.
- Repositories are abstractions in the Domain project and implementations live in the API project.
- Services are registered in `Program.cs`.

Architecture pattern in the repo today:
- `ApprovalService` is the main domain service.
- `IInvoiceRepository` defines persistence behavior.
- `InMemoryInvoiceRepository` is the current implementation.
- Controllers call the service and map exceptions to HTTP codes.

Proposed implementation decisions:
- Keep approval-limit logic in `ApprovalService` and domain model code.
- Avoid putting business logic in controllers.
- If approval metadata becomes necessary, add it to the `Invoice` model rather than creating ad hoc controller state.

## 4) Important source code locations

Confirmed current repository state:
- `backend/src/InvoiceApp.Domain/ApprovalService.cs` - invoice submission, approval, and rejection logic
- `backend/src/InvoiceApp.Domain/Invoice.cs` - invoice model
- `backend/src/InvoiceApp.Domain/InvoiceStatus.cs` - invoice states
- `backend/src/InvoiceApp.Domain/IInvoiceRepository.cs` - repository contract
- `backend/src/InvoiceApp.Api/Program.cs` - app registration and middleware setup
- `backend/src/InvoiceApp.Api/InMemoryInvoiceRepository.cs` - in-memory persistence
- `backend/src/InvoiceApp.Api/Auth/SessionAuthMiddleware.cs` - bearer-token authorization middleware
- `backend/src/InvoiceApp.Api/Auth/RequireRoleAttribute.cs` - role enforcement
- `backend/src/InvoiceApp.Api/Auth/SessionStore.cs` - in-memory session store
- `backend/src/InvoiceApp.Api/Auth/UserSession.cs` - user/session model
- `backend/src/InvoiceApp.Api/Controllers/InvoicesController.cs` - invoice endpoints
- `backend/src/InvoiceApp.Api/Controllers/ApprovalsController.cs` - approval endpoints
- `backend/src/InvoiceApp.Api/Controllers/AuthController.cs` - login/logout endpoints
- `backend/src/InvoiceApp.Api/Controllers/ApiControllerBase.cs` - HTTP error mapping helper
- `backend/src/InvoiceApp.Api/wwwroot/index.html` - web UI shell
- `backend/src/InvoiceApp.Api/wwwroot/app.js` - browser logic
- `backend/tests/InvoiceApp.Tests/ApprovalServiceTests.cs` - unit tests for current approval service
- `backend/tests/InvoiceApp.Tests/FakeInvoiceRepository.cs` - repository stub for tests
- `e2e/playwright.config.ts` - Playwright setup
- `.github/workflows/ci.yml` - GitHub Actions CI config

## 5) Invoice approval business requirements

Confirmed current repository state:
- The app manages invoices in lifecycle states: `Draft`, `PendingApproval`, `Approved`, and `Rejected`.
- Invoices move from Draft to PendingApproval via submit.
- Approvals are currently done as a single action: approve or reject.
- A rejection requires a non-empty reason.
- Controllers require session authentication and role restrictions.

Current invoice lifecycle in the code:
- `Submit` changes a draft invoice to `PendingApproval`.
- `Approve` changes a pending invoice to `Approved`.
- `Reject` changes a pending invoice to `Rejected` and stores `DecisionNote`.

Critical business rules for this assignment:
- Managers can approve invoices up to 50,000.
- Directors can approve invoices up to 500,000.
- Invoices above 500,000 require two distinct approvers.
- The second approver must be a Director.
- The same user cannot approve the same invoice twice.
- Rejected approval attempts must return a clear reason and must not modify invoice data.

Proposed implementation decisions:
- Add explicit role-aware approval limit checks in `ApprovalService`.
- Preserve the current domain service pattern rather than moving authorization logic into controllers.
- Treat user identity and role as first-class parts of the decision process.
- Require all rejected approval attempts to fail fast with a descriptive validation error and leave the invoice unchanged.

## 6) Approval limits and authorization rules

Confirmed current repository state:
- `RequireRoleAttribute` checks session role membership against allowed roles.
- Existing roles in demo config are `Approver` and `Admin`.
- There is no `Manager` or `Director` role in the current repo.

Current auth config:
- `backend/src/InvoiceApp.Api/appsettings.json` contains demo users:
  - `demo.approver` / `ChangeMe123!` / `Approver`
  - `demo.admin` / `ChangeMe123!` / `Admin`

Required role model for this feature:
- `Manager` role with maximum approval limit of 50,000.
- `Director` role with maximum approval limit of 500,000.
- Approval of an invoice above 500,000 must require two distinct approvers.
- The second approver must be a `Director`.
- The same user must never count twice toward the approval threshold.

Proposed implementation decisions:
- Add new demo users and/or role metadata in `appsettings.json` for manager/director scenarios as needed for testing.
- Keep the `RequireRoleAttribute` pattern for endpoints, but add business-level authorization checks in the domain service.
- Do not permit approval logic based only on route or UI state; always validate using domain rules.

## 7) Duplicate approver restrictions

Confirmed current repository state:
- `ApprovalService.Approve` accepts `string approver` and stores it in `Invoice.DecidedBy`.
- There is no approval history or duplicate-check mechanism in the current model.

Required behavior for this assignment:
- The same user cannot approve the same invoice twice.
- The duplicate check must be enforced using the actual user identity and by invoice-specific state.
- Duplicate attempts must fail with a clear reason and must not alter the invoice.

Proposed implementation decisions:
- Add approval-tracking state to the invoice or approval record model if required to identify unique approvers.
- Treat the same username as a non-unique approver across the same invoice even if the approval is attempted from different requests or flows.
- Reject duplicate attempts before mutating invoice state.

## 8) Rejected approval immutability requirements

Confirmed current repository state:
- `ApprovalService.Reject` currently changes invoice status to `Rejected` and writes the decision note.
- `ApiControllerBase.Handle` maps `InvalidOperationException` to HTTP 409 and `ArgumentException` to HTTP 400.

Required behavior for this assignment:
- If an approval attempt is rejected for any business rule, the invoice must remain unchanged.
- The rejection must return a clear reason to the caller.
- No invoice field may be modified for a rejected approval attempt.
- Errors should be surfaced as domain exceptions and mapped to the existing HTTP layer conventions.

Proposed implementation decisions:
- Validate all approval preconditions before mutating the entity.
- If a request is invalid, throw a descriptive exception and return an explanatory message rather than partially updating the invoice.
- Prefer a domain-level guard pattern such as “validate first, then mutate” to prevent partial state updates.

## 9) Coding conventions and naming standards

Confirmed current repository state:
- Controllers live in `Controllers/` and use `api/[controller]` routes.
- Request bodies are record types declared inside controller classes.
- Controllers inherit `ApiControllerBase` and rely on `Handle(...)` for consistent error translation.
- Services and business logic are placed in the Domain project.
- Repositories are interfaces in Domain and implementations in API.
- AI guidance from this repo prefers clear names and no one-letter variables or `flag` parameters.

Required conventions for future changes:
- Keep business rules in `InvoiceApp.Domain`.
- Prefer explicit names over abbreviations.
- Keep domain logic separate from HTTP and UI code.
- One test class per class under test, following the pattern `<ClassUnderTest>Tests`.
- Use `FakeInvoiceRepository` in unit tests.
- Do not add business logic to `Program.cs` or to controllers.

## 10) Error handling requirements

Confirmed current repository state:
- `ApiControllerBase.Handle(...)` catches and maps exceptions as follows:
  - `KeyNotFoundException` -> HTTP 404 Not Found
  - `InvalidOperationException` -> HTTP 409 Conflict
  - `ArgumentException` -> HTTP 400 Bad Request
- Domain code throws exceptions for invalid operations and missing data.

Required behavior for this assignment:
- Approval limit violations, duplicate approver attempts, and invalid second approver scenarios should be surfaced as clear domain errors.
- Rejected approval attempts should not mutate invoice state.
- Error messages should clearly communicate why the action failed.
- Business rule failures should be data-safe and deterministic.

Proposed implementation decisions:
- Keep handling consistent with the existing HTTP mapping pattern.
- Prefer descriptive messages such as “Manager cannot approve invoices above 50,000.” or “Second approver must be a Director.”
- Avoid silently swallowing validation errors.

## 11) Unit testing instructions and actual test commands

Confirmed current repository state:
- Unit tests are under `backend/tests/InvoiceApp.Tests`.
- The existing test suite is xUnit-based.
- `ApprovalServiceTests` is the current unit test class for the service.
- The repository has a `FakeInvoiceRepository` test helper.

Required testing expectations:
- Add tests for every new business rule.
- Unit tests should validate role limits, duplicate approver rules, and immutable rejection behavior.
- Use `FakeInvoiceRepository` for service tests rather than a real database.

Actual commands used in this repo:
- Build solution:
  - `dotnet build backend/InvoiceApp.sln --configuration Release`
- Run tests:
  - `dotnet test backend/InvoiceApp.sln`
- Run tests without rebuilding:
  - `dotnet test backend/InvoiceApp.sln --configuration Release --no-build`
- Run app locally:
  - `dotnet run --project backend/src/InvoiceApp.Api`

Proposed test cases to add:
- Manager approval at 50,000 is valid.
- Manager approval above 50,000 is rejected.
- Director approval at 500,000 is valid.
- Director approval above 500,000 requires a second distinct approver.
- Second approver is not a Director -> reject.
- Same user cannot approve the same invoice twice.
- Rejected approval attempt leaves invoice unchanged.

## 12) Playwright E2E testing instructions

Confirmed current repository state:
- Playwright is already configured in `e2e/playwright.config.ts`.
- The app automatically starts with `dotnet run --project ../backend/src/InvoiceApp.Api` as a web server in Playwright.
- The base URL is `http://localhost:5000`.
- The project targets Chromium.

Playwright commands:
- Install dependencies and browser:
  - `cd e2e && npm install && npx playwright install chromium`
- Run E2E tests:
  - `cd e2e && npm test`
- Run headed tests:
  - `cd e2e && npm run test:headed`
- Open HTML report:
  - `cd e2e && npm run report`

E2E expectations:
- Use view-specific page objects in `e2e/pages/` when present.
- Prefer role-based locators such as `getByRole`, `getByLabel`, and accessible selectors.
- Do not use CSS selectors or XPath for new tests when better accessible alternatives exist.
- Validate the approval flows and authentication behavior through the browser UI.

## 13) Git branching and commit conventions

Confirmed current repository state:
- There is no repository-specific branch policy currently documented in the repo.
- The repo does not appear to enforce a custom Git convention in code or CI.

Recommended conventions for AI agent work:
- Use short-lived feature branches.
- Prefer branch names like:
  - `feature/invoice-approval-limits`
  - `fix/reject-approval-immutability`
  - `test/approval-limit-rules`
- Use conventional commit prefixes when possible:
  - `feat:` for new approval rules
  - `fix:` for bug fixes or invalid state handling
  - `test:` for unit or E2E coverage work
  - `docs:` for documentation or agent guidance updates

Proposed review expectation:
- Keep commits focused and atomic.
- Avoid mixing application source changes with documentation-only updates unless necessary.

## 14) Security and authorization expectations

Confirmed current repository state:
- Every API call is protected by a session-bearing middleware, except login.
- `SessionAuthMiddleware` checks for a valid bearer token on `/api` routes.
- `RequireRoleAttribute` enforces role access on controllers and actions.
- The app uses demo credentials and is not intended for production use.

Security expectations:
- Never expose secrets, credentials, or production data in code or docs.
- Keep role checks in the auth and domain layers.
- Do not bypass middleware or validation for approval actions.
- The current app uses in-memory session state, so credentials are not persisted to a database.

Required role/authorization behavior for this assignment:
- Approval actions must not be allowed to bypass role checks.
- The app must ensure that only authorized roles may approve at the relevant thresholds.
- Rejected approval attempts must be clear and must not modify invoice state.

## 15) Code review expectations

Confirmed current repository state:
- The project is intentionally simple and expects strong, readable domain logic.
- Reviewers will likely expect the same layer boundaries used throughout the repo.

Expected review quality for new work:
- Business logic remains in the domain layer.
- Controllers remain thin and HTTP-focused.
- Error messages are clear and user-meaningful.
- Tests cover the new rules and both positive and negative flows.
- The application behavior is explicit about approval limits, role permissions, and duplicate handling.
- The implementation does not mutate invoice state on rejected approval attempts.

Before finalizing a change, verify:
- Domain rules are covered by unit tests.
- The API still returns mapped HTTP responses.
- The web UI behavior remains consistent with the service logic.
- The new rules do not break current invoice creation, submission, or rejection flows.

## 16) Special instructions for this assignment

Confirmed current repository state:
- The project does not currently implement manager or director approval limits.
- The current approval service is intentionally minimal and not yet aligned with the business requirements below.

Proposed implementation plan:
- Keep the current repo structure and conventions.
- Implement threshold logic in `ApprovalService`.
- Add or update tests in `backend/tests/InvoiceApp.Tests/ApprovalServiceTests.cs`.
- Update auth configuration and demo user configuration only when necessary for testing.
- Do not modify application source code beyond the required domain, auth, and test files that support the approval-limit feature.
- Keep all changes consistent with the current architectural boundaries.

## 17) Summary

The repo is a small ASP.NET Core 8 invoice approval demo with a static HTML/JS UI, xUnit unit tests, and Playwright browser tests. The core business logic is in `ApprovalService`. The most important current architecture rule is to keep business logic in the domain layer, while controllers remain HTTP wrappers. For this assignment, the feature should be implemented in the domain layer and validated through unit tests and Playwright E2E checks, with all approval-limit and immutability rules enforced before any invoice mutation.
