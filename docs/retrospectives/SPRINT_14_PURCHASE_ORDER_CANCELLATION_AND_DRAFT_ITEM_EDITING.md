# Sprint 14 - Purchase Order Cancellation and Draft Item Editing Retrospective

## Sprint Status
`COMPLETE - CLOSED (T10, September 13, 2026)`

All gates passed: T01-T09 implemented and externally accepted; T10 documentation synchronization and closure complete. No version has been assigned and no tag or release has been created; the sprint is ready for external PR/release decision.

## Sprint Objective
Deliver the accepted Sprint 14 lifecycle completion for Purchase Orders:

- allow authorized cancellation of Draft and Submitted Purchase Orders
- make the existing `Cancelled` state reachable through the full stack
- expose Draft item update/remove through the Application and Web layers
- preserve the accepted authorization model and the existing architecture boundaries
- maintain the current purchasing domain and persistence conventions without scope expansion

## Incoming Baseline
Accepted incoming baseline for Sprint 14 planning (not newly re-executed):

- UnitTests: 314
- IntegrationTests: 85
- Web.Tests: 33
- Total: 432
- Failed: 0
- Skipped: 0
- Full rebuild warnings: 28
- Build errors: 0

## Locked Product Decisions

### Cancellation states
- `Draft -> Cancelled` allowed
- `Submitted -> Cancelled` allowed
- `Approved -> Cancelled` forbidden
- `Receiving -> Cancelled` forbidden
- `Completed -> Cancelled` forbidden
- `Cancelled -> Cancelled` forbidden

### Cancelled terminal behavior
`Cancelled` is terminal for Sprint 14.

A cancelled Purchase Order cannot:
- Submit
- Approve
- Receive
- Edit items
- Remove items
- return to Draft
- reopen

### Draft Edit scope
Dedicated Razor Pages surface:
- `Pages/Purchasing/PurchaseOrders/Edit.cshtml`
- `Pages/Purchasing/PurchaseOrders/Edit.cshtml.cs`

Draft item editing is limited to item mutation. It does not include general Purchase Order header editing.

### Draft item domain contract
- item lookup/mutation uses `ProductId`
- `UpdateItem` modifies Quantity and UnitCost only
- Product replacement is not supported
- Update/Remove require Draft state
- item-not-found produces the current `DomainException` contract
- final-item removal is allowed
- empty Draft may temporarily exist
- `Submit()` rejects a Purchase Order with no items

### Authorization capability names
- `PurchaseOrder.Edit`
- `PurchaseOrder.Cancel`

### Database / migration position
- no schema change
- no migration
- no data backfill
- additive authorization seed only
- no new provider/package
- EF Core InMemory limitations remain explicitly documented

### Web.Tests position
Web.Tests coverage must not be artificially grown if no current PageModel seam exists.

### Manual verification strategy
Manual browser verification is mandatory before Sprint 14 closure.

## Planned Task Sequence
- T01 - Sprint 14 Contract Verification and Implementation Readiness
- T02 - Domain Cancellation Transition
- T03 - Application Cancellation Workflow
- T04 - Application Draft Item Editing Workflow
- T05 - Purchase Order Edit/Cancel Authorization
- T06 - Purchase Order Persistence Integration Coverage
- T07 - Web Cancellation Workflow
- T08 - Web Draft Item Edit Workflow
- T09 - Integrated and Manual Verification
- T10 - Documentation Synchronization and Sprint 14 Closure

## Task Progress
- T01 - COMPLETE - implementation readiness verified and externally accepted
- T02 - COMPLETE - Domain cancellation transition implemented and verified
- T03 - COMPLETE - Application cancellation workflow implemented and verified
- T04 - COMPLETE - Application draft item editing workflow implemented and verified
- T05 - COMPLETE - Purchase Order Edit/Cancel authorization implemented and verified
- T06 - COMPLETE - Purchase Order persistence integration coverage implemented and verified
- T07 - COMPLETE - Web cancellation workflow implemented and verified
- T08 - COMPLETE - Web Draft item edit workflow implemented and verified
- T09 - COMPLETE - integrated and manual verification executed successfully
- T10 - COMPLETE - documentation synchronized and Sprint 14 closed

## Verification Ledger
Final accepted values (T09, externally accepted; unchanged by T10 — documentation-only):

- Build results: normal build passed (0 errors); full non-incremental build passed (28 pre-existing warnings, 0 errors; no Sprint 14 warning regression)
- UnitTests: 346 passed, 0 failed, 0 skipped
- IntegrationTests: 92 passed, 0 failed, 0 skipped
- Web.Tests: 39 passed, 0 failed, 0 skipped
- Total: 477 passed, 0 failed, 0 skipped (346 + 92 + 39)
- Manual browser verification: passed (all T09 cancellation, draft-editing, and authorization scenarios)
- Provider verification: passed (real SQL Server persistence confirmed through the running application; manual application/provider verification, not automated SQL Server integration testing)

## T02 Execution Evidence

