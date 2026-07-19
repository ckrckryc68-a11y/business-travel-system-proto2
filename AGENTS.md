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

## C. Module Code Map

Before performing broad repository analysis, read:

`docs/module-code-map.md`

Use this file as the primary navigation index for the solution.

### When a Task Mentions a Module Code Name

1. Begin with the paths and important entry points mapped to that code name.
2. Inspect other modules only when dependencies, call paths, tests, configuration, data flow, architecture, or correctness require it.
3. Treat code names as navigation aids, not file-access restrictions.
4. Modify files outside the named module whenever the implementation requires it.
5. Explain significant cross-module changes in the final task summary.

### Code-Name Stability

Keep existing code names stable whenever their responsibilities have not materially changed.

Update the module map after a major or significant architectural change. Do not regenerate or rewrite the map for trivial edits.

### Significant Changes

A change is considered significant when one or more of the following occurs:

* A project is added, removed, renamed, split, or merged.
* A major directory is added, removed, renamed, or moved.
* A new business capability or subsystem is introduced.
* A module’s responsibility changes materially.
* Important entry points are moved.
* Dependencies between major modules change.
* Authentication is substantially redesigned.
* Persistence or data storage is substantially redesigned.
* External integrations are substantially redesigned.
* Deployment is substantially redesigned.
* Application composition is substantially redesigned.
* A large refactor makes the mapped paths inaccurate.
* Several important files are moved across module boundaries.
* The solution or project structure changes.

### Module-Map Update Procedure

For a significant change:

1. Inspect the changed files and their direct dependencies first.
2. Determine which existing code-name entries are affected.
3. Update only the affected entries whenever possible.
4. Perform a broader repository scan only when the change affects the overall architecture.
5. Preserve valid existing code names.
6. Add a new code name only for a genuinely distinct responsibility.
7. Remove or rename a code name only when its responsibility no longer exists or has materially changed.
8. Refresh module relationships and map metadata.
9. Verify that every mapped file and directory still exists.
10. Mention the module-map update in the final task summary.

### Important Interpretation Rule

Do not refuse, postpone, or avoid a necessary change because a file is mapped to another code name.

The module map identifies where analysis should begin. It does not impose architecture, ownership, or file-access restrictions.