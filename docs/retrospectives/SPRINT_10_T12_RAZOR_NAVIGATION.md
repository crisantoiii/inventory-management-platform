# Sprint 10 T12 - Razor Navigation & Capability Visibility Result

## 1. Task

T12 - Razor Navigation & Capability Visibility
STATUS: COMPLETE

## 2. Objective

Replace role-based UI visibility checks with capability-backed authorization in all affected Razor views, using the existing policies established in T08/T10.

## 3. Implementation Summary

### Mechanism Migration

Migrated 45 `User.IsInRole(...)` occurrences across 14 Razor `.cshtml` files to `(await AuthorizationService.AuthorizeAsync(User, null, policyName)).Succeeded`. The shared layout navigation admin links were also migrated.

One dead-code `User.IsInRole(...)` occurrence in `EditStatus.cshtml.cs` was documented but intentionally left unchanged (unreachable behind class-level `[Authorize(Policy = Administrator)]`).

### Pattern Applied

Each affected file received:
1. `@using Microsoft.AspNetCore.Authorization` (framework namespace for `IAuthorizationService`)
2. `@inject IAuthorizationService AuthorizationService`
3. Pre-computed boolean variables (`canManage`, `canAdmin`) in a `@{ }` block
4. Replacement of `User.IsInRole(...)` conditions with pre-computed booleans
5. Removal of `@using InventoryPlatform.Infrastructure.Identity;` (no longer needed)

### Mapping

| Existing Role Check | Replacement Policy | Seeded-User Equivalence |
|---|---|---|
| `User.IsInRole("Administrator")` | `AuthorizationPolicies.Administrator` | Yes |
| `User.IsInRole("Administrator") \|\| User.IsInRole("InventoryManager")` | `AuthorizationPolicies.InventoryManagement` | Yes |

### Global Import

`@using InventoryPlatform.Web.Authorization` was added to `_ViewImports.cshtml` to make `AuthorizationPolicies` available globally, consistent with the existing `@using InventoryPlatform.Web` base import.

## 4. Verification

### Build - SUCCESS
0 errors, 20 pre-existing warnings.

### Source Verification - SUCCESS
Six separate searches confirmed correct migration:
1. `User.IsInRole(` in `.cshtml` → 0 results
2. `IsInRole(` in `.cs` → 1 result (dead code in EditStatus.cshtml.cs)
3. `IdentityConstants.Roles` in `.cshtml` → 0 results
4. `IAuthorizationService` in `.cshtml` → 14 files
5. `AuthorizationPolicies.InventoryManagement` in `.cshtml` → 12 files
6. `AuthorizationPolicies.Administrator` in `.cshtml` → 9 files

### Browser/Runtime - NOT VERIFIED
No running application or database available in this environment.

### Tests - NOT APPLICABLE
No automated test project exists in the repository.

## 5. Files Changed

### Modified (15 files)

- `Pages/_ViewImports.cshtml` — Added `@using InventoryPlatform.Web.Authorization`
- `Pages/Shared/_Layout.cshtml` — Replaced admin nav role check with capability-backed check
- `Pages/Products/Index.cshtml` — 5 role checks → canManage/canAdmin
- `Pages/Products/Details.cshtml` — 2 role checks → canManage
- `Pages/Categories/Index.cshtml` — 5 role checks → canManage/canAdmin
- `Pages/Categories/Details.cshtml` — 2 role checks → canManage
- `Pages/Units/Index.cshtml` — 5 role checks → canManage/canAdmin
- `Pages/Units/Details.cshtml` — 2 role checks → canManage
- `Pages/Suppliers/Index.cshtml` — 5 role checks → canManage/canAdmin
- `Pages/Suppliers/Details.cshtml` — 2 role checks → canManage
- `Pages/Customers/Index.cshtml` — 5 role checks → canManage/canAdmin
- `Pages/Customers/Details.cshtml` — 2 role checks → canManage
- `Pages/InventoryTransactions/Index.cshtml` — 2 role checks → canManage
- `Pages/Administrator/Users/Index.cshtml` — 6 role checks → canManage/canAdmin
- `Pages/Administrator/Users/Details.cshtml` — 1 role check → canAdmin

### Intentionally Unchanged

- `Pages/Administrator/Users/EditStatus.cshtml.cs` — Dead-code `IsInRole` (unreachable, documented)
- All PageModel `.cshtml.cs` files — `[Authorize]` attributes unchanged
- All Domain, Application, Infrastructure files — No changes

## 6. Behavioral Equivalence

For the three seeded users (admin, manager, viewer), the capability-backed checks produce identical UI visibility as the replaced role checks. This was proven through the seed data chain:

- `IdentitySeeder.AssignUsersToGroupsAsync` assigns each user to the group matching their role
- `AuthorizationSeeder` assigns capabilities to groups matching the original role permissions

For non-seeded users, capability-backed authorization follows authorization-group membership rather than Identity role membership. This is an intentional consequence of using the authoritative capability model.

## 7. Pre-existing Discrepancies (Deferred)

- `Categories/Edit.cshtml.cs` missing `[Authorize]` — server-side gap, not repaired by T12
- `Units/Create.cshtml.cs` uses `Administrator` policy — InventoryManagers see button but get Access Denied
- `Suppliers/Create.cshtml.cs` uses `ViewInventory` policy — Viewers don't see button but server allows it
- `EditStatus.cshtml.cs` line 61 dead-code `IsInRole`

## 8. Documentation Updated

- PROJECT_STATUS.md
- CHANGELOG.md
- docs/retrospectives/SPRINT_10_T12_RAZOR_NAVIGATION.md

## 9. Next Task

T13 - Integrated Authorization Verification

## 10. STOP
