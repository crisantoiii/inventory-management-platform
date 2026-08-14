using InventoryPlatform.Shared.Results;

namespace InventoryPlatform.Application.Features.Account;

public static class AccountErrors
{

    public static Error UserNotFound(Guid guid) =>
        new("Account.UserNotFound",
            $"The current user {guid} could not be found.");

    public static Error NotFound(Guid guid) =>
        new("Account.NotFound", $"Account {guid} not found.");

}