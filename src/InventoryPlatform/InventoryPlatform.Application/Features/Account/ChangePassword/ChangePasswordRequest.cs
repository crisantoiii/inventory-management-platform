using System;
using System.Collections.Generic;
using System.Text;

namespace InventoryPlatform.Application.Features.Account.ChangePassword;

public sealed record ChangePasswordRequest
{
    public Guid UserId { get; init; }

    public string CurrentPassword { get; init; } = string.Empty;

    public string NewPassword { get; init; } = string.Empty;
}
