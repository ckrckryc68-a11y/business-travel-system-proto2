# Module Code-Name Map

## Architecture Summary

**Confirmed architecture:** This repository contains one Visual Studio solution and one .NET 8 Blazor WebAssembly project. The browser application owns startup, routing, session state, dashboard views, trip search, detail components, and prototype business behavior. Data, approvals, comments, costing values, and reporting figures are hard-coded or held in browser-memory state. No backend API, persistence project, database context, external integration layer, or test project was found.

**Inferred boundaries:** The nine code names below group responsibilities that are useful for future work. They are context selectors, not separate assemblies or file-access restrictions. Start with a named module, then follow dependencies and change other modules whenever correctness requires it.

## Part 1: Code Name Tree

```text
solution-root
├── app-composition
│   ├── responsibility: solution/project boundary, startup, DI, routing, and root layout
│   ├── important folders: BusinessTravelSystem.App/, BusinessTravelSystem.App/Layout/
│   └── important files: BusinessTravelSystemPrototype.sln, BusinessTravelSystem.App.csproj, Program.cs, App.razor
├── identity-session
│   ├── responsibility: prototype sign-in, in-memory session, page guards, and sign-out
│   ├── important folders: BusinessTravelSystem.App/Services/, BusinessTravelSystem.App/Pages/
│   └── important files: AuthSessionService.cs, Home.razor, Dashboard.razor, Find.razor
├── dashboard-operations
│   ├── responsibility: home dashboard, active-trip map, metrics, calendar, and capability panels
│   ├── important folders: BusinessTravelSystem.App/Pages/, BusinessTravelSystem.App/wwwroot/images/
│   └── important files: Dashboard.razor, app.css, business-travel-world-map.png
├── trip-search-workbench
│   ├── responsibility: trip search/filtering, result selection, detail orchestration, and trip records
│   ├── important folders: BusinessTravelSystem.App/Pages/
│   └── important files: Find.razor, Find.razor.css
├── trip-detail-view
│   ├── responsibility: application details, attachments, itinerary, flights, and arrangements
│   ├── important folders: BusinessTravelSystem.App/Pages/
│   └── important files: GeneralSection.razor, TripSection.razor, matching scoped CSS
├── costing-travel-budget
│   ├── responsibility: cost lines, release totals, currencies, and expense breakdown
│   ├── important folders: BusinessTravelSystem.App/Pages/
│   └── important files: CostingSection.razor, CostingSection.razor.css
├── approval-collaboration
│   ├── responsibility: approval state, workflow history, audit display, queue actions, and comments
│   ├── important folders: BusinessTravelSystem.App/Pages/
│   └── important files: HistorySection.razor, CommentsSection.razor, Find.razor
├── web-ui-foundation
│   ├── responsibility: browser host, global/scoped styling, layout, and static assets
│   ├── important folders: BusinessTravelSystem.App/Layout/, BusinessTravelSystem.App/wwwroot/
│   └── important files: index.html, app.css, MainLayout.razor, MainLayout.razor.css
└── build-deployment
    ├── responsibility: SDK/toolchain, locked dependencies, local launch, and GitHub Pages delivery
    ├── important folders: .github/workflows/, BusinessTravelSystem.App/Properties/
    └── important files: deploy-pages.yml, global.json, packages.lock.json, launchSettings.json
```

## `app-composition`

**Purpose:**
Defines the single-project solution boundary and composes the Blazor WebAssembly application: root components, dependency injection, routing, shared imports, and default layout.

**Primary paths:**

* `BusinessTravelSystemPrototype.sln`
* `BusinessTravelSystem.App/BusinessTravelSystem.App.csproj`
* `BusinessTravelSystem.App/Program.cs`
* `BusinessTravelSystem.App/App.razor`
* `BusinessTravelSystem.App/_Imports.razor`
* `BusinessTravelSystem.App/Layout/MainLayout.razor`
* `BusinessTravelSystem.App/Layout/MainLayout.razor.css`

**Important entry points:**

