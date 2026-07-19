# Module Code-Name Map

## Architecture Summary

**Confirmed architecture:** The repository contains one Visual Studio solution and one .NET 8 Blazor WebAssembly application project. The browser application owns startup, routing, session state, dashboard views, trip search, trip-detail components, and prototype business behavior. Data, workflow state, costing values, comments, and reporting figures are currently hard-coded or held in browser-memory component/service state. No backend API, persistence project, database context, external integration layer, or test project was found.

**Inferred module boundaries:** The nine code names below are practical context selectors derived from architectural responsibility. They are not separate assemblies or strict ownership boundaries. Future tasks should begin with the mapped paths, then follow dependencies and change other modules whenever correctness requires it.

## Part 1: Code Name Tree

```text
solution-root
├── app-composition
│   ├── responsibility: solution/project boundary, startup, dependency injection, routing, and root layout
│   ├── important folders: BusinessTravelSystem.App/, BusinessTravelSystem.App/Layout/
│   └── important files: BusinessTravelSystemPrototype.sln, BusinessTravelSystem.App.csproj, Program.cs, App.razor
├── identity-session
│   ├── responsibility: prototype sign-in, browser-memory session state, route guards, and sign-out
│   ├── important folders: BusinessTravelSystem.App/Services/, BusinessTravelSystem.App/Pages/
│   └── important files: AuthSessionService.cs, Home.razor, Dashboard.razor, Find.razor
├── dashboard-operations
│   ├── responsibility: home dashboard, active-trip map, metrics, calendar, and prototype capability panels
│   ├── important folders: BusinessTravelSystem.App/Pages/, BusinessTravelSystem.App/wwwroot/images/
│   └── important files: Dashboard.razor, app.css, business-travel-world-map.png
├── trip-search-workbench
│   ├── responsibility: trip search/filtering, result selection, detail orchestration, and prototype trip records
│   ├── important folders: BusinessTravelSystem.App/Pages/
│   └── important files: Find.razor, Find.razor.css
├── trip-detail-view
│   ├── responsibility: general application details, attachments, itinerary, flights, and arrangements
│   ├── important folders: BusinessTravelSystem.App/Pages/
│   └── important files: GeneralSection.razor, TripSection.razor, matching scoped CSS files
├── costing-travel-budget
│   ├── responsibility: prototype cost lines, release totals, currencies, and expense breakdown presentation
│   ├── important folders: BusinessTravelSystem.App/Pages/
│   └── important files: CostingSection.razor, CostingSection.razor.css
├── approval-collaboration
│   ├── responsibility: approval state, workflow history, audit presentation, queue actions, and comments
│   ├── important folders: BusinessTravelSystem.App/Pages/
│   └── important files: HistorySection.razor, CommentsSection.razor, Find.razor
├── web-ui-foundation
│   ├── responsibility: browser host, global styling, layout, static images, and shared visual behavior
│   ├── important folders: BusinessTravelSystem.App/Layout/, BusinessTravelSystem.App/wwwroot/
│   └── important files: index.html, app.css, MainLayout.razor, MainLayout.razor.css
└── build-deployment
    ├── responsibility: SDK/toolchain selection, locked dependencies, local launch settings, and GitHub Pages delivery
    ├── important folders: .github/workflows/, BusinessTravelSystem.App/Properties/
    └── important files: deploy-pages.yml, global.json, packages.lock.json, launchSettings.json
```

## `app-composition`

**Purpose:**
Defines the single-project solution boundary and composes the Blazor WebAssembly application. It establishes root components, dependency injection, routing, shared imports, and the pass-through root layout.

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
* `WebAssemblyHostBuilder.CreateDefault`, root-component registration, and service registration.
* The `<Router>` in `App.razor`.
* `MainLayout` as the default route layout.

**Main responsibilities:**

* Declare the one solution and one application project.
* Target .NET 8 and reference Blazor WebAssembly packages.
* Register `HttpClient` and `AuthSessionService`.
* Mount `App` at `#app` and add `HeadOutlet`.
* Route discovered Razor pages and render the not-found state.
* Supply shared Razor imports and the default layout.

**Depends on:**

* `identity-session` for the registered session service.
* `web-ui-foundation` for the browser mount point and layout/host contract.

