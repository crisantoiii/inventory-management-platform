using InventoryPlatform.Application.Features.Account.ChangePassword;
using InventoryPlatform.Application.Features.Account.ConfirmEmail;
using InventoryPlatform.Application.Features.Account.ForgotPassword;
using InventoryPlatform.Application.Features.Account.GenerateTwoFactorRecoveryCodes;
using InventoryPlatform.Application.Features.Account.GetProfile;
using InventoryPlatform.Application.Features.Account.GetTwoFactorStatus;
using InventoryPlatform.Application.Features.Account.RegenerateTwoFactorRecoveryCodes;
using InventoryPlatform.Application.Features.Account.RequestEmailVerification;
using InventoryPlatform.Application.Features.Account.ResetPassword;
using InventoryPlatform.Application.Features.Account.SetupTwoFactor;
using InventoryPlatform.Application.Features.Account.UpdateProfile;
using InventoryPlatform.Application.Features.Account.VerifyTwoFactor;
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

    Task<Result<RequestEmailVerificationResponse>> RequestEmailVerificationAsync(
        RequestEmailVerificationRequest request,
        CancellationToken cancellationToken = default);

    Task<Result> ConfirmEmailAsync(
        ConfirmEmailRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<GetTwoFactorStatusResponse>> GetTwoFactorStatusAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<Result<SetupTwoFactorResponse>> SetupTwoFactorAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<Result<VerifyTwoFactorResponse>> VerifyTwoFactorAsync(
        Guid userId,
        string code,
        CancellationToken cancellationToken = default);

    Task<Result<GenerateTwoFactorRecoveryCodesResponse>>
        GenerateTwoFactorRecoveryCodesAsync(
            Guid userId,
            CancellationToken cancellationToken = default);

    Task<Result> DisableTwoFactorAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<Result<RegenerateTwoFactorRecoveryCodesResponse>>
        RegenerateTwoFactorRecoveryCodesAsync(
            Guid userId,
            CancellationToken cancellationToken = default);
}