* Top-level host composition in `Program.cs`.
* Root-component and service registration.
* The `<Router>` in `App.razor`.
* `MainLayout` as the default route layout.

**Main responsibilities:**

* Declare the solution/project boundary and .NET 8 Blazor dependencies.
* Register `HttpClient` and `AuthSessionService`.
* Mount the app at `#app`, route pages, and render the not-found state.
* Supply shared Razor imports and the root layout.

**Depends on:**

* `identity-session`
* `web-ui-foundation`

**Used by:**

* Every runtime module and `build-deployment`.

**Shared or overlapping files:**

* `Program.cs` overlaps with `identity-session` through session-service registration.
* `MainLayout.razor` overlaps with `web-ui-foundation`.
* The solution and project files overlap with `build-deployment`.

**Typical future tasks:**

* Add an application-wide service, route, root component, or project.
* Upgrade the framework or reorganize application composition.

## `identity-session`

**Purpose:**
Owns the prototype sign-in experience, browser-memory session object, repeated page guards, and sign-out navigation.

**Primary paths:**

* `BusinessTravelSystem.App/Services/AuthSessionService.cs`
* `BusinessTravelSystem.App/Pages/Home.razor`
* `BusinessTravelSystem.App/Program.cs`
* `BusinessTravelSystem.App/Pages/Dashboard.razor`
* `BusinessTravelSystem.App/Pages/Find.razor`

**Important entry points:**

* `Home.HandleLogin`.
* `AuthSessionService.SignIn` and `SignOut`.
* `Dashboard.OnInitialized` and `Find.OnInitialized`.
* Page-level `SignOut` handlers.

**Main responsibilities:**

* Require nonempty username/password fields.
* Store authentication status, username, remember-me flag, and sign-in time in memory.
* Redirect signed-in users to `/dashboard` and unauthenticated protected-page visits to `/`.
* Clear session state on sign-out.

**Depends on:**

* `app-composition`
* `web-ui-foundation`

**Used by:**

* `dashboard-operations`
* `trip-search-workbench`

**Shared or overlapping files:**

* `Dashboard.razor` and `Find.razor` also belong to their feature modules because guards and user menus are embedded in those pages.
* `wwwroot/css/app.css` contains login and user-menu styles.

**Typical future tasks:**

* Replace prototype sign-in with real authentication.
* Centralize authorization, add roles, or persist sessions safely.

## `dashboard-operations`

**Purpose:**
Provides the authenticated home dashboard: active-trip map, summary metrics, charts, calendar, traveler lists, module navigation, and generic Apply/Approval/Reports prototype panels.

**Primary paths:**

* `BusinessTravelSystem.App/Pages/Dashboard.razor`
* `BusinessTravelSystem.App/wwwroot/css/app.css`
* `BusinessTravelSystem.App/wwwroot/images/business-travel-world-map.png`
* `BusinessTravelSystem.App/wwwroot/images/app-mark.png`
* `BusinessTravelSystem.App/wwwroot/images/tdk-logo.png`

**Important entry points:**

* Route `/dashboard`.
* `OnInitialized`, `OnParametersSet`, `OpenModule`, and `BackToDashboard`.
* Map drag/zoom/expand handlers and calendar selection handlers.
* In-file `Modules`, `ActiveTrips`, `MetricCards`, `MonthlyTrips`, and `CalendarDays` data.

**Main responsibilities:**

* Render the active-trip map, metrics, business-unit cost chart, monthly chart, and trip calendar.
* Show active travelers for selected dates.
* Navigate Find to `/find`.
* Host Apply, Approval, and Reports as query-selected generic panels.
* Provide the authenticated top bar and user menu.

**Depends on:**

* `app-composition`
* `identity-session`
* `trip-search-workbench` for Find navigation.
* `web-ui-foundation`

**Used by:**

* `identity-session` as the post-login destination.
* `trip-search-workbench` as its return destination and Apply shortcut target.

**Shared or overlapping files:**

