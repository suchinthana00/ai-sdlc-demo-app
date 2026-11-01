# Tasks: Role-Based Invoice Approval

**Input**: Design documents from `specs/role-based-invoice-approval/`

## Phase 1: Setup and validation

- [ ] T001 Confirm the clarified assumption for high-value approvals: first approver may be a Manager or Director; second approver must be a different Director.
- [ ] T002 Review the current domain model and repository contract for approval state tracking.
- [ ] T003 Verify the existing unit and E2E test structure for approval flows.

## Phase 2: Domain model and service rules

- [ ] T010 Update the approval domain model in `backend/src/InvoiceApp.Domain/Invoice.cs` to support unique approver tracking if required.
- [ ] T011 Implement role-aware approval validation in `backend/src/InvoiceApp.Domain/ApprovalService.cs`.
- [ ] T012 Enforce manager approval limit of 50,000.
- [ ] T013 Enforce director approval limit of 500,000.
- [ ] T014 Enforce second-approver requirement for invoices above 500,000.
- [ ] T015 Enforce second approver must be a different Director.
- [ ] T016 Enforce same-user duplicate approval prevention.
- [ ] T017 Ensure rejected approval attempts do not change status, decision history, or business data.

## Phase 3: API and UI compatibility

- [ ] T020 Verify controllers still map domain exceptions to the expected HTTP behavior.
- [ ] T021 Ensure UI error messaging reflects clear business reasons for approval rejection.
- [ ] T022 Confirm no business rules were moved into controllers or `Program.cs`.

## Phase 4: Unit tests

- [ ] T030 Add unit test for Manager approval at 50,000.
- [ ] T031 Add unit test for Manager approval above 50,000 rejection.
- [ ] T032 Add unit test for Director approval at 500,000.
- [ ] T033 Add unit test for Director approval above 500,000 requiring a second distinct approver.
- [ ] T034 Add unit test for invalid second approver rejection.
- [ ] T035 Add unit test for duplicate approver rejection.
- [ ] T036 Add unit test verifying rejected approval leaves invoice unchanged.

## Phase 5: Playwright E2E tests

- [ ] T040 Add E2E test for manager approval within limit.
- [ ] T041 Add E2E test for manager approval above limit.
- [ ] T042 Add E2E test for director approval within limit.
- [ ] T043 Add E2E test for high-value invoice requiring a second Director.
- [ ] T044 Add E2E test for invalid second approver path.
- [ ] T045 Add E2E test for duplicate approver attempt failure.
- [ ] T046 Add E2E test for user-visible rejected approval immutability.

## Phase 6: Validation

- [ ] T050 Run `dotnet test backend/InvoiceApp.sln`.
- [ ] T051 Run `cd e2e && npm test`.
- [ ] T052 Review failures, fix defects, and recertify the workflow.