**Used by:**

* `identity-session`
* `dashboard-operations`
* `trip-search-workbench`
* `trip-detail-view`
* `costing-travel-budget`
* `approval-collaboration`
* `build-deployment`

**Shared or overlapping files:**

* `BusinessTravelSystem.App/Program.cs` also belongs to `identity-session` because it registers `AuthSessionService`.
* `BusinessTravelSystem.App/Layout/MainLayout.razor` and `.razor.css` also belong to `web-ui-foundation` because they define the shared rendering shell.
* `BusinessTravelSystem.App/BusinessTravelSystem.App.csproj` also belongs to `build-deployment` because restore and publish depend on it.

**Typical future tasks:**

* Add an application-wide service or root component.
* Change route composition or the default layout.
* Split the solution into additional projects.
* Upgrade the target framework or Blazor package references.

## `identity-session`

**Purpose:**
Owns the prototype sign-in experience and browser-memory session object. It also covers the repeated page-level authentication checks and sign-out navigation used by protected views.

**Primary paths:**

* `BusinessTravelSystem.App/Services/AuthSessionService.cs`
* `BusinessTravelSystem.App/Pages/Home.razor`
* `BusinessTravelSystem.App/Program.cs`
* `BusinessTravelSystem.App/Pages/Dashboard.razor`
* `BusinessTravelSystem.App/Pages/Find.razor`

**Important entry points:**

* `Home.HandleLogin`.
* `AuthSessionService.SignIn` and `AuthSessionService.SignOut`.
* `Dashboard.OnInitialized` and `Find.OnInitialized` route guards.
* `Dashboard.SignOut` and `Find.SignOut`.

**Main responsibilities:**

* Validate that username and password fields are nonempty with data annotations.
* Store authentication status, username, remember-me selection, and sign-in timestamp in memory.
* Redirect successful sign-in to `/dashboard`.
* Redirect unauthenticated dashboard and find-page visits to `/`.
* Clear session state and return users to the sign-in page.

**Depends on:**

* `app-composition` for DI registration and routing.
* `web-ui-foundation` for the sign-in and shared-shell presentation.

**Used by:**

* `dashboard-operations`
* `trip-search-workbench`

**Shared or overlapping files:**

* `BusinessTravelSystem.App/Program.cs` overlaps with `app-composition` through service registration.
* `BusinessTravelSystem.App/Pages/Dashboard.razor` overlaps with `dashboard-operations` because the auth guard and user menu are embedded in the dashboard page.
* `BusinessTravelSystem.App/Pages/Find.razor` overlaps with `trip-search-workbench` for the same reason.
* `BusinessTravelSystem.App/wwwroot/css/app.css` overlaps with `web-ui-foundation` because it contains the login and user-menu styles.

**Typical future tasks:**

* Replace prototype sign-in with a real identity provider.
* Persist or restore sessions safely.
* Centralize route authorization instead of duplicating page guards.
* Add roles or permission checks for approvals and reports.

## `dashboard-operations`

**Purpose:**
Provides the authenticated home experience: active-trip map, summary metrics, charts, calendar, traveler lists, top-level module navigation, and prototype panels for Apply, Approval, and Reports.

**Primary paths:**

* `BusinessTravelSystem.App/Pages/Dashboard.razor`
* `BusinessTravelSystem.App/wwwroot/css/app.css`
* `BusinessTravelSystem.App/wwwroot/images/business-travel-world-map.png`
* `BusinessTravelSystem.App/wwwroot/images/app-mark.png`
* `BusinessTravelSystem.App/wwwroot/images/tdk-logo.png`

**Important entry points:**

* Route `/dashboard`.
* `Dashboard.OnInitialized` and `Dashboard.OnParametersSet`.
* `OpenModule`, `BackToDashboard`, and `SelectTrip`.
* Map handlers: `StartMapDrag`, `DragMap`, `ChangeMapZoom`, and `ToggleMapExpanded`.
* Calendar handlers: `SelectCalendarDay` and `BackToCalendar`.
* In-file `Modules`, `ActiveTrips`, `MetricCards`, `MonthlyTrips`, and `CalendarDays` data.

**Main responsibilities:**

