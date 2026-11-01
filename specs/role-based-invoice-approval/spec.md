# Feature Specification: Role-Based Invoice Approval

**Feature Branch**: `001-role-based-invoice-approval`

**Created**: 2026-11-02

**Status**: Draft

**Input**: User description: "Role-based invoice approval with manager and director limits, duplicate approver prevention, and immutable rejected approval behavior."

## User Scenarios & Testing

### User Story 1 - Manager approval within policy (Priority: P1)
A manager should be able to approve an invoice that is within their allowed amount threshold without requiring additional approval.

**Why this priority**: Manager approvals are a core business flow and represent the standard approval path for lower-value invoices.

**Independent Test**: A manager submits an approval for a 50,000 invoice and the invoice is approved with the expected role and amount checks.

**Acceptance Scenarios**:

1. **Given** a pending invoice with amount 50,000, **When** a manager approves it, **Then** the invoice is marked as Approved and the domain state is updated.
2. **Given** a pending invoice with amount 50,001, **When** a manager approves it, **Then** the approval is rejected with a clear reason and no invoice data changes.

---

### User Story 2 - Director approval and high-value policy (Priority: P1)
A director should be able to approve invoices within their threshold, and higher-value invoices should require a second distinct director approver.

**Why this priority**: Director thresholds and multi-approver enforcement are the highest-risk approval rules and are central to the business feature.

**Independent Test**: A director approves a 500,000 invoice successfully, while a 500,001 invoice requires a second valid director approval.

**Acceptance Scenarios**:

1. **Given** a pending invoice with amount 500,000, **When** a director approves it, **Then** the invoice is marked as Approved.
2. **Given** a pending invoice with amount 500,001, **When** a single approver attempts approval, **Then** validation fails until a second distinct director approves.
3. **Given** a pending invoice with amount 500,001, **When** the second approver is not a director, **Then** the approval is rejected and the invoice data remains unchanged.

---

### User Story 3 - Duplicate and rejected approval protection (Priority: P1)
The system must prevent duplicate approvals by the same user and must ensure rejected approval attempts never mutate invoice business data.

**Why this priority**: This is essential for data integrity and prevents fraud, duplicate processing, and partial approval state changes.

**Independent Test**: A second approval by the same user is rejected with a clear reason, and a rejected approval attempt leaves the invoice unchanged.

**Acceptance Scenarios**:

1. **Given** an invoice previously approved by a user, **When** the same user attempts to approve it again, **Then** the approval is rejected and the invoice remains unchanged.
2. **Given** a pending invoice, **When** a rejected approval attempt fails validation, **Then** status, decision note, approval history, and other relevant business data remain unchanged.

---

## Requirements

### Functional Requirements

- **FR-001**: The system MUST permit a Manager to approve invoices with amount less than or equal to 50,000.
- **FR-002**: The system MUST reject a Manager approval for invoices above 50,000 with a descriptive reason.
- **FR-003**: The system MUST permit a Director to approve invoices with amount less than or equal to 500,000.
- **FR-004**: The system MUST require a second distinct approver for invoices above 500,000.
- **FR-005**: The second approver for invoices above 500,000 MUST be a different Director.
- **FR-006**: The same user MUST NOT be able to approve the same invoice twice.
- **FR-007**: The system MUST reject invalid approval attempts with a clear, user-meaningful reason.
- **FR-008**: Invalid approval attempts MUST NOT modify invoice status, approval history, decision note, or other business-relevant invoice data.
- **FR-009**: Approval validation MUST occur before any invoice mutation in the domain layer.
- **FR-010**: Approval authorization MUST be based on actual user role and invoice amount, not just UI or route state.
- **FR-011**: The system MUST support deterministic domain-level error handling consistent with the existing HTTP mapping conventions.

### Key Entities

- **Invoice**: Represents an invoice with amount, status, approval data, and business metadata.
- **Approver**: Represents the actor performing approval, identified by authenticated user identity and role.
- **Approval Decision**: Represents a valid or rejected approval action with outcome and reason.

## Success Criteria

### Measurable Outcomes

- **SC-001**: Manager approvals are accepted up to 50,000 and rejected above that threshold.
- **SC-002**: Director approvals are accepted up to 500,000 and require a second distinct Director above that threshold.
- **SC-003**: Duplicate approval attempts are rejected consistently and do not change invoice state.
- **SC-004**: Rejected approval attempts leave invoice state unchanged and return a clear reason.
- **SC-005**: Unit tests cover all valid and invalid approval paths.
- **SC-006**: Playwright E2E tests cover the browser-visible approval flow and negative cases.

## Assumptions

- The original requirement text did not explicitly define the first approver role for invoices above 500,000; this feature assumes the first approver may be either a Manager or a Director, but the second approver must be a different Director.
- Approval history is tracked as part of the invoice or approval metadata so duplicate approvers can be identified reliably.
- The repo’s in-memory model is sufficient for the feature scope and no database infrastructure changes are required.
- A rejected approval attempt is treated as a failed business validation and not as a valid decision.
