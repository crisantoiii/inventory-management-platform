# Roadmap

## Overview

This roadmap outlines the planned evolution of the Inventory Management Platform.

The project is developed incrementally, with each completed module validating the architecture before expanding into additional business domains.

v0.1  Products                 ✅
v0.2  Categories               ✅
v0.3  Suppliers                ✅
v0.4  Customers / Units        ✅
v0.5  Shared Infrastructure    ✅
v0.6  Inventory Transactions   ✅
v0.7  Dashboard                ✅
v0.8  Identity & Users         ✅
v0.9  Architecture Sprint      ✅
v1.0  Purchasing Application   ✅
v1.1  Purchasing Presentation  ✅
v1.2  Reporting                ✅
v1.3  Account Management       ✅
v1.4  Additional Reporting     ✅
v1.5  Purchasing Enhancements  ✅
v1.6  Dynamic Capability Auth  ✅
(Sprint 11 Automated Testing ✅ — non-release; Sprint 12 Authorization Refinement ✅ — non-release; Sprint 13 Purchasing Workflow Test Automation ✅ — non-release; Sprint 14 Purchase Order Cancellation and Draft Item Editing ✅ — non-release; Sprint 15 Purchase Order Workflow Error Handling and UX Hardening ✅ — non-release; Sprint 16 Purchase Order POST Round-Trip State and Create Failure Presentation Corrections ✅ — non-release; Sprint 17 HTTP/Razor Integration-Test Foundation ✅ — non-release; Sprint 18 SQL Server Relational Verification ✅ — non-release; Sprint 19 Purchase Order Create HTTP/Razor Integration Coverage ✅ — non-release; Sprint 20 Provider-Neutral Continuous Verification ✅ — non-release)

---

# Current Development Strategy

The project has completed its architectural foundation and the first end-to-end Purchasing vertical slice.

Future development will prioritize expanding business capabilities while preserving the validated architecture.

Focus Areas:

- Business workflows
- Domain modeling
- Enterprise features
- Reporting
- APIs
- Incremental vertical slices

Each major business capability should be implemented from Domain and Application logic through a usable Presentation workflow before being considered complete.

### Sprint 7 Status

Sprint 7 Additional Reporting is complete, verified, and documented.

Completed:

- Inventory Valuation
- Purchase History
- Supplier Purchase Analysis
- Stock Movement
- Low Stock Report
- Inventory Movement Report
- Product Reports
- Excel Export
- PDF Export

Sprint 8 Purchasing Enhancements is complete and closed. P0, P1, P2, P3, P4, P5, P6, and P7 are complete and runtime/browser verified. P7 completed the integrated Purchasing regression pass and corrected one in-scope pagination state-preservation defect.

---

# Sprint 8 - Purchasing Enhancements

## Completed

- P0 - Actual Purchasing Source/Documentation Baseline
- P1 - Multiple Purchase Order Item Management
- P2 - Purchase Order Search
- P3 - Purchase Order Filtering
- P4 - Purchase Order Sorting
- P5 - Purchase Order Pagination
- P6 - Inventory Synchronization During Receiving - Complete and verified
- P7 - Integrated Purchasing Verification - Complete and verified

### P3 - Purchase Order Filtering

Verified filters:
- From Date
- To Date
- Purchase Order Status

Verified:
- Individual filters
- Combined filters
- Search + filter interaction
- Empty-result behavior
- Applicable filter-state preservation
- Existing authorization behavior
- No unrelated Purchasing behavior changed

Runtime/browser verification was completed successfully by the project owner.

## Final Sprint 8 State

- D1 - Documentation Synchronization - Complete
- D2 - Design Decision Synchronization - Complete
- D3 - Final Sprint 8 Retrospective - Complete
- D4 - Final Documentation Validation - Complete

The Sprint 8 final save point has been established. No new feature work begins from this roadmap state; the next activity is Next Sprint Planning.

## Sprint 9 - ASP.NET Core Code Quality & Consistency

**Status:** T03-T13 complete; final documentation and architecture validation

Sprint 9 is a bounded consistency workstream rather than a feature-module release.

Completed implementation scope:

- Razor `asp-for` normalization for Purchase History and Supplier Purchase Analysis.
- Purchase Order sorting/pagination navigation normalization to `asp-route-*`.
- Request-binding consolidation for seven core list PageModels.
- Removal of redundant inherited repository interface declarations.
- Repository query-signature validation without collapsing meaningful feature-specific filters.
- Seven `PageNum` pagination-link corrections.
- Purchase Order Details pagination context preservation.
- Purchase Order Status option de-duplication and filter label/control association corrections.

Verified conventions:

- `PageNum` is the canonical Razor/UI paging property and query parameter.
- `asp-for` is preferred where appropriate for Razor form binding and labels.
- `asp-route-*` is preferred for direct Razor navigation and query state.
- Application Request -> `PagedQuery` / repository boundaries remain when they have distinct responsibilities.
- Rule-of-Three is applied before introducing reusable helpers or abstractions.