- `PurchaseOrder.Cancel()` allows `Draft -> Cancelled` and `Submitted -> Cancelled`.
- `Approved`, `Receiving`, `Completed`, and `Cancelled` orders reject cancellation with `DomainException`.
- Cancelled orders reject Submit, Approve, Receive, UpdateItem, and RemoveItem through the existing Domain guards.
- Targeted PurchaseOrder Domain tests: 83 passed, 0 failed, 0 skipped.
- Full UnitTests: 325 passed, 0 failed, 0 skipped.
- Full solution tests: 443 passed, 0 failed, 0 skipped.
- Normal solution build: passed.
- Full non-incremental solution build: passed with 28 warnings and 0 errors, matching the accepted warning baseline.
- Graphify refresh: passed; code graph rebuilt with 6654 nodes, 11817 edges, and 518 communities.
- No Application, authorization, repository, migration, package, Web.Tests, or IntegrationTests source changes were made.

## T03 Execution Evidence

- Added the ID-only `CancelPurchaseOrderRequest` and `CancelPurchaseOrderResponse` contracts.
- Added `CancelPurchaseOrderHandler` using the existing repository, `PurchaseOrderErrors.NotFound`, aggregate `Cancel()`, and one `SaveChangesAsync` call on success.
- Domain cancellation validation remains exclusively in `PurchaseOrder.Cancel()`.
- Added 7 Application handler tests covering Draft and Submitted success, not-found handling, all forbidden states, save counts, and call order.
- Targeted T03 tests: 7 passed, 0 failed, 0 skipped.
- Full UnitTests: 332 passed, 0 failed, 0 skipped.
- Full solution tests: 450 passed, 0 failed, 0 skipped.
- Normal solution build: passed.
- Full non-incremental solution build: passed with 28 warnings and 0 errors, matching the accepted baseline.
- Graphify refresh: passed; 9 uncached code files re-extracted and graph rebuilt with 6677 nodes, 11888 edges, and 519 communities.
- No Domain, repository, Infrastructure, authorization, Web, IntegrationTests, migration, package, or project-reference changes were made.

## T04 Execution Evidence

- Added `UpdatePurchaseOrderItemRequest` / `UpdatePurchaseOrderItemResponse` / `UpdatePurchaseOrderItemHandler` and `RemovePurchaseOrderItemRequest` / `RemovePurchaseOrderItemResponse` / `RemovePurchaseOrderItemHandler` following the existing Submit/Approve/Cancel lifecycle handler conventions (positional `sealed record` contracts, `Result<TResponse>` returns).
- Both handlers load the aggregate via the existing `IPurchaseOrderRepository.GetByIdAsync`, return the existing `PurchaseOrderErrors.NotFound` when the Purchase Order is absent, delegate every item rule to the aggregate (`PurchaseOrder.UpdateItem` / `PurchaseOrder.RemoveItem`), and call `SaveChangesAsync` exactly once on success.
- Application duplicates no Domain rule: Draft-only enforcement, `ProductId` lookup, quantity/unit-cost invariants, item-not-found, and final-item-removal behavior remain exclusively in the `PurchaseOrder` aggregate. No repository `Update(...)` call, no manual item mutation, no totals recalculation in Application code.
- Failure paths verified with 0 saves: missing Purchase Order (NotFound result), unknown `ProductId`, Submitted order, Cancelled order, quantity <= 0, negative unit cost. Exact Domain exception messages preserved ("Purchase order item was not found.", "Only draft purchase orders can be modified.", "Quantity must be greater than zero.", "Unit cost cannot be negative.").
- Final-item removal succeeds and saves once; the empty-Draft and `Submit()` invariants remain Domain-owned and untouched.
- DI registration deliberately not added: the accepted T03 precedent left `CancelPurchaseOrderHandler` unregistered, and handler wiring belongs to the Web tasks (T07/T08). No project or package changes were made.
- Added 14 Application handler tests (8 Update + 6 Remove) reusing `FakePurchaseOrderRepository`, `FakeUnitOfWork`, `CallOrder`, and `PurchasingTestData` unchanged; no test-support modification was required.
- Targeted T04 tests: 14 passed, 0 failed, 0 skipped.
- Full UnitTests: 346 passed, 0 failed, 0 skipped.
- Full solution tests: 464 passed (346 UnitTests, 85 IntegrationTests, 33 Web.Tests), 0 failed, 0 skipped.
- Normal solution build: passed with 0 errors.
- Full non-incremental solution build: passed with 28 warnings and 0 errors, matching the accepted baseline; no T04-caused warnings (new files are warning-free).
- Graphify refresh: passed; 14 uncached code files re-extracted and graph rebuilt with 6803 nodes, 12153 edges, and 523 communities.
- No Domain, repository, Infrastructure, authorization, Web, IntegrationTests, migration, package, or project-reference changes were made.

## T05 Execution Evidence

