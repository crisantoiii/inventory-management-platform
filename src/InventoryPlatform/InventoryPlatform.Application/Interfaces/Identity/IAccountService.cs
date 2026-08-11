using InventoryPlatform.Application.Features.Account.ResetPassword;
using InventoryPlatform.Application.Features.Account.ChangePassword;
using InventoryPlatform.Application.Features.Account.ForgotPassword;
using InventoryPlatform.Application.Features.Account.GetProfile;
using InventoryPlatform.Application.Features.Account.UpdateProfile;
using InventoryPlatform.Shared.Results;

namespace InventoryPlatform.Application.Interfaces.Identity;

public interface IAccountService
{
    Task<Result<GetProfileResponse>> GetProfileAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<Result> UpdateProfileAsync(
        UpdateProfileRequest request,
        CancellationToken cancellationToken = default);

    Task<Result> ChangePasswordAsync(
        ChangePasswordRequest request,
        CancellationToken cancellationToken = default);

    Task<Result> ForgotPasswordAsync(
        ForgotPasswordRequest request,
        CancellationToken cancellationToken = default);

    Task<Result> ResetPasswordAsync(
        ResetPasswordRequest request,
        CancellationToken cancellationToken = default);
}