Verification limitation:

- Source-level verification is complete through T13.
- The supplied environment has no `dotnet` CLI, so no successful Sprint 9 build or runtime/browser verification is claimed.
- No automated test project/source is present in the repository.

Sprint 9 did not introduce unrelated business capabilities or structural architectural redesign.

## Sprint 11 - Automated Testing & Test Automation

**Status:** Complete

Sprint 11 established the project's first automated testing foundation and implemented risk-based automated coverage for the highest-value Domain, Application authorization, authorization seeding, and authorization repository behaviors.

### Completed

- T01: Test Infrastructure Foundation (xUnit, EF Core InMemory)
- T02: PurchaseOrder Domain Tests (101 tests)
- T03: Product Domain Tests (56 tests)
- T04: Authorization Domain Tests (46 tests)
- T05: CapabilityAuthorizationService Tests (15 tests)
- T07: AuthorizationSeeder Integration Tests (24 tests)
- T08: Authorization Repository Integration Tests (36 tests)
- T09: CI / Automated Test Execution (provider-neutral baseline)
- T10: Test Conventions and Sprint Documentation
- T11: Documentation Synchronization and Sprint Closure

### Deferred

- T06: Authorization Handler Tests → Sprint 12

### Final Test Baseline (Sprint 11 historical baseline; superseded by the Sprint 12 total below)

```text
UnitTests:        219 passed
IntegrationTests:  61 passed
Total:            280 passed, 0 failed
Build:            0 errors, 0 warnings
```

### Sprint 12 Direction

Sprint 12 should focus on:
- Web Authorization Handler Tests (CapabilityAuthorizationHandler, MultiCapabilityAuthorizationHandler)
- WebApplicationFactory integration tests
- Razor Page authorization integration tests
- CI provider establishment (if repository hosting is confirmed)

## Sprint 12 - Authorization Refinement

**Status:** Complete

Sprint 12 hardened the capability-based authorization model through automated Web authorization-handler testing and remediation of two confirmed authorization-boundary defects, without changing the authorization architecture.

### Completed

- T01: `InventoryPlatform.Web.Tests` project foundation (references Web only)
- T02: `FakeCapabilityAuthorizationService` hand-written test double (no mocking framework)
- T03: `CapabilityAuthorizationHandlerTests` (8 tests)
- T04: `MultiCapabilityAuthorizationHandlerTests` (12 tests, OR semantics with short-circuit)
- T05: Categories/Edit remediation — `[Authorize(Policy = AuthorizationPolicies.InventoryManagement)]`
- T06: Suppliers/Create remediation — `ViewInventory` replaced with `InventoryManagement`
- T08: Integrated verification — passed (no authorization regression)
- T09: Documentation synchronization and Sprint 12 closure

### Blocked/Deferred

- T07: EditStatus `User.IsInRole(InventoryManager)` cleanup — the remaining occurrence (line 61) is a reachable, behavior-affecting self-deactivation guard for supported multi-role users, NOT dead code. Removal would change observable behavior and requires an explicit behavioral decision.

### Final Test Baseline

```text
UnitTests:        219 passed
IntegrationTests:  61 passed
Web.Tests:         33 passed
Total:            313 passed, 0 failed, 0 skipped
Build:             0 errors (full rebuild: 28 pre-existing warnings)
```

## Sprint 13 - Purchasing Workflow Test Automation

**Status:** Complete

Sprint 13 extended the established risk-based automated testing program to the Purchasing workflow with layered automated coverage: the Purchasing Application layer (six handlers, two Create validators, the `PurchaseOrderErrors` contract) as primary scope, plus `PurchaseOrderRepository` integration verification as supporting scope. No production code was changed by the sprint.

### Completed

- T01: Purchasing Test Support Foundation (`FakePurchaseOrderRepository`, `FakeUnitOfWork`, `PurchasingTestData`, `EntityIdHelper`)
- T02: CreatePurchaseOrderHandler + Validator Tests (52 discovered cases)
- T03: Workflow Transition Handler Tests — Submit/Approve (13 discovered cases)
- T04: ReceivePurchaseOrderHandler Tests (14 tests)
- T05: Purchase Order Query Handler Tests (16 tests)
- T06: PurchaseOrderRepository Integration Tests (24 tests, EF Core InMemory with fresh-context isolation)
- T07: Integrated Verification — 432 passed / 0 failed / 0 skipped (314 UnitTests, 85 IntegrationTests, 33 Web.Tests); full rebuild 28 warnings / 0 errors, baseline preserved
- T08: Documentation Synchronization & Sprint 13 Closure

### Final Test Baseline

```text
UnitTests:        314 passed
IntegrationTests:  85 passed
Web.Tests:         33 passed
Total:            432 passed, 0 failed, 0 skipped
Build:             0 errors (full rebuild: 28 pre-existing warnings)
```

### Boundaries Held