- Added `PurchaseOrder.Edit` and `PurchaseOrder.Cancel` constants plus `EditPolicy` / `CancelPolicy` ("Capability:" + capability) to `AuthorizationPolicies.PurchaseOrder` in `InventoryPlatform.Web/Authorization/AuthorizationPolicies.cs`, preserving the exact singular `PurchaseOrder.<Action>` convention.
- Added both capabilities to the canonical catalog (`CapabilityCatalog.All` and the nested `PurchaseOrder` constant class) in `InventoryPlatform.Infrastructure/Identity/AuthorizationSeeder.cs`. This is the single capability source used by policy seeding and group assignment; seed rules themselves were untouched.
- Registered both policies in `AddWeb` (`InventoryPlatform.Web/Extensions/ServiceCollectionExtensions.cs`) using the existing `options.AddCapabilityPolicy(AuthorizationPolicies.ForCapability(...), ...)` mechanism. No new handlers, requirements, middleware, or composite authorization were added.
- Group assignment matrix emerged automatically from the existing filter-derived seed rules (no special-case seed logic): Administrator = all 41 catalog capabilities; InventoryManager = 23 (catalog minus User.*, activation/deactivation, Unit.Create, Administration.Access — none of which affect PurchaseOrder.*); Viewer = 15 (7 `.View`-suffixed capabilities + 7 `PurchaseOrder.*` + `User.View`). Group-capability relationships grew 73 → 79 (2 capabilities × 3 groups).
- Seed data changes only: additive `Capabilities` rows inserted by the existing idempotent `EnsureCapabilitiesAsync` mechanism on next startup. No schema change, no migration, no backfill.
- Test updates (expected-data updates preferred over repetitive one-off tests, per the task's test design rule):
  - `tests/InventoryPlatform.IntegrationTests/Authorization/AuthorizationSeederTests.cs`: updated expected capability/relationship/group counts (41 capabilities, 79 relationships, Administrator 41, InventoryManager 23, Viewer 15), extended the three PurchaseOrder capability-name arrays with Edit/Cancel, and added `SeedAsync_InventoryManagerReceivesPurchaseOrderEdit` / `SeedAsync_InventoryManagerReceivesPurchaseOrderCancel` proving the InventoryManager row of the matrix.
  - `tests/InventoryPlatform.Web.Tests/Authorization/PurchaseOrderCapabilityPolicyRegistrationTests.cs` (NEW, 6 tests): proves the Edit/Cancel capability constants exist with the exact singular naming, the EditPolicy/CancelPolicy constants follow the `Capability:<capability>` convention, and both policies resolve from the real `AddWeb` registration (`AuthorizationOptions` via `IOptionsFactory`) as a single `CapabilityRequirement` carrying the exact capability name.
- Targeted authorization tests: IntegrationTests `FullyQualifiedName~Authorization` filter — 62 passed, 0 failed, 0 skipped; Web.Tests (all authorization infrastructure tests) — 39 passed, 0 failed, 0 skipped (33 accepted T04 baseline + 6 legitimate T05 tests).
- Full UnitTests: 346 passed, 0 failed, 0 skipped.
- Full solution tests: 472 passed (346 UnitTests, 87 IntegrationTests, 39 Web.Tests), 0 failed, 0 skipped.
- Normal solution build: passed with 0 errors.
- Full non-incremental solution build: passed with 28 warnings and 0 errors, matching the accepted baseline. Two xUnit2000 warnings were transiently introduced by the new test file and corrected before final verification (expected/actual argument order); no T05 warning regression remains.
- Graphify refresh: passed; 11 uncached code files re-extracted and graph rebuilt with 6888 nodes, 12260 edges, and 530 communities.
- No Domain, Application workflow, repository, EF mapping, migration, package, project-reference, or Razor/Web page changes were made.

## T06 Execution Evidence

- Added `tests/InventoryPlatform.IntegrationTests/Purchasing/PurchaseOrderLifecyclePersistenceTests.cs` with 5 fresh-context persistence tests using the real `PurchaseOrderRepository`, the real `UnitOfWork` (`IUnitOfWork.SaveChangesAsync` — the production persistence boundary used by the T03/T04 handlers), and the existing EF Core InMemory per-test-database pattern. No new provider, package, fixture framework, or test-support abstraction was created.
- Persistence scenarios proven (each following the mandatory lifecycle: arrange + save → dispose context → fresh repository reload → real Domain mutation (`Cancel()` / `UpdateItem` / `RemoveItem`) → save via `UnitOfWork` → dispose mutation context → final fresh-context reload → assert):
  - `Cancel_DraftPurchaseOrder_PersistsCancelledStatus` — Draft → `PurchaseOrderStatus.Cancelled` after fresh reload.
  - `Cancel_SubmittedPurchaseOrder_PersistsCancelledStatus` — real `Submit()` baseline, then Submitted → `Cancelled` after fresh reload.
  - `UpdateItem_PersistsQuantityAndUnitCost` — combined quantity (3 → 7.25) and unit cost (10.00 → 4.50) update; baseline values verified from a fresh context before mutation; both new values explicitly proven after fresh reload (both-differences-in-one-test is the accepted combined-scenario form).
  - `RemoveItem_PersistsChildDeletion` — two-item Draft, one item removed by `ProductId`; after fresh reload: PO exists, remaining item present, removed `ProductId` absent, count decreased 2 → 1; plus direct child-set verification through `DbContext.PurchaseOrderItems` confirming the deleted child row no longer exists.
  - `RemoveItem_FinalItem_PersistsEmptyDraft` — single-item Draft; after fresh reload: PO exists, status remains Draft, `Items` empty, and direct child-set count through `DbContext.PurchaseOrderItems` is 0. Domain behavior unchanged — `Submit()` remains responsible for rejecting empty POs.
- Every persistence assertion used a context different from the one that performed the mutation; EF change-tracker state was never the evidence.
- Targeted T06 tests (`FullyQualifiedName~PurchaseOrderLifecyclePersistenceTests`): 5 passed, 0 failed, 0 skipped.
- Full solution tests: 477 passed (346 UnitTests, 92 IntegrationTests, 39 Web.Tests — 87 accepted T05 IntegrationTests baseline + 5 legitimate T06 tests; total corrected during T07 from the previously recorded 478, which was an arithmetic slip — 346 + 92 + 39 = 477), 0 failed, 0 skipped.
- Normal solution build: passed with 0 warnings and 0 errors.
- Full non-incremental solution build: passed with 28 warnings and 0 errors, matching the accepted baseline; no T06 warning regression.
- Graphify refresh: passed; 7 uncached code files re-extracted and graph rebuilt with 6977 nodes, 12417 edges, and 539 communities.
- Production changes: None. No Domain, Application, repository contract, EF configuration, Web, schema, migration, seed, package, or project-reference changes.
- EF Core InMemory limitation: these tests prove repository wiring, aggregate round-trip behavior, change tracking across fresh contexts, and delete persistence in the configured model. They do NOT prove SQL Server SQL translation, FK/cascade constraint enforcement at the relational level, transaction semantics, or provider-specific behavior. Provider-specific verification remains the T09 manual concern.

## T07 Execution Evidence

- Added `OnPostCancelAsync` to `DetailsModel` (`Pages/Purchasing/PurchaseOrders/Details.cshtml.cs`) following the established Submit/Approve/Receive programmatic authorization pattern exactly: class-level `[Authorize(Policy = ViewPolicy)]` page gate remains intact; the handler first evaluates `AuthorizationPolicies.ForCapability(AuthorizationPolicies.PurchaseOrder.Cancel)` via `IAuthorizationService.AuthorizeAsync(User, null, policy)` and returns `Forbid()` on failure; no handler-level `[Authorize]` attribute, no new authorization mechanism.
- The POST handler reuses the T03 `CancelPurchaseOrderHandler` (`CancelPurchaseOrderRequest(id)`) — the Web layer never calls `PurchaseOrder.Cancel()` directly, never touches repositories or `DbContext`, and never duplicates lifecycle rules. Call path: `Razor Page -> Application Handler -> Repository/UnitOfWork -> Domain`.
- Result handling matches existing purchasing convention exactly: failure → `ModelState.AddModelError` with the Application error message, re-fetch via `GetPurchaseOrderHandler`, `NotFound()` if the PO is missing, otherwise `Page()` (renders validation summary); success → `TempData["SuccessMessage"] = "Purchase Order '{Id}' was cancelled successfully."` + `RedirectToPage("./Details", ...)` preserving Search/FromDate/ToDate/Status/SortBy/Descending/PageNum/PageSize (established PRG pattern).
- `CancelPurchaseOrderHandler` DI registration added to `Application/DependencyInjection/ServiceCollectionExtensions.cs` (`AddScoped<CancelPurchaseOrderHandler>()`). This is the wiring explicitly deferred by T03/T04 to the Web tasks; the handler class itself is unchanged. The T05-verified `PurchaseOrder.Cancel` capability, policy constant, policy registration, and seed matrix are reused unchanged.
- `Details.cshtml` adds a Cancel form visible only when `Status is Draft or Submitted` (mirroring the status-gated Submit/Approve forms), styled `btn-outline-danger`, posting to the `Cancel` handler with the same hidden list-state fields. Confirmation UX uses the established project pattern (inline `onsubmit="return confirm(...);"` as in `TwoFactorAuthentication.cshtml`): "Cancel this purchase order? It will be marked as Cancelled and cannot be reopened." No new JavaScript framework or UI dependency. UI visibility is not the security boundary — the POST handler authorizes independently, so a direct POST from an unauthorized user still receives `Forbid()` and an invalid-state POST still surfaces the Domain error through the Application result.
- Cancelled presentation required no changes: Details and Index render `Status` directly in badges ("Cancelled" displays as-is), and Index `StatusOptions` plus the `GetPagedAsync` status filter already include `Cancelled`. Terminal UI behavior holds naturally: a Cancelled PO matches none of the status gates (Draft → Submit, Submitted → Approve, Approved/Receiving → Receive, Draft/Submitted → Cancel), so no workflow action forms render.
- Web.Tests: no new tests added. The accepted Sprint 14 Web.Tests Option B remains in force — PageModels depend on sealed concrete Application handler classes, so there is no valid seam for PageModel behavior testing without WebApplicationFactory/browser automation (both explicitly out of scope) or distorting production design for testability (forbidden). Cancel capability/policy registration was already covered by the T05 `PurchaseOrderCapabilityPolicyRegistrationTests` (not duplicated). Cancellation authorization enforcement, redirect/result behavior, NotFound handling, UI visibility, and confirmation UX are therefore preserved as T09 manual browser verification scenarios.
- Full solution tests: 477 passed (346 UnitTests, 92 IntegrationTests, 39 Web.Tests), 0 failed, 0 skipped — no test-count change; T07 adds production workflow wiring, not new test cases.
- Normal solution build: passed, 0 errors.
- Full non-incremental solution build: passed with 28 warnings and 0 errors, matching the accepted baseline; zero warnings originate from T07-touched files (`Details.cshtml`, `Details.cshtml.cs`, `CancelPurchaseOrderHandler`, DI `ServiceCollectionExtensions`).
- Graphify refresh: passed; 9 uncached code files re-extracted and graph rebuilt with 7042 nodes, 12494 edges, and 539 communities.
- No Domain, Application handler, repository contract, repository implementation, EF configuration, migration, seed, package, or project-reference changes. The only Application-layer change is the previously-deferred DI registration line.

## T08 Execution Evidence

- Created `Pages/Purchasing/PurchaseOrders/Edit.cshtml` and `Edit.cshtml.cs` — the dedicated Draft item edit page locked by Sprint 14 planning. Class-level `[Authorize(Policy = AuthorizationPolicies.PurchaseOrder.EditPolicy)]` gates the page; both POST handlers additionally perform programmatic `IAuthorizationService.AuthorizeAsync(... ForCapability(PurchaseOrder.Edit))` with `Forbid()` on failure, preserving the established two-layer pattern (no handler-level `[Authorize]`, no new authorization infrastructure). Link visibility on Details is not treated as the authorization boundary.
- GET loads through the existing `GetPurchaseOrderHandler`; missing Purchase Order → `NotFound()`; non-Draft → redirect to `./Details` (non-Draft is presented as invalid resource state, not an authorization failure; Domain guards remain active). The page renders Purchase Order ID, Supplier, Order Date, Status, and per-item SKU/Product/LineTotal with editable Quantity and UnitCost inputs. Totals come from the existing query contract (`GetPurchaseOrderItemResponse.LineTotal`, `GetPurchaseOrderResponse.TotalAmount`) — no pricing or Domain logic in Razor, no new lower-layer contracts.
- `OnPostUpdateItemAsync` and `OnPostRemoveItemAsync` reuse the T04 `UpdatePurchaseOrderItemHandler` / `RemovePurchaseOrderItemHandler` unchanged, with `ProductId` as the item key. Update modifies only Quantity and UnitCost; Product replacement is not offered or possible. The Web layer performs no entity mutation, no repository/`DbContext` access, and duplicates no Draft-state rules. Success → `TempData["SuccessMessage"]` + PRG redirect back to `./Edit` preserving Search/FromDate/ToDate/Status/SortBy/Descending/PageNum/PageSize via the established `[BindProperty(SupportsGet = true)]` + hidden-field pattern (same mechanism as T07).
- Failure handling uses the canonical message convention: `DomainException` from the aggregate (propagated through the unchanged T04 handlers exactly as the Submit/Approve/Receive guards propagate) is rendered verbatim via `ModelState.AddModelError(string.Empty, ...)` with page data reloaded through `GetPurchaseOrderHandler` and `Page()`; handler `Result` failure (missing Purchase Order) → `NotFound()`. A failure re-render for a Purchase Order that is no longer Draft redirects to `./Details` instead of rendering edit UI. No replacement business messages were fabricated. Quantity > 0 / UnitCost >= 0 remain Domain-owned (no FluentValidation validators exist for the T04 contracts; Domain is the accepted validation authority).
- Remove confirmation uses the established inline `onsubmit="return confirm('Remove this item from the purchase order?');"` pattern (as in T07 and `TwoFactorAuthentication.cshtml`). Final-item removal is not blocked; the empty Draft renders an info empty-state message and remains navigable; `Submit()` remains Domain-owned for rejecting empty POs.
- `Details.cshtml`: added a Draft-only "Edit Items" link (grouped with the existing Submit form, navigation state carried on the link) and a `ModelOnly` validation summary. The validation summary restores render visibility for Application/Domain error feedback that existing Details POST handlers (including the accepted T07 Cancel handler) add to ModelState but that previously had no summary markup on the page; no T07 behavior was changed.
- DI: `UpdatePurchaseOrderItemHandler` and `RemovePurchaseOrderItemHandler` registered (`AddScoped`) in `Application/DependencyInjection/ServiceCollectionExtensions.cs` following the deferred-wiring precedent used for `CancelPurchaseOrderHandler` in T07. Handler classes untouched.
- Web.Tests: no new tests added. The accepted Sprint 14 Web.Tests Option B remains in force — PageModels depend on sealed concrete Application handler classes with no valid seam, no WebApplicationFactory or browser automation is authorized, and no natural independently-testable Web component was introduced; creating abstractions solely for test-count growth is forbidden. Edit capability/policy registration was already covered by the T05 `PurchaseOrderCapabilityPolicyRegistrationTests`. Update/Remove authorization enforcement, validation feedback, PRG, totals, empty-Draft rendering, and confirmation UX are preserved as T09 manual browser verification scenarios (below).
- Focused verification: no new automated tests exist to run; Web.Tests Option B applies.
- Full solution tests: 477 passed (346 UnitTests, 92 IntegrationTests, 39 Web.Tests), 0 failed, 0 skipped — no test-count change; T08 adds production workflow wiring, not new test cases.
- Normal solution build: passed, 0 errors.
- Full non-incremental solution build: passed with 28 warnings and 0 errors, matching the accepted baseline; zero warnings originate from T08-touched files (`Edit.cshtml`, `Edit.cshtml.cs`, `Details.cshtml`, DI `ServiceCollectionExtensions`).
- Graphify refresh: passed; 10 uncached code files re-extracted and graph rebuilt with 7108 nodes, 12597 edges, and 549 communities (graph.json, graph.html, GRAPH_REPORT.md updated; curated-graph backup created).
- No Domain, T04 handler logic, repository contract/implementation, EF configuration, migration, seed, T07 cancellation behavior, package, or project-reference changes. The only Application-layer change is the two previously-deferred DI registration lines.

### T09 Manual Verification Deferred (T08 portion)

- Draft exposes Edit when authorized
- non-Draft does not expose Edit
- direct non-Draft Edit navigation does not permit mutation
- quantity update persists
- UnitCost update persists
- combined update persists correctly
- invalid quantity displays feedback
- invalid UnitCost displays feedback
- item removal persists
- final-item removal succeeds
- empty Draft Edit page renders correctly
- totals recalculate after mutation
- navigation/PRG works
- Cancelled cannot edit
- unauthorized persona is denied
- no stale state after mutation

## T09 Execution Evidence

T09 was verification-only: no production or test source was created or modified; no migration was created; no package/project changes were made. All manual scenarios were executed against the running application (Development environment, https profile) connected to the configured SQL Server database (`localhost`, trusted connection) — real provider persistence, not EF Core InMemory.

### Automated regression (re-executed)

- UnitTests: 346 passed, 0 failed, 0 skipped.
- IntegrationTests: 92 passed, 0 failed, 0 skipped.
- Web.Tests: 39 passed, 0 failed, 0 skipped.
- Total: 477 passed, 0 failed, 0 skipped — accepted post-T08 baseline exactly preserved.
- Normal solution build: passed, 0 errors.
- Full non-incremental solution build: passed, 28 warnings, 0 errors — accepted warning baseline matched; no Sprint 14 warning regression.

### Application startup and database verification

- Application started successfully and served authenticated traffic against the configured SQL Server database (the `localhost` default instance; the machine's LocalDB instance is not the configured target and was not used).
- Startup seeding ran idempotently: the `Capabilities` table grew 39 → 41 with `PurchaseOrder.Edit` and `PurchaseOrder.Cancel` appearing — additive T05 seed behavior exactly as accepted. No migration, no schema change, no schema drift, no data backfill.

### Cancellation manual verification

| Status | Cancel Visible | Cancel Attempt Result |
|---|---|---|
| Draft (PO 10) | Yes | Confirmation prompt shown; cancel succeeded (302 PRG); success feedback; badge `Cancelled`; status 6 persisted; no workflow actions remain |
| Submitted (PO 11) | Yes | Confirmation prompt shown; cancel succeeded; badge `Cancelled`; status 6 persisted; no workflow actions remain |
| Approved (PO 3) | No | No Cancel form rendered; crafted POST rejected by Domain |
| Receiving (PO 8) | No | No Cancel form rendered |
| Completed (PO 1) | No | No Cancel form rendered |
| Cancelled (PO 11) | No | No Cancel / Submit / Approve / Receive / Edit rendered (0 occurrences of each) |

- Cancelled presentation: Details and Index render the status as text in badges; no raw numeric enum value appears anywhere.
- Cancelled filter: Index filtered by `Cancelled` returned exactly the two cancelled POs (10, 11) and excluded all others; filter/paging state remained functional.

### Draft item editing manual verification

- Edit visibility: `Edit Items` appears on Details only for Draft; direct `Edit/{id}` navigation for Submitted/Approved/Receiving/Completed POs redirects (302) to Details. PO 6 (Draft) Edit page loads with SKU, product name, editable Quantity/UnitCost inputs, line totals, PO total, and a Back-to-Details link carrying navigation state.
- Quantity update (PO 6): before 10.00 → posted 12.00 → SQL-persisted 12.00 → later 15.00; success feedback rendered; PRG verified (302 with preserved `Search=running&Descending=True&PageNum=2&PageSize=10`).
- UnitCost update (PO 6): before 50.00 → 55.00 → 60.00, SQL-persisted at each step.
- Combined update: both fields changed in single posts; line total and PO total recalculated from persisted data (500.00 → 660.00 → 900.00) — posted form values were never treated as proof; SQL row inspection was used.
- Invalid quantity (0): POST returned the page with `Quantity must be greater than zero.` (canonical Domain message) in the validation summary; SQL row unchanged (no persistence).
- Invalid unit cost (-1): `Unit cost cannot be negative.` displayed; SQL row unchanged.
- Item removal (PO 10, two items): confirmation UX present (`Remove this item from the purchase order?`); removal succeeded (302 PRG, success feedback); SQL showed item count 2 → 1, removed product absent, remaining item intact; total recalculated 400.00 → 100.00.
- Final-item removal (PO 10): succeeded; SQL showed 0 items, PO still present and Draft; Edit page reloaded with the empty-state message and no crash.
- Empty-Draft submit guard (PO 10): Submit form still offered (pre-existing Details behavior); submission prevented with the canonical message `A purchase order must contain at least one item.`; status remained Draft (1).
- Provider-level persistence (SQL Server, not InMemory): updates/removals/cancellations persisted across page reloads AND a full application stop/start; no stale values restored after restart (verified through the UI and direct row inspection).

### Authorization and crafted-request verification

- Authorized persona: `viewer` (possesses all `PurchaseOrder.*` capabilities per the accepted T05 matrix) — Details and Edit load (200); quantity update persisted and was reverted through the UI; both Edit and Cancel workflows function.
- Denied persona: all readily available seeded purchasing personas hold the capability, and the remaining no-group account (`crisandrew`) has two-factor authentication enabled, so a dedicated no-capability user (`t09denied`, no authorization-group membership) was created through the application's own Administrator UI without any seed-matrix change — the limitation path explicitly allowed by the T09 contract.
- Denied results: direct GET to Details/Edit → redirect to `Identity/Account/AccessDenied`; crafted UpdateItem and Cancel POSTs carrying the denied user's own valid antiforgery token → redirect to `Identity/Account/AccessDenied`; SQL rows unchanged (quantity still 15.00, status Draft).
- UI hiding is not the protection: a crafted UpdateItem POST (admin session, valid token, Approved PO 3) returned a redirect with the Domain-originated failure and the SQL row remained 8.00 (posted value 99 was never persisted); a crafted Cancel POST on the same Approved PO was rejected by the Domain guard (`Only draft or submitted purchase orders can be cancelled.`); no silent remediation was performed or needed.

### Known limitation (deferred observation, pre-existing convention)

- When an Application/Domain failure reaches a Details POST handler (Submit/Approve/Receive/Cancel — including the T08 empty-Draft Submit attempt), the canonical message is displayed via ModelState, but the uncaught `DomainException` produces HTTP 500 with the message rendered by the Development developer-exception page. This is the application's pre-existing convention (the Web layer contains no `catch` anywhere and T07 did not alter it); the Sprint 14 Edit page renders equivalent Domain failures inline as accepted. Recorded here for external review; a possible remediation (catching `DomainException` in Details POST handlers) would be a separate task and was intentionally not made during T09.

## Key Decisions
- Cancellation is limited to Draft and Submitted states.
- Cancelled is terminal in Sprint 14.
- The edit surface is a dedicated Draft item edit page, not general PO editing.
- Authorization remains additive and follows the existing capability model.
- No architecture redesign is included in this sprint.
- Web.Tests additions are limited to valid existing seams only.

## Findings / Deferred Work
- EditStatus behavior remains deferred and blocked by a separate owner decision.
- `PagedRequest.Status` remains a known low-severity shared paging design issue and is not Sprint 14 work.
- T05 navigation reflection observation is not a production defect.
- WebApplicationFactory is explicitly out of Sprint 14 scope.
- SQL Server automated integration-test expansion is out of Sprint 14 scope.
- CI is not Sprint 14 work.
- Warning cleanup is not Sprint 14 work.
- Reorder / low stock feature is deferred.
- Sales work is deferred.

## What Went Well
- The locked planning contract (state machine, capability names, group matrix, Web.Tests position) held without a single source contradiction through T01-T09; every gate verified against current source rather than planning assumptions.
- Domain rules stayed Domain-owned end to end: Application handlers orchestrate, the Web layer never touches repositories or `DbContext`, and no business rule was duplicated into Razor or Application code.
- The two-layer authorization pattern (class-level policy gate + programmatic `IAuthorizationService.AuthorizeAsync` with `Forbid()`) was reused unchanged for both new capabilities; UI hiding was correctly treated as UX, not as the security boundary (crafted-POST verification proved it).
- Manual verification went beyond UI observation: SQL row inspection and an application stop/start cycle proved real provider persistence rather than trusting posted form values.
- The 478 → 477 arithmetic slip was caught and corrected during the sprint instead of propagating into closure documentation.

## What Could Be Improved
- The pre-existing Details POST convention lets some `DomainException` failures surface as HTTP 500 through the Development developer-exception page instead of inline validation; the T08 Edit page shows the inline pattern is achievable and the inconsistency is now a recorded deferred follow-up rather than a surprise.
- Web.Tests remain limited to policy-registration coverage because PageModels depend on sealed concrete Application handler classes; a PageModel seam (or WebApplicationFactory decision) is still an open testing-infrastructure question deferred since Sprint 13.
- EF Core InMemory integration coverage still proves wiring and round-trips only; relational provider behavior required manual verification, and automated SQL Server integration testing remains unbuilt.
- The T06 test-total arithmetic slip (478 recorded instead of 477) showed suite totals should be re-derived from per-suite numbers at every gate rather than carried forward.

## Lessons Learned
- Verify the planning contract against current source first (T01); zero mid-sprint contract surprises followed from that discipline.
- Deferred wiring is a valid pattern when it is explicit: handler DI registration was deliberately deferred from T03/T04 to the Web tasks and landed as three scoped registrations (Cancel in T07; UpdateItem/RemoveItem in T08) without touching handler classes.
- Reconciliation arithmetic matters: 346 + 92 + 39 = 477, and stating the arithmetic in evidence made the 478 slip self-detecting.
- Manual verification and automated testing answer different questions; labeling SQL Server provider evidence as "manual application/provider verification" (not automated end-to-end testing) kept the evidence classes honest.
- Documentation-only closure (T10) requires no test re-run when the accepted executable baseline is externally accepted and no executable file changed; the T09 evidence remains authoritative.

## Final Test Baseline
**Accepted final Sprint 14 baseline (T09, externally accepted): 477 passed, 0 failed, 0 skipped (346 UnitTests + 92 IntegrationTests + 39 Web.Tests); normal build 0 errors; full non-incremental build 28 warnings / 0 errors. T10 changed no executable file, so this baseline remains authoritative.**

Historical per-task verification results:

T09 verification results (accepted incoming T08 baseline: 477 tests, 28 full-rebuild warnings, 0 errors):

- UnitTests: 346 passed, 0 failed, 0 skipped
- IntegrationTests: 92 passed, 0 failed, 0 skipped
- Web.Tests: 39 passed, 0 failed, 0 skipped
- Total: 477 passed, 0 failed, 0 skipped
- Normal build: passed, 0 errors
- Full non-incremental build: 28 warnings, 0 errors (matches the accepted warning baseline; no Sprint 14 regression)
- Application startup against SQL Server: successful
- Manual browser/provider verification: passed (all T09 scenarios)

T08 verification results (accepted incoming T07 baseline: 477 tests, 28 full-rebuild warnings, 0 errors):

- UnitTests: 346 passed, 0 failed, 0 skipped
- IntegrationTests: 92 passed, 0 failed, 0 skipped
- Web.Tests: 39 passed, 0 failed, 0 skipped
- Total: 477 passed, 0 failed, 0 skipped
- Normal build: passed, 0 errors
- Full non-incremental build: 28 warnings, 0 errors (matches the accepted warning baseline; no T08 regression)
- Manual browser verification: pending for T09

T07 verification results (accepted incoming T06 baseline: 477 tests — corrected during T07 from the previously recorded 478, an arithmetic slip; 28 full-rebuild warnings, 0 errors):

- UnitTests: 346 passed, 0 failed, 0 skipped
- IntegrationTests: 92 passed, 0 failed, 0 skipped
- Web.Tests: 39 passed, 0 failed, 0 skipped
- Total: 477 passed, 0 failed, 0 skipped
- Normal build: passed, 0 errors
- Full non-incremental build: 28 warnings, 0 errors (matches the accepted warning baseline; no T07 regression)
- Manual browser verification: pending for T09

T06 verification results (historical record; total below was originally recorded as 478 and corrected to 477 during T07 — arithmetic slip: 346 + 92 + 39 = 477, see T06 Execution Evidence):

- UnitTests: 346 passed, 0 failed, 0 skipped
- IntegrationTests: 92 passed, 0 failed, 0 skipped (87 accepted T05 baseline + 5 legitimate T06 tests)
- Web.Tests: 39 passed, 0 failed, 0 skipped
- Total: 477 passed, 0 failed, 0 skipped
- Normal build: passed, 0 errors
- Full non-incremental build: 28 warnings, 0 errors (matches the accepted warning baseline; no T06 regression)
- Manual browser verification: pending for T09

- UnitTests: 346 passed, 0 failed, 0 skipped (332 accepted T03 baseline + 14 legitimate T04 tests)
- IntegrationTests: 85 passed, 0 failed, 0 skipped
- Web.Tests: 33 passed, 0 failed, 0 skipped
- Total: 464 passed, 0 failed, 0 skipped
- Normal build: passed, 0 errors
- Full non-incremental build: 28 warnings, 0 errors (matches the accepted warning baseline; no T04 regression)
- Manual browser verification: pending for T09

## Release Decision
`Not decided. Sprint 14 is feature-bearing and technically eligible for a semantic release decision after external review. No version has been assigned, no tag has been created, and no release exists; PR title/description, merge, semantic version, tag, and release decisions belong to the external reviewer/user.`

## Final Sprint Status
`Sprint 14 COMPLETE - documentation synchronized and ready for PR/release decision.`

## Final Note
T10 finalized this retrospective after T01-T09 were externally accepted. Documentation synchronization covered README, ROADMAP, PROJECT_STATUS, CHANGELOG, purchasing/authorization/testing documentation, the engineering journal, and this retrospective; the planning report and the external task breakdown remain unchanged (historical planning evidence / external execution-control artifact). No production source, test source, migration, schema, seed, package, or project file was changed during T10. Graphify update was not run (documentation-only task). No Git operations were performed. No version was assigned and no tag or release was created. Sprint 15 has not been started.
