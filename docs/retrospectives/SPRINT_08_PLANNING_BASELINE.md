# InventoryPlatform - Sprint 8 Planning Baseline

**Sprint:** Sprint 8 - Purchasing Enhancements  
**Repository/Branch:** `feature/purchasing_enhancements`  
**Baseline Date:** 2026-08-19  
**Status:** P0 complete; P1 - Multiple Purchase Order Item Management complete and runtime/browser verified

---

## 1. Sprint 7 Closure

Sprint 7 - Additional Reporting & Exports is complete, verified, and documented.

Completed reporting capabilities:

- Inventory Valuation
- Purchase History
- Supplier Purchase Analysis
- Stock Movement
- Low Stock Report
- Inventory Movement Report
- Product Reports
- Excel Export
- PDF Export

Final project-wide verification covered:

- Authentication
- Account Management and 2FA
- Product, Category, Supplier, and Customer management
- Purchase Orders
- Inventory operations
- All seven reporting pages
- Reporting filters, sorting, pagination, navigation, and no-result behavior
- All seven Excel exports
- All seven PDF exports
- Full filtered export behavior
- Inventory Valuation Total Inventory Value
- Empty database behavior
- Explicit query failure and database recovery
- Existing authorization boundaries
- Final `dotnet restore` and `dotnet build`

The final Sprint 7 verification was performed on `feature/additional-reporting`, with a clean working tree after temporary verification configuration was restored.

Sprint 7 also confirmed that Dynamic Capability-Based Authorization was not implemented and remains a future architecture direction.

### Sprint 7 Lessons to Carry Forward

1. Reuse established architecture instead of introducing feature-specific parallel patterns.
2. Keep business behavior in Domain entities and aggregates.
3. Keep Application handlers focused on orchestration.
4. Keep Razor PageModels thin.
5. Keep persistence concerns behind repository/application abstractions.
6. Use dedicated read models for read-oriented reporting.
7. Keep filtering, sorting, and pagination server-side.
8. Validate EF Core queries against actual translation and runtime behavior.
9. Prefer complete vertical slices over isolated technical changes.
10. Perform browser/manual verification before declaring a workflow complete.
11. Keep implementation commits separate from documentation commits.
12. Synchronize documentation only after behavior has been verified.

---

## 2. Architecture and Patterns to Preserve

The validated solution structure is:

```text
InventoryPlatform
|
+-- InventoryPlatform.Web
+-- InventoryPlatform.Application
+-- InventoryPlatform.Domain
+-- InventoryPlatform.Infrastructure
+-- InventoryPlatform.Shared
```

The established workflow-oriented Purchasing architecture is:

```text
Razor Page
     |
     v
Application Handler
     |
     v
PurchaseOrder Aggregate
     |
     v
Repository / Unit of Work
     |
     v
Entity Framework Core
     |
     v
SQL Server
```

The aggregate remains responsible for business rules and state transitions.

The established Purchasing workflow is:

```text
Draft
  |
  v
Submitted
  |
  v
Approved
  |
  v
Receiving
  |
  v
Completed
```

The Purchasing vertical slice already supports:

- Purchase Order creation
- Purchase Order retrieval
- Purchase Order listing
- Submission
- Approval
- Partial receiving
- Final receiving
- Completed state
- Supplier selection
- Product selection
- Ordered quantity display
- Received quantity display
- Remaining quantity display
- Purchase Order total calculation
- Client-side validation
- Domain validation
- Success feedback
- Query-failure feedback

Shared infrastructure to preserve includes:

- `PagedRequest`
- `PagedQuery`
- `PagedResult<T>`
- Shared filtering infrastructure
- Shared sorting infrastructure
- `Result`
- `Result<T>`
- Dependency injection conventions
- Repository pattern
- Unit of Work
- Feature-first organization
- Vertical Slice Architecture
- Rich Domain Model
- Thin PageModels
- Thin Application handlers
- Request/Response/Handler patterns