- No production changes, no new packages, no database/migration/seed changes, no CI changes, no WebApplicationFactory (Sprint 13 historical boundary — superseded by Sprint 17, which introduced the database-safe WebApplicationFactory foundation)
- Repository integration tests use EF Core InMemory: repository wiring/query-shape regression coverage only — not SQL Server integration testing
- Sprint 12 EditStatus item and the T05 `PagedRequest.Status` pass-through finding remain deferred, untouched

## Sprint 15 - Purchase Order Workflow Error Handling and UX Hardening

**Status:** Complete/Closed

Sprint 15 hardened the Purchase Order Details presentation boundary: all four POST workflows (Submit, Approve, Receive, Cancel) now catch expected `DomainException` failures and render the canonical Domain message as inline validation feedback (ModelState + Purchase Order reload via `GetPurchaseOrderHandler` + `Page()`, through one private local helper; helper reload failure returns `NotFound()`), instead of propagating to the Development exception page. Authorization remains before try/catch and unchanged; existing Application Result failures keep their ModelState + reload + Page() semantics; unexpected exceptions propagate. Rejected operations persist no state changes (SQL before/after and restart evidence in T04); successful paths are unchanged.

### Completed

- T01: Contract Verification and Design Lock
- T02: Purchase Order Details Workflow Error Handling (`Details.cshtml.cs` only)
- T03: Regression and Coverage Verification (all suites green; Result/NotFound semantics documented correctly)
- T04: Integrated and Manual Verification (real-browser failure/success/authorization/NotFound scenarios with SQL before/after, restart persistence check, migration-history check)
- T05: Documentation Synchronization and Sprint Closure

### Final Test Baseline

```text
UnitTests:        346 passed
IntegrationTests:  92 passed
Web.Tests:         39 passed
Total:            477 passed, 0 failed, 0 skipped
Build:             0 errors (full rebuild: 28 pre-existing warnings)
```

### Boundaries Held

- No production source changed outside `Details.cshtml.cs` (T02); no test, migration/schema, authorization/seed, package, or project changes
- No artificial PageModel test seam introduced
- `Descending=True` hidden-field POST round-trip loss (Details and Edit pages) is a pre-existing markup issue, deferred to a future approved task — representative navigation/query state was preserved except for this issue
- No version assigned, no tag created, no release published (non-release technical-hardening sprint)

## Sprint 14 - Purchase Order Cancellation and Draft Item Editing

**Status:** Complete

Sprint 14 completed the Purchase Order lifecycle: authorized cancellation is now available from Draft and Submitted states through the full stack (Domain aggregate `Cancel()`, Application cancellation workflow, `PurchaseOrder.Cancel` capability, Razor Pages cancellation workflow on Details), and Draft Purchase Orders support item editing (Quantity/UnitCost) and item removal through the dedicated `Pages/Purchasing/PurchaseOrders/Edit.cshtml` surface backed by `PurchaseOrder.Edit`. `Cancelled` is terminal. Cancellation is forbidden from Approved, Receiving, Completed, and Cancelled states; item mutation is keyed by `ProductId` and only Draft orders can be edited; removing the final item is allowed while empty Purchase Orders still cannot be submitted.

### Completed

- T01: Sprint 14 Contract Verification and Implementation Readiness
- T02: Domain Cancellation Transition
- T03: Application Cancellation Workflow
- T04: Application Draft Item Editing Workflow
- T05: Purchase Order Edit/Cancel Authorization (`PurchaseOrder.Edit`, `PurchaseOrder.Cancel` — catalog 39 → 41)
- T06: Purchase Order Persistence Integration Coverage (EF Core InMemory, fresh-context isolation)
- T07: Web Cancellation Workflow
- T08: Web Draft Item Edit Workflow (dedicated Edit page)
- T09: Integrated and Manual Verification — automated suites re-executed plus mandatory manual browser verification against SQL Server
- T10: Documentation Synchronization and Sprint 14 Closure

### Final Test Baseline

```text
UnitTests:        346 passed
IntegrationTests:  92 passed
Web.Tests:         39 passed
Total:            477 passed, 0 failed, 0 skipped
Build:             0 errors (full rebuild: 28 pre-existing warnings; no Sprint 14 warning regression)
```

### Boundaries Held

- No schema migration, no EF mapping change, no data backfill — the existing `Cancelled = 6` status is used
- Capability authorization remains additive within the existing dynamic capability model (not role-only authorization)
- Manual browser/provider verification against SQL Server remains manual — it is not automated end-to-end or SQL Server integration testing
- No version assigned, no tag created, no release published

## Sprint 16 - Purchase Order POST Round-Trip State and Create Failure Presentation Corrections

**Status:** Complete/Closed — non-release sprint; v1.6.0 remains the release baseline.

Sprint 16 corrected the six Purchase Order Details/Edit hidden `Descending` values and added narrow inline handling for expected Create `DomainException` failures. A 12-case true/false runtime matrix passed for Submit, Approve, Cancel, Receive, UpdateItem, and RemoveItem. Duplicate-product, zero-quantity, and negative-cost Create failures rendered inline without persistence; the successful workflow and authorization checks also passed. The final automated baseline is 477/0/0 (346 UnitTests, 92 IntegrationTests, 39 Web.Tests), with 0/0 normal-build diagnostics and 28 pre-existing warnings/0 errors non-incrementally. Candidates C, D1, D4, and E remain deferred.