* The Approval panel overlaps conceptually with `approval-collaboration`.
* The cost chart overlaps conceptually with `costing-travel-budget`, but uses separate hard-coded data.
* `app.css` is shared with `web-ui-foundation` and is a broad styling hotspot.

**Typical future tasks:**

* Change dashboard metrics, map behavior, charts, or calendar interactions.
* Connect dashboard data or split Apply/Approval/Reports into dedicated routed features.

## `trip-search-workbench`

**Purpose:**
Owns the dedicated trip-finding workspace, filters, result selection, detail-tab orchestration, prototype queue actions, and the central in-file trip record/catalog.

**Primary paths:**

* `BusinessTravelSystem.App/Pages/Find.razor`
* `BusinessTravelSystem.App/Pages/Find.razor.css`

**Important entry points:**

* Route `/find`.
* `RunSearch`, `MatchesSearch`, `ToggleMyApprovals`, and `ClearFilters`.
* `FilteredTrips`, `EffectiveSelectedTrip`, `SelectTrip`, and `SelectTab`.
* Public nested record `Find.BusinessTrip` and static `Trips` catalog.
* The component switch for General, Trip, Costing, History, and Comments.

**Main responsibilities:**

* Search by trip ID, traveler, destination, summary, purpose, department, or cost center.
* Filter local/overseas trips and trips requiring the current user's approval.
* Select a trip and coordinate all detail tabs.
* Hold transient approval/action state.
* Supply the shared `BusinessTrip` type consumed by child components.

**Depends on:**

* `app-composition`
* `identity-session`
* `trip-detail-view`
* `costing-travel-budget`
* `approval-collaboration`
* `web-ui-foundation`

**Used by:**

* `dashboard-operations` through navigation.
* Every selected-trip detail module through `Find.BusinessTrip`.

**Shared or overlapping files:**

* `Find.razor` overlaps with `identity-session` for its guard/user menu and with `approval-collaboration` for approval filtering and mutation.
* Its nested `Find.BusinessTrip` record is the main cross-feature coupling hotspot.
* Header/navigation markup overlaps with `dashboard-operations` and `web-ui-foundation`.

**Typical future tasks:**

* Add search fields, sorting, paging, or saved filters.
* Replace static data or extract `BusinessTrip` into a stable shared contract.

## `trip-detail-view`

**Purpose:**
Renders the selected trip's general application information and travel plan: business details, attachments, ownership, destinations, flights, and arrangements.

**Primary paths:**

* `BusinessTravelSystem.App/Pages/GeneralSection.razor`
* `BusinessTravelSystem.App/Pages/GeneralSection.razor.css`
* `BusinessTravelSystem.App/Pages/TripSection.razor`
* `BusinessTravelSystem.App/Pages/TripSection.razor.css`
* `BusinessTravelSystem.App/wwwroot/images/tdk-headquarters.png`
* `BusinessTravelSystem.App/wwwroot/images/airplane-takeoff.png`
* `BusinessTravelSystem.App/wwwroot/images/airplane-landing.png`
* `BusinessTravelSystem.App/wwwroot/images/dummy-traveler-avatar.svg`

**Important entry points:**

* Required `Trip` parameters on `GeneralSection` and `TripSection`.
* `TripSection.SecondaryVenue`, `DestinationAirport`, `DestinationCode`, and `TravelPeriod`.
* The General and Trip branches in `Find.razor`.

**Main responsibilities:**

* Present application, business, ownership, attachment, and estimated-cost details.
* Derive destination labels, airport codes, and display dates.
* Render prototype destinations, flights, arrangements, assignees, and file labels.

**Depends on:**

* `trip-search-workbench`
* `web-ui-foundation`

**Used by:**

* `trip-search-workbench`

**Shared or overlapping files:**

* Both components depend directly on `Find.BusinessTrip`.
* Static images overlap with `web-ui-foundation`.
* Estimated-cost and attachment presentation overlap conceptually with `costing-travel-budget`.

**Typical future tasks:**