No structural architectural redesign is planned for Purchasing Enhancements unless actual source inspection proves that an existing boundary cannot support the required behavior.

---

## 3. Sprint 8 Objective

Sprint 8 begins with **Purchasing Enhancements** as its exclusive implementation focus.

The objective is to evolve the existing Purchasing vertical slice from its current core workflow into a more complete operational Purchasing capability while preserving the validated architecture.

The sprint must:

- Extend existing Purchasing behavior rather than replace it.
- Reuse the existing Purchase Order aggregate and workflow where appropriate.
- Improve Purchase Order item management.
- Add operational Purchase Order discovery capabilities.
- Integrate receiving with inventory where required by the verified business rules.
- Preserve domain validation and transactional integrity.
- Maintain consistent Razor Pages behavior.
- Preserve existing authorization until the dedicated authorization phase.

---

## 4. Purchasing Business and Portfolio Value

Purchasing is already the platform's first workflow-driven business module. Enhancing it increases the practical value of the platform by connecting:

```text
Supplier
   |
   v
Purchase Order
   |
   v
Purchase Order Items
   |
   v
Approval
   |
   v
Receiving
   |
   v
Inventory
```

The enhancements are intended to make Purchasing operationally useful beyond the current core demonstration workflow.

Portfolio value includes demonstrating:

- Rich domain modeling
- Aggregate-based workflow management
- Multi-item transactional behavior
- Search/filter/sort/pagination patterns
- Inventory integration
- Transactional consistency
- End-to-end business workflow validation
- Reuse of shared infrastructure
- Controlled incremental architecture evolution

---

## 5. Locked Sprint Priority Order

The Sprint 8 priority order is locked as:

1. Purchasing Enhancements
2. Dynamic Capability-Based Authorization
3. Sales Module
4. Audit / Activity Logging
5. Bulk Import / Export
6. Barcode / QR

Only priority 1 is active during the Purchasing Enhancements work.

Dynamic Capability-Based Authorization must not be implemented as part of Purchasing Enhancements.

---

## 6. Repository and Branch Strategy

Sprint 8 Purchasing Enhancements uses:

```text
feature/purchasing_enhancements
```

The Sprint 7 branch:

```text
feature/additional-reporting
```

remains the historical Sprint 7 baseline.

Sprint 7 implementation history must not be modified as part of Sprint 8.

`main` must not be used as the active development branch.

The first Sprint 8 implementation work must begin only after the actual repository and branch state have been inspected and confirmed.

Repository creation, if required by the Sprint 8 environment, is part of Sprint 8 initial setup and must preserve the documented architecture and history baseline.

---

## 7. Purchasing Scope

The current source/documentation establishes the following Purchasing enhancement areas:

### In Scope

- Multiple Purchase Order Item Management
- Purchase Order Search
- Purchase Order Filtering
- Purchase Order Sorting
- Purchase Order Pagination
- Inventory Integration During Receiving
- Additional Purchasing User Experience Improvements

These areas extend the already implemented Purchase Order lifecycle.

### Explicitly Preserved Existing Behavior

- Draft -> Submitted -> Approved -> Receiving -> Completed workflow
- Purchase Order aggregate ownership of business state
- Domain validation as the authoritative business-rule boundary
- Existing repository and Unit of Work patterns
- Existing Razor Pages architecture
- Existing Identity authorization model until the authorization phase

---

## 8. Explicit Non-Scope

The following are outside Purchasing Enhancements:

- Dynamic Capability-Based Authorization implementation
- Sales Module
- Audit / Activity Logging
- Bulk Import / Export as a platform-wide feature
- Barcode / QR implementation
- REST API
- Blazor
- Mobile application
- Inventory forecasting
- Unrelated reporting changes
- Architectural redesign without evidence requiring it
- Refactoring solely for stylistic preference
- Unplanned schema changes without an approved business requirement
- Changes to Sprint 7 reporting behavior unless required by a verified regression