## Sprint 17 - HTTP/Razor Integration-Test Foundation

**Status:** Complete/Closed — non-release sprint; v1.6.0 remains the release baseline. No version tag was created.

Sprint 17 established a real HTTP/Razor integration-test foundation in `InventoryPlatform.Web.Tests`, closing the gap where Web.Tests could not exercise the actual ASP.NET Core pipeline. The originally planned `IStartupFilter` pre-seeding database guard was discovered invalid during T01 (startup seeding runs synchronously inside `app.UseWeb()` before any `IStartupFilter` middleware executes) — a planning/lifecycle correction, not a product defect. Revision 4 accepted the corrected safety architecture: an early deliberately non-production sentinel `DefaultConnection` injected through host configuration before application registration reads it, normal production registrations, `ApplicationDbContext` replacement in test service customization before final root-provider construction, structural fail-closed validation before the root provider builds, unchanged `UseWeb()` startup seeding against EF Core InMemory, and positive provider/seed verification.

### Completed

- T01: Discovery/blocker — `IStartupFilter` pre-seeding guard lifecycle correction
- T02: Public partial `Program` test-host seam + approved Web.Tests packages (`Microsoft.AspNetCore.Mvc.Testing`, `Microsoft.EntityFrameworkCore.InMemory`)
- T03: Database-safe `InventoryPlatformWebApplicationFactory` — early sentinel, context capture/replacement, structural fail-closed validation (with negative unsafe-registration proof), real startup seeding, cross-factory isolation (5 tests)
- T04: Test-only seeded-user authentication — `X-Test-User` selector resolved through the real Identity store into real persisted GUID identities; no fabricated role/capability claims; production capability authorization, Identity challenge/forbid schemes retained (8 tests)
- T05: Real HTTP GET authorization matrix for `/Categories/Create` — anonymous 302 challenge, InventoryManager 200, Viewer access-denied forbid (3 tests)
- T06: Real rendered-antiforgery Category Create POST — extracted token + matching preserved cookie, real model binding/PageModel/Application/persistence, successful 302 PRG, same-factory persistence verification (1 test)
- T07: Real empty-Draft Purchase Order Submit domain-failure proof — actual `A purchase order must contain at least one item.` invariant through the real Details/Submit form, real `PurchaseOrder.Submit` authorization, existing `DomainException` redisplay (HTTP 200), navigation-state preservation, no-mutation verification (2 tests)
- T08: Verification-only integrated isolation/repeatability gate — T03–T07 subset 19/19 twice, Web.Tests 58/58 twice, default parallelism preserved, zero source changes
- T09: Documentation synchronization and Sprint 17 closure

### Final Test Baseline

```text
UnitTests:        346 passed
IntegrationTests:  92 passed
Web.Tests:         58 passed
Total:            496 passed, 0 failed, 0 skipped
Build:             0 errors (normal: 0 warnings; full rebuild: 28 pre-existing warnings)
EF:                no pending model changes
```

### Representative Coverage — not exhaustive

Sprint 17 proves the foundation through representative cases: one Category GET authorization matrix, one Category successful POST, and one Purchase Order domain-failure POST. It does NOT imply every Category or Purchase Order route now has HTTP integration coverage. Broader route coverage, browser/Playwright end-to-end automation, relational-provider (SQL Server) verification, and CI provider establishment remain deferred. No relational behavior is claimed from the InMemory-based test host.

### Boundaries Held

- No production source changed (the only production seam is the T02 `public partial class Program { }`)
- No antiforgery bypass, no token manufacturing, no HTML-parser package (narrow BCL-only form extraction)
- Default xUnit parallelism preserved; no global serialization, sleeps, or retries
- No version assigned, no tag created, no release published

## Sprint 18 - SQL Server Relational Verification

**Status:** Complete/Closed — technical/non-release sprint; v1.6.0 remains the latest release baseline. No version or tag was created.

Sprint 18 established an automated SQL Server-backed relational verification tier in `tests/InventoryPlatform.IntegrationTests/Relational/`. It runs only against `(localdb)\MSSQLLocalDB`, creates a unique `InventoryPlatformRelationalTests_<guid>` database per test, validates the server and database prefix before create/migrate/drop operations, fails hard when LocalDB is unavailable, uses trusted authentication, never reads production `DefaultConnection`, invokes no production seeders, and retains default xUnit parallelism.

### Completed

