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
- T05 - NOT STARTED
- T06 - NOT STARTED
- T07 - NOT STARTED
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
T04 verification results (accepted incoming T03 baseline: 450 tests, 28 full-rebuild warnings, 0 errors):

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
