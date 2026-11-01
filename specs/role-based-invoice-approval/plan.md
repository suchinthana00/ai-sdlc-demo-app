# Implementation Plan: Role-Based Invoice Approval

**Branch**: `001-role-based-invoice-approval` | **Date**: 2026-11-02 | **Spec**: `specs/role-based-invoice-approval/spec.md`

**Input**: Feature specification from `specs/role-based-invoice-approval/spec.md`

## Summary

Implement role-aware invoice approval thresholds in the domain layer, enforce duplicate approver prevention and immutable rejection behavior, and validate the workflow with unit tests and Playwright E2E tests.

## Technical Context

**Language/Version**: C# / .NET 8

**Primary Dependencies**: ASP.NET Core 8, xUnit, Playwright

**Storage**: In-memory repository, no external database

**Testing**: xUnit for unit tests; Playwright for browser E2E

**Target Platform**: Local .NET web app

**Project Type**: Web API with static UI

**Performance Goals**: Local demo app; no large-scale performance requirements

**Constraints**: Business rules must remain in `InvoiceApp.Domain`; controllers remain thin; no database or service infrastructure migration

**Scale/Scope**: Single invoice approval workflow in a small demo app

## Constitution Check

This feature aligns with the constitution because it:

- keeps business rules in the domain layer,
- validates before mutation,
- enforces explicit authorization and data integrity,
- requires test coverage for positive and negative approval paths,
- documents the explicit assumption for the first approver above 500,000.

## Project Structure

```text
backend/
├── src/
│   ├── InvoiceApp.Api/
│   │   ├── Controllers/
│   │   ├── Auth/
│   │   └── wwwroot/
│   ├── InvoiceApp.Domain/
│   │   ├── ApprovalService.cs
│   │   ├── Invoice.cs
│   │   ├── InvoiceStatus.cs
│   │   └── IInvoiceRepository.cs
│   └── ...
├── tests/
│   └── InvoiceApp.Tests/
│       ├── ApprovalServiceTests.cs
│       ├── FakeInvoiceRepository.cs
│       └── ...
└── InvoiceApp.sln

e2e/
├── pages/
├── tests/
├── playwright.config.ts
└── package.json

specs/
└── role-based-invoice-approval/
    ├── spec.md
    ├── plan.md
    ├── research.md
    └── tasks.md
```

## Phase 0: Research and Clarification

- Confirm the open assumption: for invoices above 500,000, the first approver may be a Manager or Director, but the second approver must be a different Director.
- Confirm whether approval history is stored on the invoice record or represented as separate approval metadata.
- Confirm whether a rejected approval attempt must be fully atomic with no state mutation.

## Phase 1: Domain Design

### Validation Order
1. Verify invoice exists and is pending.
2. Verify approver identity is present.
3. Verify user is not already recorded as approver for this invoice.
4. Verify role-based amount threshold for the approver.
5. Verify second approver requirement for amounts above 500,000.
6. Mutate invoice state only after all checks pass.

### Domain Model Considerations
- Extend `Invoice` or an approval record structure to support unique approver tracking if needed.
- Keep the model in `InvoiceApp.Domain` and avoid controller state.
- Maintain immutability on failed validation by not mutating the invoice until all checks succeed.

## Phase 2: Service Implementation

- Update `ApprovalService.Approve` in `backend/src/InvoiceApp.Domain/ApprovalService.cs`.
- Add validation for:
  - manager threshold of 50,000,
  - director threshold of 500,000,
  - repeat approver rejection,
  - second approver requirement for large invoices,
  - second approver must be a Director,
  - no mutation when validation fails.

## Phase 3: API and UI Considerations

- Preserve `ApiControllerBase` mapping behavior and error semantics.
- Do not move business logic into controllers or `Program.cs`.
- Ensure the UI surfaces clear error messages from the domain layer.

## Phase 4: Test Strategy

### Unit Testing
Add tests in `backend/tests/InvoiceApp.Tests/ApprovalServiceTests.cs` for:

- Manager approval at 50,000 is valid.
- Manager approval over 50,000 is rejected.
- Director approval at 500,000 is valid.
- Director approval over 500,000 requires a second distinct approver.
- Second approver not a Director is rejected.
- Same user cannot approve same invoice twice.
- Rejected approval attempts do not mutate invoice state.

### E2E Testing
Add Playwright coverage in `e2e/tests/` for:

- Manager approval within limit.
- Manager approval over limit.
- Director approval within limit.
- Director approval above limit with second Director.
- Failure for invalid second approver.
- Duplicate approval attempt rejection.
- Rejection reason display and no state mutation.

## Complexity Tracking

No constitution violations require justification.