* Change overview fields, attachment rules, itinerary stops, booking states, or travel arrangements.

## `costing-travel-budget`

**Purpose:**
Owns the selected trip's prototype costing view, including expense lines, currency grouping, release totals, and local/overseas allowance labels.

**Primary paths:**

* `BusinessTravelSystem.App/Pages/CostingSection.razor`
* `BusinessTravelSystem.App/Pages/CostingSection.razor.css`
* `BusinessTravelSystem.App/Pages/Find.razor`
* `BusinessTravelSystem.App/Pages/Dashboard.razor`

**Important entry points:**

* Required `Trip` parameter on `CostingSection`.
* Static `CostLines`, computed `ReleaseTotals`, `AllowanceTitle`, and formatting helpers.
* The Costing branch in `Find.razor`.

**Main responsibilities:**

* Display release totals grouped by currency.
* Present hotel, transportation, and traveler-allowance categories.
* Format prototype monetary values and vary the allowance label by trip category.

**Depends on:**

* `trip-search-workbench`
* `web-ui-foundation`

**Used by:**

* `trip-search-workbench`
* `dashboard-operations` conceptually for cost reporting.

**Shared or overlapping files:**

* `Find.razor` supplies the trip contract and hosts the tab.
* `Dashboard.razor` has an independent hard-coded cost chart rather than a shared calculation source.

**Typical future tasks:**

* Add expense rules, exchange rates, approval limits, persistence, or reconciled dashboard totals.

## `approval-collaboration`

**Purpose:**
Combines prototype approval actions with workflow history, audit-log presentation, and trip comments because they share the selected trip and transient component state.

**Primary paths:**

* `BusinessTravelSystem.App/Pages/HistorySection.razor`
* `BusinessTravelSystem.App/Pages/HistorySection.razor.css`
* `BusinessTravelSystem.App/Pages/CommentsSection.razor`
* `BusinessTravelSystem.App/Pages/CommentsSection.razor.css`
* `BusinessTravelSystem.App/Pages/Find.razor`
* `BusinessTravelSystem.App/Pages/Dashboard.razor`

**Important entry points:**

* `Find.ApproveTrip`, `IsApproved`, `IsForMyApproval`, and the approval-only filter.
* `HistorySection.FinalApprovalStatus`, `CompletionPercent`, and `CurrentStageLabel`.
* `CommentsSection.SubmitComment` and `CommentCount`.
* The History and Comments branches in `Find.razor`.

**Main responsibilities:**

* Identify and approve trips in the current user's prototype queue.
* Render approval stages, status-dependent completion, and audit events.
* Accept and immediately display a transient comment.
* Surface Approval navigation/counts on the dashboard.

**Depends on:**

* `trip-search-workbench`
* `identity-session` conceptually for current-user context.
* `web-ui-foundation`

**Used by:**

* `trip-search-workbench`
* `dashboard-operations` conceptually through its Approval panel.

**Shared or overlapping files:**

* Approval filtering/actions live in `Find.razor`, while generic Approval navigation/counts live in `Dashboard.razor`.
* The comments component hard-codes the displayed current-user name rather than reading `identity-session`.

**Typical future tasks:**

* Add approval rules, stages, delegation, authorization, persistence, audit events, mentions, or notifications.

## `web-ui-foundation`

**Purpose:**
Provides the browser host, root mount point, global and scoped style system, shared layout, static assets, and small global pointer-tracking behavior.

**Primary paths:**

* `BusinessTravelSystem.App/wwwroot/index.html`
* `BusinessTravelSystem.App/wwwroot/css/app.css`
* `BusinessTravelSystem.App/wwwroot/css/bootstrap/`
* `BusinessTravelSystem.App/wwwroot/images/`
* `BusinessTravelSystem.App/wwwroot/favicon.png`
* `BusinessTravelSystem.App/wwwroot/icon-192.png`
* `BusinessTravelSystem.App/Layout/MainLayout.razor`
* `BusinessTravelSystem.App/Layout/MainLayout.razor.css`
* `BusinessTravelSystem.App/Pages/*.razor.css`

