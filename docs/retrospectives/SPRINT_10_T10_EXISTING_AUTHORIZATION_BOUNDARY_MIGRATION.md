# Sprint 10 T10 - Existing Authorization Boundary Migration Result

## 1. Task

T10 - Existing Authorization Boundary Migration
STATUS: COMPLETE

## 2. Objective

Migrate the remaining existing authorization boundaries to capability-backed authorization where justified.

## 3. Required Inspection Results

### Inventory of Authorization Boundaries

**Static role-based policies (to migrate):**
- Administrator -- RequireRole("Administrator") -- 16 page models
- InventoryManagement -- RequireRole("Administrator", "InventoryManager") -- 8 page models
- ViewInventory -- RequireRole("Administrator", "InventoryManager", "Viewer") -- 13 page models

**Already capability-backed (T09 - excluded):**
- PurchaseOrder.View, .Create, .Submit, .Approve, .Receive

**Folder-level conventions:**
- / -- AuthorizeFolder("/") (authenticated user gate) -- unchanged
- /Products -- AuthorizeFolder("/Products") (authenticated user gate) -- unchanged
- /Administration -- migrated from role name to policy name
- /Inventory -- migrated from role names to policy name

**Razor User.IsInRole checks:** 46 occurrences across 14 Razor view files -- deferred to T12

**Code-level IsInRole:** 1 occurrence in EditStatus.cshtml.cs line 61 -- unreachable dead code

## 4. Validation Pass

### Confirmed

1. Capability catalog: Administrator group gets all 37 capabilities. InventoryManager gets 18. Viewer gets 12.

2. ViewInventory OR-capability equivalence: OR-composite of 7 view capabilities grants access to exactly Administrator, InventoryManager, and Viewer.

3. InventoryManagement OR-capability equivalence: OR-composite of 9 capabilities grants access to exactly Administrator and InventoryManager.

4. Administration.Access: New capability, only in Administrator group via CapabilityCatalog.All.

5. EditStatus.cshtml.cs IsInRole: Unreachable dead code.

## 5. Architecture Decision

T10 migrates existing role-based authorization policies to capability-backed equivalents using the same policy names. The [Authorize(Policy = ...)] attributes on page models are unchanged.

OR-composite authorization uses MultiCapabilityRequirement + MultiCapabilityAuthorizationHandler with TRUE OR semantics.

## 6. Exact Files Changed

### File 1: AuthorizationSeeder.cs
Added Administration.Access capability to CapabilityCatalog.All.

### File 2: MultiCapabilityRequirement.cs (NEW)
OR-composite capability requirement with IReadOnlyList of capability names.

### File 3: MultiCapabilityAuthorizationHandler.cs (NEW)
OR-composite handler. Succeeds on first matching capability. Default deny if none match.

### File 4: CapabilityAuthorizationExtensions.cs
Added OR-composite AddCapabilityPolicy overload with params string[].

### File 5: AuthorizationPolicies.cs
Added AdministrationAccess, ViewInventoryCapabilities (7), InventoryManagementCapabilities (9).

### File 6: ServiceCollectionExtensions.cs
Replaced 3 role-based policies, 2 folder conventions, registered new handler.

## 7. Authorization Equivalence (Configuration-Level)

| Policy | Before | After | Groups With Access |
|---|---|---|---|
| Administrator | RequireRole | Single cap Administration.Access | Administrator only |
| InventoryManagement | RequireRole | OR-composite of 9 caps | Administrator, InventoryManager |
| ViewInventory | RequireRole | OR-composite of 7 caps | All three groups |

Configuration-level policy equivalence established through group-capability analysis; runtime authorization behavior not verified due to environment limitations.

## 8. Verification

### Build - SUCCESS
0 errors, 26 pre-existing warnings.

### Tests - NOT APPLICABLE
No automated test project exists.

### Database - NOT VERIFIED
No running SQL Server. Seed data will create Administration.Access on startup.

### Runtime/Browser - NOT VERIFIED
No running application. Configuration-level equivalence established.

### Source - VERIFIED
All 6 files inspected. No unintended changes.

## 9. Preservation

All 43 page-level [Authorize] attributes unchanged. All 46 Razor User.IsInRole checks unchanged. PurchaseOrder authorization (T09) unchanged. EditStatus.cshtml.cs unchanged.

## 10. Pre-existing Issues

1. Categories/Edit.cshtml.cs missing [Authorize] - any authenticated user can edit categories.
2. Suppliers/Create.cshtml.cs uses ViewInventory - Viewers can create suppliers.
3. EditStatus.cshtml.cs line 61 IsInRole is unreachable dead code.

## 11. Documentation Updated

- PROJECT_STATUS.md
- CHANGELOG.md
- docs/retrospectives/SPRINT_10_T10_EXISTING_AUTHORIZATION_BOUNDARY_MIGRATION.md
- docs/DESIGN_DECISIONS.md
- docs/ENGINEERING_JOURNAL.md

## 12. Deferred Work

- T12: Razor User.IsInRole migration
- T11: Authorization Administration
- Future: Categories/Edit authorization, Suppliers/Create policy

## 13. Next Task

T11 - Authorization Administration

## 14. STOP