Dynamic Capability-Based Authorization remains a separate future implementation phase.

---

## 9. Task Sequence P0-P7

The task labels below are established by this planning baseline because no prior Sprint 8 P0-P7 task definition was found in the available project documentation.

### P0 - Actual Purchasing Source / Documentation Baseline

Inspect and document the actual repository state before implementation.

Required:

- Confirm repository
- Confirm branch
- Confirm working-tree state
- Review Purchasing Domain entities
- Review Purchase Order aggregate behavior
- Review Purchase Order item behavior
- Review Application handlers
- Review repository interfaces and implementations
- Review EF Core configurations
- Review migrations
- Review Purchasing Razor Pages
- Review shared paging/filtering/sorting infrastructure
- Review current authorization boundaries
- Review relevant documentation
- Identify exact files affected by each planned enhancement

No code changes are permitted during P0 unless required to establish the baseline and explicitly approved.

### P1 - Multiple Purchase Order Item Management

Improve Purchase Order item management using the actual existing domain and presentation patterns.

**Current implementation status:** Source implementation completed for multi-item Purchase Order creation in the Create UI. Runtime/browser verification completed successfully.

Primary concern:

- Support multi-item Purchase Order creation without bypassing the PurchaseOrder aggregate.

### P2 - Purchase Order Search

Add server-side Purchase Order search based on verified source fields and business requirements.

Search fields must be identified from actual source inspection.

No field names or search semantics may be guessed.

### P3 - Purchase Order Filtering, Sorting, and Pagination

Extend Purchase Order listing with reusable server-side:

- Filtering
- Sorting
- Pagination

Existing shared infrastructure must be reused where applicable.

### P4 - Inventory Integration During Receiving

Connect verified Purchase Order receiving behavior to inventory updates.

Required behavior must be derived from the actual Domain model, Inventory Transaction model, persistence implementation, and existing receiving workflow.

Inventory updates must preserve:

- Domain invariants
- Transactional consistency
- Existing inventory quantity rules
- Inventory transaction history
- Purchase Order receiving state transitions

### P5 - Purchasing User Experience Improvements

Address verified usability gaps discovered during P1-P4.

Examples may include:

- Clearer item-entry behavior
- Better validation feedback
- Better workflow feedback
- Consistent pagination/filter state
- Better receiving presentation

Only issues demonstrated by actual source or browser verification are in scope.

### P6 - Purchasing Regression and Integration Verification

Perform comprehensive Purchasing verification after implementation.

Verification must cover:

- Existing Purchase Order creation
- Multi-item behavior
- Search
- Filtering
- Sorting
- Pagination
- Submission
- Approval
- Partial receiving
- Final receiving
- Inventory synchronization
- Domain validation
- Failure behavior
- Existing authorization boundaries
- Regression of unrelated core modules

### P7 - Final Purchasing Completion Verification

Perform final project-level verification for the Purchasing Enhancements scope.

Required:

- Build verification
- Runtime verification
- Browser/manual verification
- Database behavior verification
- Review of generated database changes, if any
- Git diff inspection
- Confirmation that no unrelated files changed
- Confirmation that Dynamic Capability-Based Authorization was not introduced
- Documentation synchronization readiness

P7 is the completion gate for the Purchasing Enhancements implementation scope.

---

## 10. Documentation Task Sequence D1-D4

### D1 - Planning and Scope Documentation

Maintain the Sprint 8 Planning Baseline and ensure that the documented task sequence matches the approved scope.

### D2 - Feature Documentation Synchronization

After verified implementation, synchronize applicable:

- `PROJECT_STATUS.md`
- `FEATURES.md`
- `ROADMAP.md`

Documentation must describe verified behavior rather than planned behavior.

### D3 - Engineering and Design Documentation

Synchronize applicable:

- `docs/ENGINEERING_JOURNAL.md`
- `docs/DESIGN_DECISIONS.md`
- Purchasing-specific retrospective or technical documentation