**Important entry points:**

* The `#app` host and Blazor boot script in `index.html`.
* Global CSS variables and cross-page selectors in `app.css`.
* The pointer-move script that updates cursor-position CSS variables.
* `MainLayout` and component-scoped CSS files.

**Main responsibilities:**

* Host the compiled app, loading UI, and error UI.
* Provide global theme, typography, responsive shell rules, common controls, and static assets.
* Provide scoped styling for feature components.

**Depends on:**

* `app-composition` for the compiled app, generated style bundle, and mount contract.

**Used by:**

* Every user-facing module and `build-deployment`.

**Shared or overlapping files:**

* `app.css` spans sign-in, dashboard, navigation, charts, calendar, and common controls and is the broadest visual maintenance hotspot.
* `MainLayout.razor` overlaps with `app-composition`.
* Feature `.razor.css` files remain primarily owned by their feature modules.

**Typical future tasks:**

* Change theme, breakpoints, accessibility, shared shell components, loading/error behavior, or assets.

## `build-deployment`

**Purpose:**
Defines SDK/tooling, dependency locking, local launch configuration, ignore rules, and GitHub Pages publication of the Blazor WebAssembly output.

**Primary paths:**

* `.github/workflows/deploy-pages.yml`
* `.gitignore`
* `global.json`
* `BusinessTravelSystemPrototype.vsconfig`
* `BusinessTravelSystemPrototype.sln`
* `BusinessTravelSystem.App/BusinessTravelSystem.App.csproj`
* `BusinessTravelSystem.App/packages.lock.json`
* `BusinessTravelSystem.App/Properties/launchSettings.json`
* `BusinessTravelSystem.App/wwwroot/index.html`

**Important entry points:**

* Push-to-`main` and manual workflow triggers.
* `dotnet restore` and `dotnet publish -c Release -o release`.
* Published base-href rewrite and Pages artifact deployment.
* SDK pinning in `global.json` and package locking in `packages.lock.json`.

**Main responsibilities:**

* Select the .NET 8 toolchain and restore/publish the single project.
* Prepare static output for the repository Pages path and disable Jekyll.
* Upload/deploy the `release/wwwroot` artifact.
* Define local launch profiles and ignored generated/local/secret files.

**Depends on:**

* `app-composition`
* `web-ui-foundation`

**Used by:**

* No runtime module; it delivers all runtime modules as one static application.

**Shared or overlapping files:**

* The solution/project files overlap with `app-composition`.
* `index.html` overlaps with `web-ui-foundation` and is rewritten only in published output.
* `packages.lock.json` is generated dependency metadata intentionally tracked for repeatability.

**Typical future tasks:**

* Repair/harden deployment, change SDK/dependencies, add build/test validation, or add deployment targets.

## Code-Name Quick Reference