* Render and operate the world-map view of active trips.
* Present local/overseas metrics, business-unit cost chart, monthly travel chart, and trip calendar.
* Show active travelers for selected dates.
* Route Find to `/find`.
* Render Apply, Approval, and Reports as generic dashboard-hosted prototype panels selected by query string or menu action.
* Provide the authenticated header and user sign-out menu.

**Depends on:**

* `app-composition`
* `identity-session`
* `trip-search-workbench` for navigation into the dedicated Find experience.
* `web-ui-foundation`

**Used by:**

* `identity-session` as the post-login destination.
* `trip-search-workbench` as the return destination and host for the Apply shortcut.
* `approval-collaboration` conceptually through the Approval panel and notification count.

**Shared or overlapping files:**

* `BusinessTravelSystem.App/Pages/Dashboard.razor` overlaps with `identity-session` for auth guards and sign-out.
* The Approval prototype panel overlaps conceptually with `approval-collaboration`; it does not yet share a dedicated workflow service.
* The cost chart overlaps conceptually with `costing-travel-budget`; its data is independently hard-coded in `Dashboard.razor`.
* `BusinessTravelSystem.App/wwwroot/css/app.css` overlaps with `web-ui-foundation` and is a broad cross-feature styling hotspot.

**Typical future tasks:**

* Change dashboard metrics, calendar behavior, or map interactions.
* Connect active trips and charts to live data.
* Turn Apply, Approval, or Reports into dedicated routed features.
* Consolidate dashboard and find-page header behavior.

## `trip-search-workbench`

**Purpose:**
Owns the dedicated trip-finding workspace, including query/category filters, approval-only filtering, result selection, tab orchestration, prototype queue actions, and the central in-file trip record/catalog.

**Primary paths:**

* `BusinessTravelSystem.App/Pages/Find.razor`
* `BusinessTravelSystem.App/Pages/Find.razor.css`

**Important entry points:**

* Route `/find`.
* `RunSearch`, `MatchesSearch`, `ToggleMyApprovals`, `ClearFilters`, and `ClearKeyword`.
* `FilteredTrips`, `SearchFilteredTrips`, and `EffectiveSelectedTrip`.
* `SelectTrip`, `SelectTab`, and the detail-component switch.
* Public nested record `Find.BusinessTrip`.
* Static prototype catalog `Trips`.

**Main responsibilities:**

* Search trips by ID, traveler, destination, summary, purpose, department, or cost center.
* Filter local/overseas trips and items requiring the current user's approval.
* Select a trip and coordinate General, Trip, Costing, History, and Comments tabs.
* Hold prototype approval/action state in component memory.
* Provide the shared `BusinessTrip` shape consumed by all detail components.
* Navigate between dashboard capabilities.

**Depends on:**

* `app-composition`
* `identity-session`
* `trip-detail-view`
* `costing-travel-budget`
* `approval-collaboration`
* `web-ui-foundation`

**Used by:**

* `dashboard-operations` through navigation to `/find`.
* `trip-detail-view` through `Find.BusinessTrip`.
* `costing-travel-budget` through `Find.BusinessTrip`.
* `approval-collaboration` through `Find.BusinessTrip` and approval queue state.

**Shared or overlapping files:**

* `BusinessTravelSystem.App/Pages/Find.razor` also belongs to `identity-session` because it contains an auth guard and sign-out.
* It overlaps with `approval-collaboration` because approval filtering, approval mutation, and queue action messages live in the search page.
* It overlaps with all trip-detail modules because their shared data contract is the nested `Find.BusinessTrip` record.
* Header markup and navigation overlap with `dashboard-operations` and `web-ui-foundation`.

**Typical future tasks:**

* Add search criteria, sorting, paging, or saved filters.
* Replace static trip records with an API or repository.
* Move `BusinessTrip` into a stable shared contract.
* Change result selection, responsive detail navigation, or queue actions.

## `trip-detail-view`

**Purpose:**
Renders the selected trip's general application information and travel plan. It covers business details, attachment presentation, ownership, destinations, flights, and on-ground arrangements.

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

* `GeneralSection` and its required `Trip` parameter.
* `TripSection` and its required `Trip` parameter.
* `TripSection.SecondaryVenue`, `DestinationAirport`, `DestinationCode`, and `TravelPeriod`.
* The `Find.razor` tab switch that instantiates these components.

