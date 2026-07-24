# Approval Flow

> **TPC Business Travel System — Process Flow Report**

| Metadata | Value |
|---|---|
| Source worksheet | Process Flow |
| Classification | L2 - Internal Use Only |
| Generated | July 24, 2026 |

## Contents

1. [Purpose](#1-purpose)
2. [Process Flow](#2-process-flow)
3. [Role Responsibility Summary](#3-role-responsibility-summary)
4. [Key Workflow Controls and Business Rules](#4-key-workflow-controls-and-business-rules)
5. [End-to-End Flow Summary](#5-end-to-end-flow-summary)
6. [Database and Implementation Alignment Notes](#6-database-and-implementation-alignment-notes)
7. [Project Usage Rule](#7-project-usage-rule)

---

## 1. Purpose

This document presents the sequential business travel process, responsible roles, persons-in-charge (PICs), required information, and operational remarks recorded in the Project Plan workbook's **Process Flow** worksheet.

---

## 2. Process Flow

### Step 01 — Application

**Role / PIC:** Common

**Required information**

- Basic Information
- Cost Center
- Budget Justification
- Contract Arrangement
- Hotel Booking
- Cost Center (To Charge)

**Remarks:** None specified.

### Step 02 — Recommending Approval

**Role / PIC:** Superior

**Activities**

- Review and recommend the travel application.

**Remarks**

- The Superior can edit application details.

### Step 03 — Trip Preparation

**Role / PIC:** GA

**Activities**

- VISA
- Hotel Booking
- Flight Booking
- Shuttle Service Arrangement
- Host Arrangement

**Remarks**

- The GA role is open across the entire process flow.

### Step 04 — Cash Advance (Declaration)

**Role / PIC:** HR

**Activities**

- Allowance
- Contract Arrangement

**Remarks:** None specified.

### Step 05 — Cash Advance (Review)

**Role / PIC:** Accounting

**Activities**

- Allowance Review
- Trip Review

**Remarks**

- Reversal to Cash Advance (Declaration) will be allowed.

### Step 06 — Accounting Manager Approval

**Role / PIC:** Accounting

**Activities**

- Review and approve the cash advance/application.

**Remarks:** None specified.

### Step 07 — Vice President Approval

**Role / PIC:** Vice President

**Activities**

- Review and approve the application.

**Remarks:** None specified.

### Step 08 — HR Manager Approval

**Role / PIC:** HR

**Activities**

- Review and approve the application.

**Remarks:** None specified.

### Step 09 — Accounting Manager Approval

**Role / PIC:** Accounting

**Activities**

- Perform the second Accounting Manager approval shown in the source worksheet.

**Remarks**

- This approval appears a second time in the worksheet and is retained in the same sequence.

### Step 10 — CEO Approval

**Role / PIC:** CEO

**Activities**

- Final executive review and approval.

**Remarks:** None specified.

### Step 11 — Cash Advance (Release)

**Role / PIC:** Accounting

**Activities**

- Release the approved cash advance.

**Remarks**

- Notify one week in advance.
- Include medical supplies from the Clinic.
- The applicant can cancel the business trip at any time; a cancellation reason is required.

### Step 12 — Business Trip Report

**Role / PIC:** HRD

**Activities**

- Submit or review the business trip report after travel.

**Remarks**

- Attachments in PDF format can be viewed directly.

### Step 13 — Liquidation

**Role / PIC:** Accounting

**Activities**

- Consolidation of Receipts

**Remarks:** None specified.

---

## 3. Role Responsibility Summary

| Role / PIC | Responsibility |
|---|---|
| Common | Travel application details and initial arrangements |
| Superior | Recommending approval; may edit application details |
| GA | Trip preparation and support across the full process flow |
| HR | Cash advance declaration and HR Manager approval |
| Accounting | Cash advance review, Accounting Manager approvals, cash release, and liquidation |
| Vice President | Executive approval |
| CEO | Final executive approval |
| HRD | Business trip report handling |
| Clinic | Provides medical supplies for inclusion before travel |
| Applicant | May cancel the trip at any time with a required reason |

---

## 4. Key Workflow Controls and Business Rules

1. The Superior may edit the application's details during recommending approval.
2. GA access and responsibility remain open throughout the entire process flow.
3. Accounting may reverse the workflow from Cash Advance (Review) to Cash Advance (Declaration).
4. Cash advance release requires notice one week in advance.
5. Medical supplies from the Clinic must be included before the trip.
6. Applicants may cancel a business trip at any time, but must provide a reason.
7. Business Trip Report PDF attachments should support direct viewing.
8. Accounting Manager Approval occurs twice in the recorded sequence.

---

## 5. End-to-End Flow Summary

```text
Application
  -> Recommending Approval
  -> Trip Preparation
  -> Cash Advance (Declaration)
  -> Cash Advance (Review)
  -> Accounting Manager Approval (First)
  -> Vice President Approval
  -> HR Manager Approval
  -> Accounting Manager Approval (Second)
  -> CEO Approval
  -> Cash Advance (Release)
  -> Business Trip Report
  -> Liquidation / Consolidation of Receipts
```

---

## 6. Database and Implementation Alignment Notes

These observations identify where the current process is supported by `DATABASE_DICTIONARY.md` and where schema or implementation decisions must be aligned before related features are built. They are not automatic schema changes.

### Supported by the Current Schema

- The 13 ordered steps can be represented through `WORKFLOW`, `WORKFLOW_STEP`, and `TRIP_PROCESS`.
- The two Accounting Manager approvals can be represented as separate workflow steps with different `SEQUENCE_NO` and `STEP_CODE` values while using the same approver role.
- Approve, reject, reverse, and cancel actions can be represented through `WORKFLOW_ACTION`, `APPROVAL`, and `TRIP_ACTION`.
- Application edits by the Superior can be captured in `AUDIT_LOG`, provided role authorization and post-submission edit rules are explicitly enforced.
- Trip preparation activities such as hotel, shuttle, and host arrangements can generally use `ARRANGEMENT`; flights can use `FLIGHT`.
- Expense details and receipt attachments can partly use `TRIP_EXPENSE` and `ATTACHMENT`.

### Alignment Required Before Implementation

1. **Cash advance data model:** The schema has expense and workflow records but no dedicated cash advance declaration, review, approval, release, reversal, or settlement entity. Confirm whether to add a cash advance table set or formally model these values through `TRIP_EXPENSE` and lookup stages.
2. **Business Trip Report:** There is no dedicated report entity, report status, submission date, reviewer, or report-specific attachment parent. Decide whether a new report table is required.
3. **Liquidation:** `TRIP_EXPENSE` can hold expenses, but the schema does not explicitly represent liquidation submission, consolidated receipt totals, returned or reimbursable amounts, review outcome, or completion date.
4. **GA process-wide access:** `WORKFLOW_STEP` models a step approver, not a role with continuous access across all steps. This must be defined as an authorization rule or represented through an additional workflow-access model.
5. **Cancellation at any time:** `WORKFLOW_ACTION` and `TRIP_ACTION` can record cancellation, but transition rules must explicitly allow cancellation from every applicable status and require non-empty remarks as the cancellation reason.
6. **Reversal rule:** The permitted reversal from Cash Advance (Review) to Cash Advance (Declaration) needs an explicit transition rule so other unintended reversals are rejected.
7. **One-week advance notice:** Clarify what event requires seven days' notice and which date is the reference point. This may be a configurable `APP_SETTING` and a validation against the trip's earliest departure.
8. **Clinic medical supplies:** The schema has no explicit medical-supply checklist or Clinic handoff. Decide whether this is an `ARRANGEMENT` type, a task/checklist record, or another entity.
9. **Superior editing after submission:** Define which fields can be changed, whether approval resets are required, and how changes affect already-started workflow steps. All changes must remain auditable.
10. **HR and HRD roles:** Confirm whether HR and HRD are separate `ROLE` values and which modules they belong to.
11. **Contract Arrangement:** It appears in both Application and Cash Advance Declaration. Confirm whether both refer to the same arrangement record or to separate business data.
12. **Cost Center duplication:** Step 01 lists both “Cost Center” and “Cost Center (To Charge).” Confirm whether these are the same value or require two distinct fields. The current `TRIP` table has one `COST_CENTER_ID`.
13. **PDF direct viewing:** `ATTACHMENT` supports MIME type and BLOB content, but application security, browser headers, file-size limits, and inline-view permissions must be defined.

---

## 7. Project Usage Rule

Treat this document as the current approval and operational process reference. Every requested change must be checked against both `APPROVAL_FLOW.md` and `DATABASE_DICTIONARY.md`.

When a requested change contradicts the documented sequence, role responsibility, workflow control, or database model—or exposes an unresolved item listed above—raise the mismatch before implementation so the process and schema can be aligned first.