- R1: Real `MigrateAsync()` on a fresh database; all 10 migrations applied through `20260831141400_CreateAuthorizationSchema`; zero pending migrations afterward
- R2: `IX_Products_Sku` rejected a duplicate Product SKU with SQL error 2601
- R3: `FK_Products_Categories_CategoryId` rejected deletion of a referenced Category with SQL error 547
- R4: One `SaveChangesAsync()` containing valid and constraint-violating pending writes persisted neither write; this is not a guarantee about every application workflow
- R5: Representative `Product.QuantityOnHand` `decimal(18,2)` persistence was observed before being asserted; the evidence does not assign conversion responsibility to EF or SQL Server
- R6: The actual `GetInventoryMovementHandler` → `InventoryMovementRepository.GetInventoryMovementAsync` query translated and executed on SQL Server with deterministic aggregates; this does not cover all reports
- T06 integrated verification and T07 documentation closure completed

### Final Verification Baseline

```text
Relational:        41 passed
UnitTests:        346 passed
IntegrationTests: 133 passed
Web.Tests:         58 passed
Total:            537 passed, 0 failed, 0 skipped
Build:             0 errors (normal: 0 warnings; non-incremental: 28 unchanged historical warnings)
EF:                no pending model changes
Migrations:        10; latest 20260831141400_CreateAuthorizationSchema
```

### Boundaries Held

- Zero production behavior/source, EF model/mapping, migration, package/project, startup, or configuration changes
- Coverage is limited to R1-R6; broader relational/report and HTTP/Razor coverage remain deferred
- CI and a future CI SQL Server endpoint remain deferred
- SQLite remains rejected as an equivalent SQL Server verification provider

## Sprint 19 - Purchase Order Create HTTP/Razor Integration Coverage

**Status:** Complete/Closed — technical/non-release sprint; v1.6.0 remains the latest release baseline. No version or tag was created.

Sprint 19 extended the Sprint 17 database-safe HTTP/Razor integration-test foundation to verify the real Purchase Order Create pipeline end-to-end. Five accepted HTTP behaviors (H1-H5) now cover representative authorization, antiforgery, model binding, persistence, PRG, expected Domain-failure redisplay with restoration, and no-mutation scenarios for `/Purchasing/PurchaseOrders/Create` — all through the real ASP.NET Core pipeline with production capability authorization and rendered antiforgery, on EF Core InMemory per-factory test hosts.

### Completed

- T01: Contract and Test-Seam Lock
- T02: GET Authorization Coverage (H1 anonymous challenge, H2 authorized manager access, H3 real-capability denial)
- T03: Valid POST Coverage (H4 antiforgery POST → 302 PRG → same-factory persistence; marker `S19-T03-VALID-CREATE`)
- T04: Duplicate-Product Failure Coverage (H5 canonical `The product already exists in this purchase order.` redisplay/restoration, no mutation; marker `S19-T04-DUPLICATE`)
- T05: Integrated Regression and Architecture Verification — all eight gates pass; 542/542 fresh; LocalDB available so the relational tier ran freshly
- T06: Documentation Synchronization and Sprint Closure

### H1-H5 Behavioral Coverage

- **H1:** Anonymous GET `/Purchasing/PurchaseOrders/Create` challenges to Identity login with the correct semantic ReturnUrl
- **H2:** Persisted seeded `manager@inventory.local` accesses the real Create page
- **H3:** Persisted test-only `purchaseorder-denied@inventory.test`, with no authorization-group assignment, is denied by real `PurchaseOrder.Create` capability authorization. The seeded Viewer is NOT the H3 identity because the Viewer already possesses `PurchaseOrder.Create`. No production seed or authorization bypass was added.
- **H4:** Real GET + real antiforgery token/cookie + indexed POST → real PageModel/Application handler → immediate 302 PRG to `/Purchasing/PurchaseOrders` → same-factory persistence of a Draft PO plus one item
- **H5:** Duplicate ProductId rows trigger the real Domain invariant; HTTP 200 redisplay with Supplier/Product options, selection, date, remarks, and both rows/values restored; PO/item counts unchanged and no order persisted

H4/H5 are EF Core InMemory HTTP-host evidence, not SQL Server relational evidence. Sprint 18 remains the relational architecture authority.

### Final Test Baseline

```text
UnitTests:        346 passed
IntegrationTests: 133 passed (relational tier freshly passed against available LocalDB)
Web.Tests:         63 passed (H1-H5 included)
Total:            542 passed, 0 failed, 0 skipped
Build:             0 errors (normal: 0 warnings; non-incremental: 28 unchanged historical warnings)
EF:                no pending model changes
Migrations:        10; latest 20260831141400_CreateAuthorizationSchema (unchanged; none created)
```

### Boundaries Held

- Zero production behavior/source, test-source, seed/policy/config, migration, package, or project changes — verification and documentation only from T05 onward
- No antiforgery bypass, no fabricated capability claims, no third-party HTML parser, no generic browser/HTTP DSL
- No relational claim is made from InMemory HTTP evidence
- No version assigned, no tag created, no release published

### Unassigned Carry-Forward (not assigned to any future sprint)