**Main responsibilities:**

* Present application ID, travel period, estimated cost, category, business purpose, and ownership.
* Present the prototype attachment requirement and uploaded-file card.
* Derive destination labels, airport codes, and display dates.
* Render prototype destinations, flights, arrangements, assignees, and supporting file labels.

**Depends on:**

* `trip-search-workbench` for `Find.BusinessTrip` and component orchestration.
* `web-ui-foundation` for shared assets and visual conventions.

**Used by:**

* `trip-search-workbench`

**Shared or overlapping files:**

* Both Razor components depend directly on the nested `Find.BusinessTrip` type from `trip-search-workbench`.
* Static images overlap with `web-ui-foundation` because they live under the shared `wwwroot/images/` asset directory.
* Attachment and estimated-cost presentation overlap conceptually with `costing-travel-budget`; no shared data service currently connects them.

**Typical future tasks:**

* Change trip overview fields or attachment requirements.
* Add itinerary stops, booking states, or transport modes.
* Connect travel arrangements and files to live records.
* Improve date parsing and destination/airport mapping.

## `costing-travel-budget`

**Purpose:**
Owns the selected trip's prototype costing view, including expense lines, currency grouping, release totals, and local/overseas allowance labels.

**Primary paths:**

* `BusinessTravelSystem.App/Pages/CostingSection.razor`
* `BusinessTravelSystem.App/Pages/CostingSection.razor.css`
* `BusinessTravelSystem.App/Pages/Find.razor`
* `BusinessTravelSystem.App/Pages/Dashboard.razor`

**Important entry points:**

* `CostingSection` and its required `Trip` parameter.
* Static `CostLines` and computed `ReleaseTotals`.
* `AllowanceTitle`, `FormatAmount`, and `FormatCost`.
* The Costing branch in the `Find.razor` detail-tab switch.

**Main responsibilities:**

* Display released totals grouped by currency.
* Present hotel, transportation, and traveler-allowance expense categories.
* Format prototype monetary values.
* Change the allowance label based on local versus overseas travel.

**Depends on:**

* `trip-search-workbench` for `Find.BusinessTrip` and tab orchestration.
* `web-ui-foundation` for shared styling and layout behavior.

**Used by:**

* `trip-search-workbench`
* `dashboard-operations` conceptually for cost reporting, though the dashboard currently has separate hard-coded figures.

**Shared or overlapping files:**

* `BusinessTravelSystem.App/Pages/Find.razor` supplies the trip contract and hosts the Costing tab.
* `BusinessTravelSystem.App/Pages/Dashboard.razor` has a separate business-unit cost chart and is an overlapping reporting surface rather than a shared calculation source.
* `Find.BusinessTrip.EstimatedCost` and `CostBreakdown` are used here but defined inside `trip-search-workbench`.

**Typical future tasks:**

* Add expense categories or approval limits.
* Implement exchange-rate and total calculations.
* Reconcile dashboard cost reporting with trip-level costing.
* Persist budget, release, and reimbursement data.

## `approval-collaboration`

**Purpose:**
Combines the prototype approval queue/actions with workflow history, audit-log presentation, and trip comments. These responsibilities are grouped because they share the same selected trip and currently use transient component state.

**Primary paths:**

* `BusinessTravelSystem.App/Pages/HistorySection.razor`
* `BusinessTravelSystem.App/Pages/HistorySection.razor.css`
* `BusinessTravelSystem.App/Pages/CommentsSection.razor`
* `BusinessTravelSystem.App/Pages/CommentsSection.razor.css`
* `BusinessTravelSystem.App/Pages/Find.razor`
* `BusinessTravelSystem.App/Pages/Dashboard.razor`

**Important entry points:**

* `Find.ApproveTrip`, `IsApproved`, `IsForMyApproval`, and the approval-only filter.
* `HistorySection.FinalApprovalStatus`, `CompletedStageCount`, `CompletionPercent`, and `CurrentStageLabel`.
* `CommentsSection.SubmitComment` and `CommentCount`.
* The History and Comments branches in the `Find.razor` detail-tab switch.

**Main responsibilities:**

