# Sprint 21 Retrospective - Validation Invocation Architecture

> **STATUS: FINAL — SPRINT COMPLETE / CLOSED.**
>
> Finalized at T05 from the externally accepted T01-T04 execution record.

## Sprint Identity

- **Sprint:** Sprint 21
- **Title:** Validation Invocation Architecture
- **Retrospective state:** FINAL
- **Sprint status:** COMPLETE / CLOSED
- **Release classification:** Technical/architecture — no semantic version increment or tag; v1.6.0 remains the latest release baseline
- **Carry-forward closed:** C20-11 and C19-01 (the same item, carried forward twice)

## 1. Objective

Close the recorded validation-invocation/architecture gap (C20-11, carried from Sprint 19 as C19-01) by making asynchronous Purchase Order Create request validation the first operation of the Application handler, under an explicit architecture decision, and prove it at the handler and real HTTP/Razor levels.

Before Sprint 21 the repository had two validation authorities but only one wired: FluentValidation validators for `CreatePurchaseOrderRequest` existed and were unit-tested, but no production path invoked them, so the runtime authority was the `PurchaseOrder` aggregate (`DomainException`) plus handler `Result` checks. The risk was divergence between tested validator rules and enforced runtime rules.

## 2. Delivered Outcome

- **Application-boundary validation invocation (Option A).** `CreatePurchaseOrderHandler` takes `IValidator<CreatePurchaseOrderRequest>` as a constructor dependency and calls `await _validator.ValidateAsync(request, cancellationToken)` as the literally first statement of `HandleAsync`, before any repository call.
- **Frozen A1 scalar Result contract.** An invalid result returns `Result<CreatePurchaseOrderResponse>.Failure(PurchaseOrderErrors.Validation(validationResult.Errors[0].ErrorMessage))` — exactly one error, selected deterministically as `Errors[0]`, message passed through verbatim, no sorting, grouping, concatenation, or field mapping.
- **Rule matrix frozen by the sprint.**
  - Retired: `RuleFor(x => x.Items).NotEmpty()`. No replacement null/count rule was added.
  - Retained top-level order: `SupplierId` → `ExpectedDeliveryDate` → `Remarks` (`MaximumLength(500)`).
  - Retained child order (`CreatePurchaseOrderItemValidator`): `ProductId` → `Quantity` (`GreaterThan(0)`) → `UnitCost` (`GreaterThanOrEqualTo(0)`).
  - Error precedence follows declaration order: `SupplierId` → `ExpectedDeliveryDate` → `Remarks` → first failing item in list order → its `ProductId` → `Quantity` → `UnitCost`.
- **Authority boundary is now explicit and single.** Application/FluentValidation owns request-shape input validation; the Domain aggregate retains all invariants, including duplicate `ProductId`, `Quantity > 0`, `UnitCost >= 0`, Draft-only mutation, and non-empty-Draft `Submit()`. `DomainException` still propagates uncaught from the handler; the Web layer still maps it to the existing inline ModelState presentation.
- **Handler lookup ownership unchanged.** Supplier and Product existence/active checks remain in the handler with their existing `Result` error factories; validation was not given ownership of reference data.
- **Empty-Draft behavior is unchanged and correct.** An empty item collection is valid input: `PurchaseOrder.Create` produces a Draft whose `Items` collection is empty, `AddAsync`/`SaveChangesAsync` persist it, and the handler returns success. This is distinct from `Submit()`, which remains Domain-invalid for an empty Draft.
- **Three distinct proof layers.** Validator rule tests (T01), handler invocation/contract tests (T02), and one real HTTP/Razor composition scenario (T03) — plus a provider-neutral integrated gate (T04).

## 3. Final Task Outcomes

| Task | Outcome | Status |
|---|---|---|
| T01 | Validator rule matrix + `PurchaseOrderErrors.Validation` contract | COMPLETE / ACCEPTED |
| T02 | `CreatePurchaseOrderHandler` invokes validation first; A1 contract, `Errors[0]`, token forwarding, empty Draft | COMPLETE / ACCEPTED |
| T03 | Real HTTP/Razor proof: validator-only failure (Remarks = 501) → model-level redisplay, restoration, no mutation | COMPLETE / ACCEPTED |
| T04 | Integrated provider-neutral verification + scope audit | COMPLETE / ACCEPTED (script exit 0) |
| T05 | Documentation synchronization and Sprint closure | COMPLETE |

## 4. Delivered Executable Files (9)

**Production (3):**