C19-01 validation invocation/architecture; C19-02 provider-neutral continuous verification; C19-03 relational CI; C19-04 broader HTTP/Razor coverage; C19-05 browser/E2E; C19-06 EditStatus authorization; C19-07 report authorization; C19-08 warning remediation; C19-09 broader SQL relational/report verification; C19-10 Sales; C19-11 Audit; C19-12 import/attachment/barcode. Observation preserved: the seeded Viewer has broad `PurchaseOrder.*`, including `PurchaseOrder.Create` — recorded only; no remediation commitment.

## Sprint 20 - Provider-Neutral Continuous Verification

**Status:** Complete/Closed — technical/non-release sprint; v1.6.0 remains the latest release baseline. No version or tag was created.

Sprint 20 established provider-neutral continuous verification: an explicit, fail-safe IntegrationTest tier contract, a single shared verification command used identically locally and in CI, and a GitHub Actions provider-neutral gate with a successful hosted run. The SQL Server relational tier and browser/E2E remain outside the provider-neutral gate.

### Delivered

- Explicit `TestTier` contract: every IntegrationTest resolves to exactly one supported tier — `ProviderNeutral` or `SqlServerRelational`. Classification follows actual runtime provider dependency, not folder/namespace; tests that connect or attempt to connect to SQL Server/LocalDB belong to `SqlServerRelational`.
- Fail-safe classification audit: rejects missing, duplicate/multiple, and unknown tier values; locks the SQL-provider-bound inventory by fully qualified identity (14 identities); the audit is ProviderNeutral and runs inside every gate invocation.
- Shared verification entry point `scripts/verify-provider-neutral.ps1`: tool restore (`dotnet-ef 10.0.10`), tool-resolution evidence, solution restore, normal Release build, UnitTests, Web.Tests, affirmative `TestTier=ProviderNeutral` IntegrationTests, EF `migrations has-pending-model-changes`, TRX output to `artifacts/verification/`, explicit executed/excluded tier summary, strict exit-code propagation, working-directory independence.
- GitHub Actions workflow `.github/workflows/provider-neutral-verification.yml`: PR→`main`, push→`main`, `workflow_dispatch`; `windows-latest`; .NET `10.0.x`; `contents: read`; full delegation to the shared script; TRX artifact upload with `if: always()`; no secrets, no database/browser setup. The hosted job `Provider-neutral verification (windows-latest)` completed successfully on a clean hosted runner, uploading a three-file `provider-neutral-verification-results` artifact.

### Verification-Driven Remediation

Initial classification placed one real LocalDB connection probe (`RelationalTier_FailsHard_WhenServerIsUnavailable`) in `ProviderNeutral`. T03 verification exposed actual provider contact (a stopped `MSSQLLocalDB` auto-started during a gated run); runtime behavior proved the test provider-dependent, and it was moved to `SqlServerRelational` with the lock updated intentionally — relational inventory 13 → 14, ProviderNeutral 127 → 126. The final provider-neutral gate proved zero provider contact: with `MSSQLLocalDB` stopped before an integrated run, it remained stopped afterward (last-start unchanged). Treated as resolved verification-driven hardening, not an open defect.

### Final Verification Baseline (Sprint 20 T05)

```text
UnitTests:               346 passed
Web.Tests:                63 passed
ProviderNeutral:         126 passed (classification audit included)
SqlServerRelational:      14 discovered only (NOT executed during Sprint 20)
Classification audit:      7/7 passed
EF:                        no pending model changes
Normal Release build:      0 warnings, 0 errors
Shared script exit:        0
LocalDB/SQL contact:       none
Hosted workflow:           PASS (qualified evidence; see retrospective)
```

Hosted evidence proves the hosted job, script step, summary step, and three-file artifact upload succeeded; detailed hosted test counts and EF output remain supported by the local/integrated verification record.

### Boundaries Held

- Zero production behavior/source changes; zero test/script/workflow source changes in T06
- No SQL Server relational test execution during Sprint 20 (discovery/lock verification only); no SQL relational-pass claim is made for Sprint 20
- No browser/E2E execution
- No version assigned, no tag created, no release published

### Unassigned Carry-Forward (not assigned to any future sprint)

C20-01 SQL Server relational CI (hosted relational execution needs a hosted SQL Server/LocalDB endpoint decision); C20-02 broader HTTP/Razor coverage; C20-03 browser/E2E; C20-04 EditStatus authorization guard decision; C20-05 report authorization; C20-06 warning remediation (28-warning non-incremental baseline unchanged); C20-07 broader SQL relational/report verification; C20-08 Sales; C20-09 Audit; C20-10 import/attachment/barcode; C20-11 validation invocation/architecture. Observation preserved: the seeded Viewer has broad `PurchaseOrder.*`, including `PurchaseOrder.Create` — recorded only; no remediation commitment.

## Next Sprint Planning

Sprint 11 through Sprint 20 are complete. Sprint 20 closed the C19-02 provider-neutral continuous verification carry-forward item: the provider-neutral CI foundation exists, the hosted workflow executed successfully, and the provider-neutral gate makes zero LocalDB/SQL Server provider contact. SQL Server relational CI remains future work (C20-01). The next activity requires separate planning; no Sprint 21 scope has been selected and none of the Sprint 20 carry-forward items are assigned to it.