* Identify trips awaiting the current user's approval.
* Record approvals in an in-memory `HashSet` for the current component instance.
* Render three approval stages and status-dependent completion state.
* Render prototype audit events.
* Accept and immediately display a transient comment.
* Surface Approval navigation and counts on the dashboard.

**Depends on:**

* `trip-search-workbench` for `Find.BusinessTrip`, selected-trip context, and queue state.
* `identity-session` conceptually for the current user; the comments component currently hard-codes the displayed current-user name.
* `web-ui-foundation` for shared visuals and avatar assets.

**Used by:**

* `trip-search-workbench`
* `dashboard-operations` conceptually through its Approval panel and notification badge.

**Shared or overlapping files:**

* `BusinessTravelSystem.App/Pages/Find.razor` is the main overlap because approval filtering/actions are embedded in the search workbench.
* `BusinessTravelSystem.App/Pages/Dashboard.razor` overlaps through the generic Approval panel and hard-coded notification count.
* `BusinessTravelSystem.App/wwwroot/images/dummy-traveler-avatar.svg` overlaps with `trip-detail-view` and `web-ui-foundation`.

**Typical future tasks:**

* Add approval rules, stages, delegation, or authorization.
* Persist approval decisions, audit events, and comments.
* Add mentions, notifications, or comment editing.
* Connect dashboard approval counts to the actual queue.

## `web-ui-foundation`

**Purpose:**
Provides the browser host, root mount point, global style system, shared layout, static assets, and small global pointer-tracking behavior used by the visual prototype.

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

* The `#app` host element and Blazor boot script in `wwwroot/index.html`.
* Global CSS custom properties and cross-page selectors in `wwwroot/css/app.css`.
* The pointer-move script that updates cursor-position CSS variables.
* `MainLayout` and each component's scoped CSS file.

**Main responsibilities:**

* Host the compiled Blazor application and error/loading UI.
* Provide global colors, typography, responsive shell rules, dashboard/login styles, and common controls.
* Supply images and icons used by pages and detail components.
* Provide scoped component styling for feature-specific views.
* Establish the root layout contract.

**Depends on:**

* `app-composition` for the compiled app, generated scoped-style bundle, and `#app` mount contract.

**Used by:**

* `identity-session`
* `dashboard-operations`
* `trip-search-workbench`
* `trip-detail-view`
* `costing-travel-budget`
* `approval-collaboration`
* `build-deployment`

**Shared or overlapping files:**

* `BusinessTravelSystem.App/wwwroot/css/app.css` spans sign-in, dashboard, navigation, charts, calendar, and common controls; it is the broadest visual maintenance hotspot.
* `MainLayout.razor` overlaps with `app-composition` as the default router layout.
* `wwwroot/images/` is shared by nearly every user-facing module.
* Feature `.razor.css` files remain primarily owned by their corresponding feature modules even though they are listed here as part of the styling mechanism.

**Typical future tasks:**

* Change global theme, typography, breakpoints, or focus behavior.
* Refactor shared headers and repeated navigation markup into components.
* Replace or optimize static assets.
* Improve accessibility, loading, error, or responsive behavior.

## `build-deployment`

**Purpose:**
Defines the repository's development toolchain, dependency locking, local launch configuration, ignore rules, and automatic GitHub Pages publication of the Blazor WebAssembly output.

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

* Push-to-`main` and manual triggers in `deploy-pages.yml`.
* Workflow restore and `dotnet publish -c Release -o release` steps.
* Base-href rewrite for the repository-specific GitHub Pages path.
* GitHub Pages artifact upload and deployment jobs.
* SDK pinning in `global.json` and package locking in `packages.lock.json`.

**Main responsibilities:**

* Select the .NET 8 SDK/tooling baseline.
* Restore and publish the single Blazor project.
* Prepare the static output for GitHub Pages and disable Jekyll processing.
* Upload and deploy the published `wwwroot` artifact.
* Define local development launch profiles.
* Exclude generated output, local databases, secrets, caches, and IDE state.

**Depends on:**

* `app-composition` for the solution/project and publishable application.
* `web-ui-foundation` for `wwwroot/index.html` and static output.

**Used by:**

* No runtime module. It delivers all runtime modules as one static browser application.

**Shared or overlapping files:**