- `src/InventoryPlatform/InventoryPlatform.Application/Features/Purchasing/CreatePurchaseOrder/CreatePurchaseOrderValidator.cs`
- `src/InventoryPlatform/InventoryPlatform.Application/Features/Purchasing/CreatePurchaseOrder/CreatePurchaseOrderHandler.cs`
- `src/InventoryPlatform/InventoryPlatform.Application/Features/Purchasing/PurchaseOrderErrors.cs`

**Unit tests (3):**

- `tests/InventoryPlatform.UnitTests/Application/Purchasing/CreatePurchaseOrderValidatorTests.cs`
- `tests/InventoryPlatform.UnitTests/Application/Purchasing/CreatePurchaseOrderHandlerTests.cs`
- `tests/InventoryPlatform.UnitTests/Application/Purchasing/PurchaseOrderErrorsTests.cs`

**Web tests (1):**

- `tests/InventoryPlatform.Web.Tests/Http/PurchaseOrderCreateAuthorizationTests.cs`

**Accepted conditional test-support (2):** recording-only cancellation-token observation added to the two shared fakes already recording call order —

- `tests/InventoryPlatform.UnitTests/TestSupport/Purchasing/FakePurchaseOrderRepository.cs`
- `tests/InventoryPlatform.UnitTests/TestSupport/Purchasing/FakeUnitOfWork.cs`

The conditionally-permitted extraction-helper change was **not** needed: the existing `PurchaseOrderCreateFormExtraction` covered every T03 assertion, so `PurchaseOrderCreateFormExtraction.cs` is unchanged.

## 5. Final Verification Baseline (Sprint 21 T04 — fresh, current environment)

```text
UnitTests:               355 passed
Web.Tests:                64 passed
ProviderNeutral:         126 passed (classification audit included)
SqlServerRelational:      14 discovered only (NOT executed during Sprint 21)
EF pending-model:          none
Normal Release build:      0 warnings, 0 errors
Shared script exit:        0
Migration chain:          unchanged (10, latest 20260831141400_CreateAuthorizationSchema)
Browser/E2E:               NOT EXECUTED (outside the gate)
```

Provider-neutral gate total: **545 passed, 0 failed, 0 skipped** (355 + 64 + 126), up from Sprint 20's 535 (+9 UnitTests from the new handler tests, +1 from the new error-contract test, +1 from the new HTTP scenario). Sprint 21 makes **no** SQL relational-pass claim; the 14 relational tests remain discovered and locked only, exactly as in Sprint 20.

Targeted evidence per task:

- **T01 focused:** 39 passed / 0 failed / 0 skipped (`CreatePurchaseOrderValidatorTests` + `CreatePurchaseOrderItemValidatorTests` + `PurchaseOrderErrorsTests`).
- **T02 focused:** 145 passed / 0 failed / 0 skipped (handler, validator, item-validator, error-contract, and Domain `PurchaseOrder` tests).
- **T03 focused:** 6 passed / 0 failed / 0 skipped (H1-H6 Purchase Order Create HTTP fixture).
- **T04:** `scripts/verify-provider-neutral.ps1` exit code `0`, run three times with identical results.

## 6. Behavioral Evidence Highlights

- Validation failure returns `Error.Code == "PurchaseOrder.Validation"` and the verbatim first FluentValidation message (`A valid supplier must be selected.`, `A valid product must be selected.`, `The length of 'Remarks' must be 500 characters or fewer. You entered 501 characters.`).
- On a validation failure the handler performs **zero** supplier reads, **zero** product reads, **zero** `AddAsync`, **zero** `SaveChangesAsync`; the shared `CallOrder` recorder observed no events at all.
- Deterministic selection: a request failing `SupplierId`, `ExpectedDeliveryDate`, `Remarks`, and an all-invalid item returns the `SupplierId` message; a request failing only an item's `ProductId`, `Quantity`, and `UnitCost` returns the `ProductId` message.
- The same `CancellationToken` supplied to `HandleAsync` reaches `ValidateAsync`, both `GetByIdAsync` calls, `AddAsync`, and `SaveChangesAsync`.
- Duplicate product: validation passes, supplier is read, the second `AddItem` reaches the Domain and throws the canonical `The product already exists in this purchase order.`; `AddAsync` = 0 and `SaveChangesAsync` = 0.
- HTTP: an over-length `Remarks` POST through the real rendered form with real antiforgery token and cookie returns HTTP 200 (not a redirect, `Location` null), presents the exact message in the model-level `asp-validation-summary="ModelOnly"` region, restores supplier/product options, selection, date, the full 501-character remarks value, and the single item row, and leaves Purchase Order and Purchase Order Item counts unchanged.
- Assertion liveness was proven for the HTTP message by a deliberate mutation (reverted) that made the test fail with `Assert.Contains() Failure: Sub-string not found`.