## D1 - Documentation Synchronization

**Status: Complete**

Current-state documentation now reflects the verified Sprint 8 Purchasing sequence through P7, including the integrated verification result and the corrected pagination date-filter state preservation.

No implementation behavior was changed during D1.

## Later
- Additional Purchasing User Experience Improvements

Dynamic Capability-Based Authorization remains outside the completed Purchasing implementation scope. It is the next locked priority after Sprint 8 closure and must not be started automatically as part of this handoff.

---

# Current Release

## Version 1.5.0 - Sprint 8 Purchasing Enhancements

### Completed

- Multiple Purchase Order Item Management
- Purchase Order Search
- Purchase Order Filtering
- Purchase Order Sorting
- Purchase Order Pagination
- Inventory Synchronization During Receiving
- Integrated Purchasing Verification
- D1-D4 Sprint 8 documentation and closure

### Verification

- Complete Purchasing workflow from creation through receiving
- Search, date/status filtering, sorting, and pagination
- Inventory synchronization and StockIn transaction creation
- Existing authorization boundaries
- Relevant empty-result and failure/recovery behavior
- Pagination state preservation after the P7 correction

Sprint 8 is closed. Sprint 9 is the current code-quality workstream and does not change the release version.

---

# Historical Release

## Version 1.4.0 – Additional Reporting & Exports

### Completed

#### Profile

- User Profile
- Update Profile
- Self-Service Account Management

#### Password Management

- Change Password
- Forgot Password
- Reset Password
- Force Password Change

#### Email Verification

- Email Verification
- Verification Request
- Email Confirmation

#### Two-Factor Authentication

- 2FA Setup
- TOTP Verification
- 2FA Login Challenge
- Recovery Codes
- Recovery Code Login
- Recovery Code Regeneration
- Recovery Code Invalidation
- Disable 2FA

### Result

The Account Management vertical slice is now complete.

The implementation provides authenticated users with self-service
account management capabilities while preserving the existing
Clean Architecture, Vertical Slice Architecture, Application
handler patterns, Identity abstraction, and Razor Pages workflows.

The completed functionality includes:

- Self-service user profile management
- Password management
- Email verification
- Two-factor authentication
- Authenticator-based TOTP verification
- Recovery-code authentication
- Recovery-code regeneration
- Recovery-code invalidation
- 2FA disablement

The implementation was verified through actual browser workflows
and completed without requiring structural architectural redesign.

---

# Phase 1 — Foundation ✅

---

# Phase 2 — Inventory Core ✅

---

# Phase 3 — Identity & User Management ✅

---

# Phase 4 – Architecture Sprint 1

Objectives

- Review overall solution architecture
- Apply Rule of Three refactoring where justified
- Improve shared UI components
- Standardize Razor Page patterns
- Review dependency registration
- Update project documentation
- Prepare foundation for Purchasing

---

# Phase 5 — Purchasing Module

Status: ✅ Complete

Completed:

- Purchase Orders
- Purchase Order Items
- Purchase Approval Workflow
- Goods Receiving
- Partial Receiving
- Purchase Order Completion
- Purchasing Presentation Layer
- End-to-End Workflow Validation
- Inventory Integration
- Purchase Order Search
- Purchase Order Filtering
- Purchase Order Sorting
- Purchase Order Pagination

---

# Phase 6 — Reporting

Status: ✅ Complete

### Completed

- Inventory Valuation
- Purchase History
- Purchase History Search
- Purchase History Date Filtering
- Purchase History Pagination
- Purchase History Sorting
- Supplier Purchase Analysis
- Supplier Purchase Analysis Search
- Supplier Purchase Analysis Date Filtering
- Supplier Purchase Analysis Status Filtering
- Supplier Purchase Analysis Pagination
- Supplier Purchase Analysis Sorting
- Supplier Purchase Analysis Purchase Period
- Stock Movement
- Stock Movement Search
- Stock Movement Date Filtering
- Stock Movement Movement Type Filtering
- Stock Movement Pagination
- Stock Movement Sorting
- Low Stock Report
- Low Stock Search
- Low Stock Pagination
- Low Stock Sorting
- Inventory Movement Report
- Inventory Movement Search
- Inventory Movement Date Filtering
- Inventory Movement Reporting Period
- Inventory Movement Pagination
- Inventory Movement Sorting
- Product Reports
- Product Reports Search
- Product Reports Status Filtering
- Product Reports Pagination
- Product Reports Sorting
- Excel Export
- PDF Export

### Export Options

- Excel
- PDF

### Final Verification

- Empty database behavior verification
- Explicit query-failure testing
- Authorization regression
- Final build verification

### Additional Reporting

Additional Reporting was completed as Sprint 7 within the broader Phase 6 Reporting roadmap.

Completed Sprint 7 scope:

- Inventory Valuation
- Purchase History
- Supplier Purchase Analysis
- Stock Movement
- Low Stock Report
- Inventory Movement Report
- Product Reports
- Excel export
- PDF export
- Final project-wide verification