* `BusinessTravelSystemPrototype.sln` and `BusinessTravelSystem.App/BusinessTravelSystem.App.csproj` overlap with `app-composition`.
* `BusinessTravelSystem.App/wwwroot/index.html` overlaps with `web-ui-foundation`; the deployment workflow rewrites its `<base href>` in the published artifact.
* `BusinessTravelSystem.App/packages.lock.json` is generated dependency metadata but is repository-specific and intentionally tracked for build repeatability.

**Typical future tasks:**

* Repair or harden the GitHub Pages workflow.
* Change SDK, framework, or dependency versions.
* Add build validation, tests, linting, or artifact checks.
* Add another deployment target or environment.

## Code-Name Quick Reference

| Code name | Responsibility | Primary paths | Related modules |
| --------- | -------------- | ------------- | --------------- |
| `app-composition` | Solution/project composition, startup, DI, routing, and root layout | `BusinessTravelSystemPrototype.sln`; `BusinessTravelSystem.App/Program.cs`; `BusinessTravelSystem.App/App.razor` | `identity-session`, `web-ui-foundation`, `build-deployment` |
| `identity-session` | Prototype login, in-memory session, page guards, and sign-out | `BusinessTravelSystem.App/Services/AuthSessionService.cs`; `BusinessTravelSystem.App/Pages/Home.razor` | `app-composition`, `dashboard-operations`, `trip-search-workbench` |
| `dashboard-operations` | Dashboard map, metrics, charts, calendar, navigation, and placeholder capability panels | `BusinessTravelSystem.App/Pages/Dashboard.razor`; `BusinessTravelSystem.App/wwwroot/css/app.css` | `identity-session`, `trip-search-workbench`, `approval-collaboration` |
| `trip-search-workbench` | Search/filter/result workflow, selected-trip orchestration, and prototype trip records | `BusinessTravelSystem.App/Pages/Find.razor`; `BusinessTravelSystem.App/Pages/Find.razor.css` | `trip-detail-view`, `costing-travel-budget`, `approval-collaboration` |
| `trip-detail-view` | General application details, attachments, destinations, flights, and arrangements | `BusinessTravelSystem.App/Pages/GeneralSection.razor`; `BusinessTravelSystem.App/Pages/TripSection.razor` | `trip-search-workbench`, `web-ui-foundation` |
| `costing-travel-budget` | Expense lines, currency totals, and budget presentation | `BusinessTravelSystem.App/Pages/CostingSection.razor` | `trip-search-workbench`, `dashboard-operations` |
| `approval-collaboration` | Approval queue/actions, workflow history, audit display, and comments | `BusinessTravelSystem.App/Pages/HistorySection.razor`; `CommentsSection.razor`; `Find.razor` | `trip-search-workbench`, `identity-session`, `dashboard-operations` |
| `web-ui-foundation` | Browser host, global/scoped styles, layout, and assets | `BusinessTravelSystem.App/wwwroot/`; `BusinessTravelSystem.App/Layout/` | All user-facing modules, `app-composition`, `build-deployment` |
| `build-deployment` | SDK, locked dependencies, local launch, ignore rules, and GitHub Pages workflow | `.github/workflows/deploy-pages.yml`; `global.json`; `packages.lock.json` | `app-composition`, `web-ui-foundation` |

## Cross-Module Relationships

### Runtime and user flow

1. `app-composition` starts the Blazor application, registers `AuthSessionService`, and routes pages.
2. `identity-session` signs a user in at `/` and navigates to `dashboard-operations` at `/dashboard`.
3. `dashboard-operations` presents the home dashboard and navigates Find to `trip-search-workbench` at `/find`.
4. `trip-search-workbench` filters its in-file trip catalog, selects a `Find.BusinessTrip`, and passes that record to `trip-detail-view`, `costing-travel-budget`, and `approval-collaboration` components.
5. Approval actions and posted comments remain in component memory; a refresh or component recreation discards them.
6. `web-ui-foundation` supplies the host, layout, CSS, and assets across the entire flow.

### Compile-time relationships

* There is one application project, so no project-reference graph or assembly-level circular dependency exists.
* Feature relationships are expressed through Razor component references, injected services, navigation strings, and a nested shared record rather than project boundaries or interfaces.
* The strongest compile-time feature coupling is every detail component's dependency on `Find.BusinessTrip`, a public nested record declared inside `Find.razor`.