## 7. What Went Well

- **Architecture decision before implementation avoided a second seam.** Choosing the Application handler boundary (Option A) over PageModel invocation, a decorator, or a global pipeline meant the Web layer needed zero production change — the existing failed-`Result` branch already presented the scalar error correctly.
- **A1 scalar `Errors[0]` is small enough to fully verify and deterministic by construction.** Freezing declaration order as the precedence rule made "first failure" an observable, testable property rather than an ordering accident.
- **Retiring `Items.NotEmpty()` without a replacement fixed a real product defect.** The real Create form can submit zero item rows; the previous rule would have rejected a legitimate empty Draft.
- **Three proof layers answered three different questions.** Validator tests prove the rule matrix, handler tests prove invocation and the Result contract, the HTTP test proves composition and presentation. No single layer could have proven production invocation before Sprint 21.
- **Verification found the seam honestly.** T04's scope audit confirmed the historical 28-warning non-incremental baseline originates entirely in files Sprint 21 never touched, and that zero diagnostics come from Sprint 21 files.

## 8. Important Observations

- The provider-neutral gate is provider-neutral: the `SqlServerRelational` runtime suites produced zero rows in the provider-neutral TRX, so no SQL Server or LocalDB was contacted during Sprint 21 verification.
- Validation is invoked for Purchase Order **Create** only. `CreatePurchaseOrderHandler` is the single handler in the repository with an injected `IValidator<T>`; other features' validators remain defined and unit-tested but not invoked in production paths. Extending invocation to other features is a future, separately-planned concern.
- The 28-warning non-incremental historical build baseline is unchanged and remains deferred (C20-06).
- No migration, schema, package, project, configuration, CI, relational, or release change was introduced.

## 9. Scope Discipline

- Production changes limited to the three Application files listed in §4; no Web/Razor/PageModel, Domain, Shared `Result`/`Error`, or DI change.
- No A2 structured error contract, no field-level property mapping or prefix adapter, no global validation pipeline or decorator, no MediatR, no automatic MVC FluentValidation integration.
- No new validation rule beyond retiring `Items.NotEmpty()`; no trimming, normalization, or date-chronology rule.
- No duplicate-product validator rule: the Domain aggregate remains the sole owner.
- No SQL Server relational execution, no browser/E2E, no route-coverage expansion beyond the single new Create scenario.
- No semantic version, tag, or release; v1.6.0 remains the latest release baseline.
- No Git operations from this workspace.

## 10. Carry-Forward

**Closed by Sprint 21:**

- C20-11 validation invocation/architecture (the same item carried forward from Sprint 19 as C19-01) — closed. The validators are now invoked at the Application handler boundary under a recorded architecture decision (DD-046), with handler-level and real HTTP/Razor evidence.

**Remaining unassigned (unchanged, not reallocated):**

- C20-01 SQL Server relational CI (hosted relational execution needs a hosted SQL Server/LocalDB endpoint decision)
- C20-02 broader HTTP/Razor coverage
- C20-03 browser/E2E
- C20-04 EditStatus authorization guard decision
- C20-05 report authorization
- C20-06 warning remediation (the 28-warning non-incremental baseline)
- C20-07 broader SQL relational/report verification
- C20-08 Sales
- C20-09 Audit
- C20-10 import/attachment/barcode
- Observation preserved without a remediation commitment: the seeded Viewer has broad `PurchaseOrder.*`, including `PurchaseOrder.Create`

No Sprint 22 scope is selected. The next activity requires separate planning; Sprint 21 did not allocate any remaining carry-forward item.

## 11. Closure

Sprint 21 is complete and closed on the accepted T04 integrated verification baseline (provider-neutral gate exit 0; 355 UnitTests, 64 Web.Tests, 126 ProviderNeutral IntegrationTests; 0 failed, 0 skipped; normal Release build 0 warnings/0 errors; no pending EF model changes). T05 changed documentation only. Graphify was not updated because repository convention does not include documentation-only changes in the source graph. Git operations remain owner-controlled and were not performed.

Retrospective: `docs/retrospectives/SPRINT_21_VALIDATION_INVOCATION_ARCHITECTURE.md`.
Architecture decision: `docs/DESIGN_DECISIONS.md` — DD-046.
