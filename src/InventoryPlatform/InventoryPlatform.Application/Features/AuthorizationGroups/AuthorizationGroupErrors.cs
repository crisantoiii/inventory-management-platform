using InventoryPlatform.Shared.Results;

namespace InventoryPlatform.Application.Features.AuthorizationGroups;

public static class AuthorizationGroupErrors
{
    public static Error NotFound(int id) =>
        new(
            "AuthorizationGroup.NotFound",
            $"Authorization group {id} not found.");

    public static readonly Error DuplicateName =
        new(
            "AuthorizationGroup.DuplicateName",
            "An authorization group with the same name already exists.");

    public static readonly Error HasAssignedUsers =
        new(
            "AuthorizationGroup.HasAssignedUsers",
            "Cannot delete a group that has users assigned. Remove all users first.");

    public static Error CapabilityNotFound(int id) =>
        new(
            "AuthorizationGroup.CapabilityNotFound",
            $"Capability {id} not found.");
}