### Data flow

* Dashboard records, trip search records, metrics, cost values, workflow events, and comments are independent hard-coded/in-memory datasets.
* `Find.razor` is the source of selected-trip data for all detail tabs.
* Dashboard cost/trip figures are not calculated from the trip catalog or costing component, so similarly named figures can diverge.
* No server, database, repository, durable browser storage, or external API data flow was found.

### Build and deployment flow

* `build-deployment` restores and publishes the single project.
* The workflow rewrites the published `index.html` base path for the GitHub Pages repository path, uploads `release/wwwroot`, and deploys it as a static site.

### Boundary observations and hotspots

* **Broad modules:** `Dashboard.razor` combines navigation shell, map behavior, charts, calendar, prototype module pages, and large in-file datasets. `Find.razor` combines shell/navigation, search, filtering, data contracts, seeded data, approval actions, responsive selection state, and child-component composition.
* **Shared-type hotspot:** `Find.BusinessTrip` is physically inside a routed UI page but functions as the shared trip contract for five components. This boundary is inferred and may be a useful extraction point in a future refactor, but no refactor is performed by this map.
* **Styling hotspot:** `wwwroot/css/app.css` has application-wide responsibility and overlaps with multiple scoped CSS files. Changes can have broad visual effects.
* **Repeated shell hotspot:** Dashboard and Find duplicate substantial top-bar, module-menu, and user-menu markup rather than consuming a shared shell component.
* **Folder mismatch:** `BusinessTravelSystem.App/Pages/` contains routed pages and non-routed detail components. Folder membership alone therefore does not define a route or module boundary.
* **Prototype capability ambiguity:** Apply, Approval, and Reports appear as named user capabilities, but only Find has a dedicated route and detailed implementation. Their current boundary is represented under `dashboard-operations`; approval detail behavior is separately mapped under `approval-collaboration`.
* **Identity ambiguity:** The sign-in form validates required fields but does not authenticate credentials. Session state is scoped browser memory and the route guards are duplicated page code, not a formal authorization subsystem.
* **No test boundary:** No test project or repository test infrastructure was found, so no `test-foundation` code name is assigned.
* **No persistence/integration boundary:** No database context, migration, repository, API client usage, or external booking/integration implementation was found. The registered `HttpClient` is composition infrastructure but is not visibly consumed by current features.

## Usage Examples

```text
In `trip-search-workbench`, add a date-range filter and keep the selected-trip
behavior stable. Start with the mapped files, but update `trip-detail-view` or
`web-ui-foundation` if the new filter changes shared presentation.
```

```text
In `approval-collaboration`, prevent duplicate approvals and add an audit event.
Start with the mapped files, but update `trip-search-workbench`,
`identity-session`, or a new persistence boundary when correctness requires it.
```

```text
Review `identity-session` for authorization gaps. Follow the call and navigation
paths into `dashboard-operations` and `trip-search-workbench`; code names are
starting points, not restrictions.
```

```text
In `costing-travel-budget`, calculate released totals from trip expense data and
reconcile the dashboard chart. Inspect `dashboard-operations` and the shared trip
contract before deciding where calculations should live.
```

```text
In `web-ui-foundation`, extract the repeated top bar into a reusable component.
Update both `dashboard-operations` and `trip-search-workbench` as necessary and
explain the cross-module changes in the final summary.
```

```text
In `build-deployment`, add a build verification step for the Blazor project.
Inspect `app-composition` and any introduced test project or tool configuration
needed to make the validation reliable.
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
* **Path verification:** Mapped paths were verified from the repository's complete five-commit history and representative current-file reads on `main`; the four commits after the initial solution addition changed only `.github/workflows/deploy-pages.yml`.
* **Analysis limitations:** A local clone and build could not be performed because the execution environment could not resolve `github.com`. Analysis used the connected GitHub repository API, the complete initial solution-addition diff, all later commit diffs, and current reads of the principal mapped files. Binary image contents and generated/dependency-heavy files were not semantically inspected. No claim is made about runtime behavior beyond what the inspected source and workflow establish.

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
