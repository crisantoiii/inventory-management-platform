# Sprint 14 - Purchase Order Cancellation and Draft Item Editing Retrospective

## Sprint Status
`PLANNED - IMPLEMENTATION NOT STARTED`

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
- T08 - NOT STARTED
- T09 - NOT STARTED
- T10 - NOT STARTED

## Verification Ledger
Placeholders only. No actual verification values are recorded yet.

- Build results: pending
- UnitTests: pending
- IntegrationTests: pending
- Web.Tests: pending
- Warnings: pending
- Errors: pending
- Manual browser verification: pending
- Provider verification: pending

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
Placeholder. This section will be completed after verification and implementation work.

## What Could Be Improved
Placeholder. This section will be completed after verification and implementation work.

## Lessons Learned
Placeholder. This section will be completed after verification and implementation work.

## Final Test Baseline
T07 verification results (accepted incoming T06 baseline: 478 tests, 28 full-rebuild warnings, 0 errors):

- UnitTests: 346 passed, 0 failed, 0 skipped
- IntegrationTests: 92 passed, 0 failed, 0 skipped
- Web.Tests: 39 passed, 0 failed, 0 skipped
- Total: 477 passed, 0 failed, 0 skipped
- Normal build: passed, 0 errors
- Full non-incremental build: 28 warnings, 0 errors (matches the accepted warning baseline; no T07 regression)
- Manual browser verification: pending for T09

- UnitTests: 346 passed, 0 failed, 0 skipped
- IntegrationTests: 92 passed, 0 failed, 0 skipped (87 accepted T05 baseline + 5 legitimate T06 tests)
- Web.Tests: 39 passed, 0 failed, 0 skipped
- Total: 478 passed, 0 failed, 0 skipped
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
`Not decided. Sprint 14 is likely feature-release-worthy, but semantic version/tag/release decisions occur separately after successful sprint closure.`

## Final Sprint Status
`NOT COMPLETE`

## Final Note
This retrospective baseline is intentionally created as a planning artifact only. It does not claim any implementation has occurred and must remain in a planned state until T01-T10 complete successfully.