Only decisions and behavior supported by actual implementation should be recorded as completed.

### D4 - Final Documentation Validation

Before Sprint 8 handoff:

- Compare documentation against source
- Compare documentation against verified runtime behavior
- Remove stale Sprint 7 statements where they conflict with Sprint 8 state
- Confirm task completion status
- Confirm no future feature is incorrectly marked complete
- Confirm branch/repository information is accurate
- Confirm verification statements are evidence-based

Documentation commits remain separate from implementation commits.

---

## 11. Dependencies

Primary dependencies are:

```text
P0
 |
 +--> P1
 |
 +--> P2
 |
 +--> P3
 |
 +--> P4
 |
 +--> P5
 |
 +--> P6
 |
 +--> P7
```

More specifically:

- P1 depends on actual PurchaseOrder and PurchaseOrderItem domain behavior.
- P2 depends on the actual Purchase Order persistence/read model.
- P3 depends on shared paging/filtering/sorting infrastructure and the Purchase Order query shape.
- P4 depends on the existing receiving workflow and inventory transaction rules.
- P5 depends on issues discovered during verified implementation.
- P6 depends on P1-P5 completion.
- P7 depends on P6 completion and documentation readiness.
- D2-D4 depend on verified implementation behavior.

Dynamic Capability-Based Authorization is not a dependency for Purchasing Enhancements.

---

## 12. Risks

### Domain Workflow Risk

Changes to Purchase Order item or receiving behavior may affect existing aggregate invariants.

**Control:** inspect and reuse existing domain methods before modifying behavior.

### Inventory Consistency Risk

Receiving changes can affect both Purchase Order state and inventory state.

**Control:** verify transactional behavior and existing Unit of Work/persistence boundaries.

### Query Translation Risk

Search, filtering, sorting, and pagination may produce EF Core translation issues.

**Control:** validate queries through actual database execution.

### Regression Risk

Purchasing enhancements may affect existing Purchase Order workflows.

**Control:** repeat the complete existing workflow during browser/manual verification.

### Scope Creep Risk

Purchasing work can naturally expand into authorization, sales, audit, or broader inventory features.

**Control:** enforce the locked Sprint 8 priority and explicit non-scope.

### Documentation Drift Risk

Planning documents may become stale after implementation.

**Control:** documentation is synchronized only after behavior is verified.

### Architectural Duplication Risk

New feature-specific abstractions may duplicate existing infrastructure.

**Control:** inspect existing shared patterns first and apply the Rule of Three.

---

## 13. Acceptance Criteria

Sprint 8 Purchasing Enhancements are accepted only when:

- The implementation is on `feature/purchasing_enhancements`.
- Sprint 7 historical work remains unchanged.
- Existing Purchasing workflow behavior remains valid.
- Multiple Purchase Order item management works according to the verified requirements.
- Purchase Order search works according to verified source fields.
- Purchase Order filtering works according to verified requirements.
- Purchase Order sorting works according to verified requirements.
- Purchase Order pagination works using the established paging infrastructure where applicable.
- Receiving correctly updates inventory according to the verified domain rules.
- Inventory transaction history remains correct.
- Domain validation remains authoritative.
- Existing authorization behavior remains intact.
- No Dynamic Capability-Based Authorization implementation is introduced.
- No unrelated features are implemented.
- `dotnet restore` succeeds.
- `dotnet build` succeeds.
- Applicable runtime/database verification succeeds.
- Browser/manual verification succeeds for the implemented workflow.
- Git diff contains only intended changes.
- Implementation and documentation commits are separate.
- Documentation matches verified source and behavior.
- Final Git state is clean or contains only explicitly documented intentional changes.

---

## 14. Verification Strategy

Verification follows the established project workflow.

### Source Verification

- Inspect exact source paths before coding.
- Inspect related Domain, Application, Infrastructure, Web, and Shared components.
- Inspect existing tests and verification mechanisms if present.

### Build Verification

Run as applicable:

```text
dotnet restore
dotnet build
```

Tests must also be run when the actual repository contains applicable automated tests.

### Browser Verification

Verify actual application behavior using persisted database data.

Purchasing verification should include:

- Purchase Order listing
- Purchase Order creation
- Multiple items
- Search
- Filtering
- Sorting
- Pagination
- Details
- Submission
- Approval
- Partial receiving
- Final receiving
- Inventory update
- Success feedback
- Validation failures
- No-result behavior
- Existing authorization boundaries

### Database Verification

Where inventory integration is implemented:

- Confirm Purchase Order receiving state.
- Confirm Product quantity.
- Confirm Inventory Transaction creation.
- Confirm no duplicate inventory update.
- Confirm partial receiving behavior.
- Confirm final receiving behavior.

### Regression Verification

Existing core modules and Sprint 7 reporting must remain operational.

No verification claim may be made unless the verification was actually performed.

---

## 15. Commit Strategy

Implementation and documentation commits remain separate.

### Planning Baseline

```text
docs: establish sprint 8 planning baseline
```

This is the first Sprint 8 documentation commit.

### Implementation

Each implementation task should use a focused feature commit, for example:

```text
feat(purchasing): ...
```

The exact message must reflect the actual completed implementation.

### Documentation

Documentation must use a separate commit, for example:

```text
docs: update purchasing enhancements status
```

The exact message must reflect the actual documentation changes.

Do not combine implementation and documentation changes in one commit.

---

## 16. ZIP and Manual-Patch Packaging Rules

When source files are handed off for review:

- Use ZIP for multiple files.
- Preserve the repository-relative directory structure.
- Do not include unnecessary build artifacts such as `bin/` or `obj/`.
- Do not include unrelated generated files.
- Keep implementation handoff ZIPs separate from documentation handoff ZIPs when both are provided.

Manual-review patches must be packaged separately as a patch ZIP.

A manual patch ZIP must:

- Contain only the intended patch/diff material.
- Clearly identify the source and target paths.
- Preserve enough context for manual review.
- Not be mixed with unrelated documentation or generated artifacts.

---

## 17. Documentation Synchronization Rules

Documentation is authoritative only after it has been validated against actual source and behavior.

Rules:

1. Do not mark planned work as completed before verification.
2. Do not claim browser verification without performing it.
3. Do not claim build success without running the build.
4. Do not claim database behavior without executing the relevant workflow.
5. Keep Sprint 7 historical documentation intact except for necessary cross-sprint status references.
6. Update Sprint 8 documentation after verified behavior, not before.
7. Keep implementation and documentation commits separate.
8. Remove stale status statements when they conflict with verified current state.
9. Preserve architectural decision history.
10. Record important deviations from established patterns and their justification.

---

## 18. Sprint 8 Completion Criteria

Sprint 8 Purchasing Enhancements is complete only when all of the following are true:

- P0-P7 are complete.
- D1-D4 are complete.
- Purchasing Enhancements are implemented and verified.
- Existing Purchasing workflows remain operational.
- Inventory integration is verified where applicable.
- Regression verification passes.
- Build verification passes.
- Browser/manual verification passes.
- Documentation is synchronized and validated.
- Implementation and documentation commits are separate.
- Git working tree is clean or the remaining state is explicitly documented.
- A Sprint 8 save point is created.
- A final handoff identifies the completed scope, verification evidence, commits, documentation state, and next Sprint 8 priority.

The next priority after Purchasing Enhancements is Dynamic Capability-Based Authorization, but it must not begin automatically from this planning baseline.

---

## 19. P0/P1 Handoff State

P0 - Actual Purchasing Source/Documentation Baseline is complete. The actual source baseline was inspected and the documented Purchasing core workflow was reconciled against the implementation.

P1 - Multiple Purchase Order Item Management is also complete and runtime/browser verified. The Create Purchase Order workflow now supports multiple item rows while preserving the existing Application, Domain, Infrastructure, and persistence boundaries.