---

# Phase 7 — Dynamic Capability-Based Authorization

Status: ✅ Complete

**Implementation status:** T01-T14 complete. T15 (Final Verification & Retrospective) complete. v1.6.0 released.

### Objective

Evolve the existing Identity-based authorization model into a
dynamic capability-based authorization model.

### Model

```text
User
  ↓
Group
  ↓
Capabilities
  ↓
Application Action
  ↓
Domain State Validation
```

### Capabilities

Capabilities represent atomic application functionality,
actions, or permissions.

Examples:

- PurchaseOrder.View
- PurchaseOrder.Create
- PurchaseOrder.Edit
- PurchaseOrder.Submit
- PurchaseOrder.Approve
- PurchaseOrder.Reject
- PurchaseOrder.Receive

### Groups

Groups compose reusable capabilities into business
responsibilities.

Examples:

- PO Account
- IT Account
- Inventory Manager
- Viewer
- Administrator

### Implementation Goals

- Define capability catalog
- Define groups
- Map groups to capabilities
- Assign groups to users
- Introduce capability authorization
- Preserve existing Identity infrastructure where appropriate
- Preserve Domain state validation
- Apply authorization to Purchasing workflow
- Apply authorization to Reporting
- Validate UI and server-side authorization behavior

### Implementation Summary

- T01-T12: Full implementation (domain model, application abstractions, persistence, handlers, policies, admin UI, UI visibility)
- T13: Runtime verification complete (build success, authorization verified for all 3 seeded users)
- T14: Documentation synchronization complete
- T15: Final verification, retrospective, and save point — Complete

### Deferred Findings from T13

- InventoryManager group includes Administration.Access (seed data issue)
- InventoryManagement OR-composite grants broad access via single capability
- Categories/Edit missing [Authorize] attribute (pre-existing gap)
- Reports unrestricted (design decision pending)

### Sequencing

Sprint 10 implementation is complete. T15 closure gate is BLOCKED due to P1 findings requiring database remediation. Documentation and retrospective remain.

---

# Phase 8 — Advanced Features

Status: ⏳ Planned

Planned:

- Audit Trail
- Activity Logs
- File Uploads
- Barcode Scanner Integration
- Product Images
- QR Code Support
- Email Notifications
- Bulk Import
- Bulk Export

---

# Long-Term Goals

Future enhancements may include:

## Business Modules

- Purchasing Enhancements
- Sales
- Warehouse
- Inventory Transfers
- Cycle Counts
- Returns
- Stock Adjustments Approval

## Integrations

- REST API
- Barcode Scanner Integration

## Client Applications

- Mobile Application

## Intelligence

- Inventory Forecasting

---

# Planned Releases

| Version | Milestone |
|---------|-----------|
| v0.9.0 | Architecture Sprint 1 ✅ |
| v1.0.0 | Purchasing Application Layer ✅ |
| v1.1.0 | Purchasing Presentation Layer ✅ |
| v1.2.0 | Reporting — Inventory Valuation ✅ |
| v1.3.0 | Account Management ✅ |
| v1.4.0 | Additional Reporting & Exports — Released |
| v1.5.0 | Sprint 8 Purchasing Enhancements — Released |
| v1.6.0 | Dynamic Capability-Based Authorization ✅ |
| v2.0.0 | REST API & Blazor ⏳ |

---

# Guiding Principles

Each new module should:

- Reuse the shared paging infrastructure.
- Reuse the shared filtering infrastructure.
- Reuse the shared sorting infrastructure.
- Follow Clean Architecture.
- Maintain consistent UI behavior.
- Prefer composition over duplication.
- Reuse established application handler patterns.
- Maintain consistent Razor Pages workflows.
- Keep business rules inside domain entities.
- Favor consistency over premature abstraction.
- Prefer DTO projections for read-only reporting features.
- Encapsulate framework-specific implementations behind application abstractions.
- Apply the Rule of Three before introducing shared abstractions.
- Prefer complete vertical slices over isolated technical implementations.
- Validate new workflows through real application usage before considering the feature complete.
- Validate read-oriented queries against actual EF Core translation before introducing client-side evaluation.

The architecture should evolve through reuse rather than introducing module-specific implementations whenever possible.

## Sprint 7 Final Verification

- [x] Final Project-wide Verification

### P2 - Purchase Order Search - Complete

**Status: Complete and verified**

P2 implements server-side Purchase Order search using the existing Purchase Order listing/query architecture.

Verified behavior:
- Search by Purchase Order ID.
- Search by Supplier Name.
- Empty or whitespace-only search returns the normal unfiltered list.
- No-match searches return the correct empty result state.
- Search state is preserved through the applicable Purchase Order navigation.
- Existing authorization behavior remains intact.
- Existing Purchase Order list behavior outside search remains unchanged.

The project owner completed runtime/browser verification successfully after implementation.

No P3-P6 functionality was implemented as part of P2.

D1 documentation synchronization was completed after the Sprint 8 P7 verification.
