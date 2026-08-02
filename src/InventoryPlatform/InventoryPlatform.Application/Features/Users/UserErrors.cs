using InventoryPlatform.Shared.Results;

namespace InventoryPlatform.Application.Features.Users;

public static class UserErrors
{
    public static Error NotFound(Guid guid) =>
        new(
            "User.NotFound",
            $"User {guid} not found.");

    public static readonly Error DuplicateUserName =
        new(
            "User.DuplicateUserName",
            "Duplicate Username.");

    public static readonly Error DuplicateEmail =
        new(
            "User.DuplicateEmail",
            "Duplicate Email.");

    public static readonly Error InvalidPassword =
        new(
            "User.InvalidPassword",
            "Invalid Password.");

    public static readonly Error InvalidEmail =
        new(
            "User.InvalidEmail",
            "Invalid Email.");
}