No P2-P6 implementation has been started. Dynamic Capability-Based Authorization remains outside the current Purchasing Enhancements scope.

The next task is:

**P2 - Purchase Order Search**

P2 must begin by reusing the actual Purchase Order listing/query architecture established by the existing source.


## 20. P0 - Actual Purchasing Source Baseline

P0 inspected the actual Sprint 8 source and documentation baseline. The existing Purchasing vertical slice was confirmed as implemented across Domain, Application, Infrastructure, and Web.

Confirmed current Purchasing behavior:

- Purchase Order creation
- Purchase Order listing
- Purchase Order details
- Submit
- Approve
- Partial receiving
- Final receiving
- Completed state
- Supplier and product validation
- Purchase Order item domain validation
- Existing repository and Unit of Work persistence
- Existing EF Core Purchase Order and Purchase Order Item mappings

Current Purchasing limitations confirmed by source:

- Purchase Order Create previously rendered only one item row.
- Purchase Order search is not implemented.
- Purchase Order filtering is not implemented.
- Purchase Order sorting is not implemented.
- Purchase Order pagination is not implemented.
- Purchase Order receiving does not currently synchronize inventory.
- Dynamic Capability-Based Authorization is not part of Purchasing Enhancements.

The supplied source snapshot did not contain `.git` metadata, so branch status, working-tree status, and commit history were not asserted from the ZIP.

The source snapshot also could not be built in the inspection environment because the .NET CLI was unavailable. Runtime verification was subsequently completed against the updated application by the project owner.

## 21. P1 - Multiple Purchase Order Item Management

P1 is complete. The Purchase Order Create page now supports multiple item rows while preserving the existing Purchasing architecture and Application/Domain contracts.

Implemented behavior:

- Add multiple Purchase Order item rows.
- Remove individual item rows.
- Preserve indexed Product, Quantity, and Unit Cost bindings.
- Preserve existing validation behavior.
- Preserve the existing Create Purchase Order handler.
- Preserve the existing `PurchaseOrder.AddItem()` domain operation.
- Preserve the existing Purchase Order persistence model.
- Preserve the existing Details page, Submit, Approve, and Receiving workflow.

Runtime/browser verification was completed successfully. Multi-item Purchase Order creation and the existing downstream Purchasing workflow were confirmed working.

No database migration was required for P1.
No Dynamic Capability-Based Authorization changes were introduced.
No P2-P6 implementation was started.

### P1 Acceptance Result

**Status: COMPLETE**

P1 acceptance criteria were satisfied through source inspection and successful runtime/browser verification.

The next task is **P2 - Purchase Order Search**.

## 22. P1 Documentation Audit

P1 documentation synchronization was reviewed against the actual source and verified runtime behavior.

### Updated

- `PROJECT_STATUS.md`
- `ROADMAP.md`
- `README.md`
- `CHANGELOG.md`
- `docs/FEATURES.md`
- `docs/ENGINEERING_JOURNAL.md`
- `docs/retrospectives/SPRINT_08_PLANNING_BASELINE.md`
- `docs/retrospectives/SPRINT_04_APPLICATION.md` - historical cross-reference only

### Reviewed and Preserved

- `ARCHITECTURE.md` - no architectural change introduced by P1
- `docs/ARCHITECTURE_REVIEW.md` - no new architecture boundary required
- `docs/DESIGN_DECISIONS.md` - no new design decision required
- `CODE_STYLE.md` - no project-wide style rule changed
- `CONTRIBUTING.md` - no contribution workflow changed

### Historical Sprint Documentation Preserved

Sprint 1, Sprint 2, Sprint 3, Sprint 5, Sprint 6, and Sprint 7 retrospective documents were reviewed for P1 impact and left unchanged where their statements remain historically accurate. Sprint 7 reporting documentation remains the historical baseline and was not modified.

Documentation status is therefore synchronized for P1 without rewriting historical records or introducing unsupported assumptions.