| Code name | Responsibility | Primary paths | Related modules |
| --------- | -------------- | ------------- | --------------- |
| `app-composition` | Startup, DI, routing, solution/project boundary | `BusinessTravelSystemPrototype.sln`; `BusinessTravelSystem.App/Program.cs`; `BusinessTravelSystem.App/App.razor` | `identity-session`, `web-ui-foundation`, `build-deployment` |
| `identity-session` | Prototype login, in-memory session, guards, sign-out | `BusinessTravelSystem.App/Services/AuthSessionService.cs`; `BusinessTravelSystem.App/Pages/Home.razor` | `app-composition`, `dashboard-operations`, `trip-search-workbench` |
| `dashboard-operations` | Map, metrics, charts, calendar, navigation, prototype panels | `BusinessTravelSystem.App/Pages/Dashboard.razor`; `BusinessTravelSystem.App/wwwroot/css/app.css` | `identity-session`, `trip-search-workbench`, `approval-collaboration` |
| `trip-search-workbench` | Search/filter/results, selected-trip orchestration, trip records | `BusinessTravelSystem.App/Pages/Find.razor`; `BusinessTravelSystem.App/Pages/Find.razor.css` | `trip-detail-view`, `costing-travel-budget`, `approval-collaboration` |
| `trip-detail-view` | Application details, attachments, itinerary, bookings | `BusinessTravelSystem.App/Pages/GeneralSection.razor`; `BusinessTravelSystem.App/Pages/TripSection.razor` | `trip-search-workbench`, `web-ui-foundation` |
| `costing-travel-budget` | Expense lines, currencies, and release totals | `BusinessTravelSystem.App/Pages/CostingSection.razor` | `trip-search-workbench`, `dashboard-operations` |
| `approval-collaboration` | Approval actions/history, audits, comments | `BusinessTravelSystem.App/Pages/HistorySection.razor`; `BusinessTravelSystem.App/Pages/CommentsSection.razor`; `BusinessTravelSystem.App/Pages/Find.razor` | `trip-search-workbench`, `identity-session`, `dashboard-operations` |
| `web-ui-foundation` | Browser host, styles, layout, and assets | `BusinessTravelSystem.App/wwwroot/`; `BusinessTravelSystem.App/Layout/` | All user-facing modules, `app-composition`, `build-deployment` |
| `build-deployment` | SDK, dependency lock, local launch, Pages deployment | `.github/workflows/deploy-pages.yml`; `global.json`; `BusinessTravelSystem.App/packages.lock.json` | `app-composition`, `web-ui-foundation` |

## Cross-Module Relationships

### Runtime and user flow

1. `app-composition` starts the app, registers `AuthSessionService`, and routes pages.
2. `identity-session` signs a user in at `/` and sends the user to `dashboard-operations` at `/dashboard`.
3. `dashboard-operations` presents the home dashboard and opens `trip-search-workbench` at `/find`.
4. `trip-search-workbench` selects a `Find.BusinessTrip` and passes it to `trip-detail-view`, `costing-travel-budget`, and `approval-collaboration`.
5. Approval actions and comments remain in component memory and are lost after refresh/component recreation.
6. `web-ui-foundation` supplies the host, layout, CSS, and assets across the flow.

### Compile-time and data relationships

* All modules compile into one project, so there is no project-reference cycle.
* Razor references, injected services, navigation strings, and `Find.BusinessTrip` form the practical dependency graph.
* The strongest coupling is every detail component's dependency on the public nested `Find.BusinessTrip` record.
* Dashboard records, trip records, cost figures, workflow events, and comments are independent hard-coded/in-memory datasets; similarly named figures can diverge.
* No server, database, repository, durable browser storage, or external API data flow was found.

### Build and deployment flow

* `build-deployment` restores and publishes the single project.
* The workflow rewrites published `index.html` for the repository Pages path, uploads `release/wwwroot`, and deploys it as a static site.

### Boundary observations and hotspots

* **Broad responsibilities:** `Dashboard.razor` combines navigation, map behavior, charts, calendar, generic capability panels, and seeded data. `Find.razor` combines shell, search, data contract, seeded data, approval actions, selection state, and child composition.
* **Shared-type hotspot:** `Find.BusinessTrip` is physically inside a routed page but acts as the shared contract for five detail components.
* **Styling hotspot:** `wwwroot/css/app.css` has application-wide reach and overlaps with scoped CSS.
* **Repeated shell hotspot:** Dashboard and Find duplicate top-bar, module-menu, and user-menu markup.
* **Folder mismatch:** `BusinessTravelSystem.App/Pages/` contains routed pages and non-routed detail components.
* **Inferred capability boundary:** Apply, Approval, and Reports are named capabilities, but only Find has a dedicated route and detailed implementation. Generic panels stay under `dashboard-operations`; detailed approval behavior is under `approval-collaboration`.
* **Identity limitation:** Required-field validation is not credential authentication. Session state is in memory and guards are duplicated page code.
* **No test boundary:** No test project or repository test infrastructure was found, so no test code name is assigned.
* **No persistence/integration boundary:** No database, migration, repository, or external booking/API implementation was found. The registered `HttpClient` is not visibly consumed.

