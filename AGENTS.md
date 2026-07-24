# AGENTS.md

## A. Terminology

### Home Laptop

* Device: MacBook Neo
* Not controlled by the company intranet.
* My personal computer used outside of work.
* ChatGPT and Codex may be given full access to the device, repository, files, terminal, and development tools.

### Mobile Phone

* Device: iPhone 15
* Not controlled by the company intranet.
* Uses mobile data through GOMO.
* My personal phone, which I may bring to work and use from time to time.
* ChatGPT and Codex may be given full access to available mobile features and connected services.

### Work Laptop

* Device: Lenovo ThinkPad E14
* Controlled and restricted by the company intranet.
* My company-issued laptop used while at work.
* Do not install third-party applications such as ChatGPT or Codex.
* ChatGPT and Codex cannot be accessed through the work laptop’s browser or company network.
* The laptop may be used to view the deployed GitHub Pages website.

---

## B. Personal Development Workflow

### At Home

1. **Home Laptop — Project Development**

   Make changes to the Business Travel System project:

   `https://chatgpt.com/g/g-p-6a5c31a384448191be32b9ad062bff1e-baba-business-travel-system/project`

2. **Home Laptop — Repository Management**

   View, edit, update, and delete files in the GitHub repository:

   `https://github.com/ckrckryc68-a11y/business-travel-system-proto2.git`

3. **Home Laptop — Deployment**

   Deploy the application through GitHub Pages.

4. **Home Laptop — Review**

   Visit and review the deployed application:

   `https://ckrckryc68-a11y.github.io/business-travel-system-proto2/`

### At Work

1. **Mobile Phone — Development Instructions**

   Use dictation or typed instructions to request changes through:

   `https://chatgpt.com/codex`

2. **Mobile Phone — Deployment**

   Ask Codex to run the GitHub Actions workflow:

   `Deploy Blazor WASM to GitHub Pages`

3. **Work Laptop — Review Only**

   Visit and review the deployed application:

   `https://ckrckryc68-a11y.github.io/business-travel-system-proto2/`

### Common Conditions and Goals

* Help prevent me from making decisions that could violate the company’s intranet, cybersecurity, data-handling, software-installation, or acceptable-use rules.
* Do not automatically stop work solely because an action may present a compliance concern.
* Clearly warn me before proceeding with an action that may violate company rules.
* Explain the specific risk in simple terms.
* Ask whether I want to continue when a safer compliant alternative is available.
* Prefer workflows that keep ChatGPT, Codex, source-code access, and repository changes on my personal devices and external services.
* Treat the Work Laptop as a review-only device unless I explicitly confirm that another activity is authorized.
* Never suggest bypassing company security controls, network restrictions, access controls, monitoring, or software-installation policies.
* Never transfer company-confidential information, credentials, private source code, internal documents, or intranet-only data to personal devices or external AI services without confirmed authorization.

---

## C. Database Schema Alignment

### Database Dictionary as a Required Reference

* Treat `DATABASE_DICTIONARY.md` as the current database reference and as a required factor in every upcoming project change.
* Before implementing a requested change, review whether it affects or assumes anything about persistence, entities, columns, relationships, lookup types, statuses, workflows, approvals, attachments, audit history, security, or retention.
* Do not silently work around, contradict, or expand the documented schema in application code.
* If a requested change conflicts with the database dictionary, exposes an unclear relationship, requires a missing table, column, lookup value, or constraint, or creates a data-integrity concern, notify the user before implementation.
* Explain the specific schema mismatch, its likely project impact, and the recommended schema adjustment so the database and application can be aligned first.
* Continue implementation only after the schema direction is aligned. Update the database dictionary and affected implementation together when the agreed change modifies the schema.
* Even when a request appears unrelated to the database, verify that it does not introduce a conflicting persistence assumption.

---

## D. Approval Flow Alignment

### Approval Flow as a Required Reference

* Treat `APPROVAL_FLOW.md` as the current business-process and approval-flow reference and as a required factor in every upcoming project change.
* Review requested changes against both `APPROVAL_FLOW.md` and `DATABASE_DICTIONARY.md`; neither document should be considered in isolation when a feature affects workflow, permissions, status transitions, approvals, cash advances, travel preparation, reporting, liquidation, cancellation, or attachments.
* Preserve the documented 13-step sequence unless the user explicitly approves a process revision.
* Treat the two Accounting Manager Approval entries as separate workflow steps with distinct sequence numbers and step codes.
* Do not silently change a role's responsibility, remove an approval, bypass a required control, or invent a transition that is not documented.
* Enforce or account for the documented special rules: Superior editing, GA process-wide access, the Cash Advance Review reversal, one-week advance notice, Clinic medical supplies, cancellation at any time with a required reason, direct PDF viewing, and receipt consolidation.
* If a requested change conflicts with the approval flow, exposes an ambiguous process rule, or cannot be represented safely by the current database schema, notify the user before implementation.
* Explain the process or schema mismatch, its likely impact, and the recommended alignment decision. Do not implement a workaround that causes the application, approval flow, and database dictionary to disagree.
* When an approved project change alters the workflow or schema, update `APPROVAL_FLOW.md`, `DATABASE_DICTIONARY.md`, and the affected implementation together.