## Usage Examples

```text
In `trip-search-workbench`, add a date-range filter. Start with the mapped files,
but update `trip-detail-view` or `web-ui-foundation` when required.
```

```text
In `approval-collaboration`, prevent duplicate approvals and add an audit event.
Update `trip-search-workbench`, `identity-session`, or persistence code when
correctness requires it.
```

```text
Review `identity-session` for authorization gaps and follow connected call and
navigation paths into `dashboard-operations` and `trip-search-workbench`.
```

```text
In `costing-travel-budget`, calculate totals from trip expense data and reconcile
the dashboard chart by inspecting `dashboard-operations` and the shared contract.
```

```text
In `web-ui-foundation`, extract the repeated top bar. Update both dashboard and
search modules and explain the cross-module changes in the final summary.
```

```text
In `build-deployment`, add build verification. Inspect `app-composition` and any
new test/tool configuration needed for reliable validation.
```

## Map Metadata

* **Repository name:** `business-travel-system-proto2`
* **Repository full name:** `ckrckryc68-a11y/business-travel-system-proto2`
* **Branch analyzed:** `main`
* **Commit analyzed:** `62cc7f814e1826dc316d9ba6e7294bedd9dbb687`
* **Date generated:** `2026-07-19`
* **Solution files discovered:** `BusinessTravelSystemPrototype.sln`
* **Project files discovered:** `BusinessTravelSystem.App/BusinessTravelSystem.App.csproj`
* **Number of code-name modules:** 9
* **Documentation file path:** `docs/module-code-map.md`
* **Path verification:** At analysis start the repository had five commits. Mapped paths were verified from the full solution-addition commit, the three later pre-document commits (which changed only `.github/workflows/deploy-pages.yml`), and current reads of principal files on `main`.
* **Analysis limitations:** A local clone and build could not be performed because the execution environment could not resolve `github.com`. Analysis used the connected GitHub repository API, complete commit diffs, and current file reads. Binary image contents and generated/dependency-heavy files were not semantically inspected. Runtime behavior is described only where established by inspected source/workflow files.

## AGENTS.md Module Map Maintenance Instruction

```markdown
## Module Code-Name Map

Before broad repository analysis, read `docs/module-code-map.md` and use it as the first navigation index for this solution.

When a task mentions a module code name:

1. Begin with that code name's mapped paths and important entry points.
2. Inspect other modules only when dependencies, call paths, tests, configuration, data flow, or correctness require it.
3. Treat code names as navigation aids, not file-access restrictions. Modify files outside the named module whenever the implementation requires it.
4. Explain significant cross-module changes in the final task summary.

Keep existing code names stable whenever their responsibilities have not materially changed. Update the module map after a major or significant architectural change, but do not regenerate or rewrite it for trivial edits.

A significant change is one or more of the following:

* A project is added, removed, renamed, split, or merged.
* A major directory is added, removed, renamed, or moved.
* A new business capability or subsystem is introduced.
* A module's responsibility changes materially.
* Important entry points move.
* Dependencies between major modules change.
* Authentication, persistence, integration, deployment, or application composition is substantially redesigned.
* A large refactor makes the mapped paths inaccurate.
* Several important files are moved across module boundaries.
* The solution or project structure changes.

For a significant change, use this update procedure:

1. Inspect the changed files and their direct dependencies first.
2. Determine which existing code-name entries are affected.
3. Update only the affected entries when possible.
4. Perform a broader repository scan only when the change affects the overall architecture.
5. Preserve valid existing code names.
6. Add a new code name only for a genuinely distinct responsibility.
7. Remove or rename a code name only when its responsibility no longer exists or has materially changed.
8. Refresh module relationships and map metadata.
9. Verify that every mapped file and directory still exists.
10. Mention the module-map update in the final task summary.

Do not refuse or avoid a necessary change because a file is mapped to another code name. The map identifies where analysis should start; it does not impose architecture or ownership rules.
```
