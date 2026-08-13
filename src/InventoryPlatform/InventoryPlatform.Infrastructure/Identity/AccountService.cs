using InventoryPlatform.Application.Features.Account;
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
using InventoryPlatform.Application.Interfaces.Communication;
using InventoryPlatform.Application.Interfaces.Identity;
using InventoryPlatform.Shared.Results;
using Microsoft.AspNetCore.Identity;

namespace InventoryPlatform.Infrastructure.Identity;

public sealed class AccountService : IAccountService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailService _emailService;

    public AccountService(
        UserManager<ApplicationUser> userManager,
        IEmailService emailService)
    {
        _userManager = userManager;
        _emailService = emailService;
    }

    public async Task<Result<GetProfileResponse>> GetProfileAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user is null)
        {
            return Result<GetProfileResponse>.Failure(
                AccountErrors.NotFound(userId));
        }

        return Result<GetProfileResponse>.Success(
            new GetProfileResponse
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber,
                EmailConfirmed = user.EmailConfirmed,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                TwoFactorEnabled = user.TwoFactorEnabled
            });
    }

    public async Task<Result> UpdateProfileAsync(
        UpdateProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(
            request.UserId.ToString());

        if (user is null)
        {
            return Result.Failure(
                AccountErrors.UserNotFound(request.UserId));
        }

        user.PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber)
            ? null
            : request.PhoneNumber.Trim();

        user.PhoneNumberConfirmed = false;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            return Result.Failure(
                Error.Validation2(
                    string.Join(
                        Environment.NewLine,
                        result.Errors.Select(e => e.Description))));
        }

        return Result.Success();
    }

    public async Task<Result> ChangePasswordAsync(
        ChangePasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(
            request.UserId.ToString());

        if (user is null)
        {
            return Result.Failure(
                AccountErrors.NotFound(request.UserId));
        }

        var result = await _userManager.ChangePasswordAsync(
            user,
            request.CurrentPassword,
            request.NewPassword);

        if (!result.Succeeded)
        {
            return Result.Failure(
                Error.Validation2(
                    string.Join(
                        Environment.NewLine,
                        result.Errors.Select(e => e.Description))));
        }

        // A successful password change satisfies the
        // forced password change requirement.
        if (user.MustChangePassword)
        {
            user.MustChangePassword = false;

            var updateResult = await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                return Result.Failure(
                    Error.Validation2(
                        string.Join(
                            Environment.NewLine,
                            updateResult.Errors.Select(e => e.Description))));
            }
        }

        return Result.Success();
    }

    public async Task<Result> ForgotPasswordAsync(
        ForgotPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        // Do not reveal whether the account exists.
        if (user is null || !user.EmailConfirmed)
        {
            return Result.Success();
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        var resetUrl =
            $"/Account/ResetPassword?email={Uri.EscapeDataString(user.Email!)}&token={Uri.EscapeDataString(token)}";

        var body = $"""
        A password reset was requested for your Inventory Platform account.

        Use the following link to reset your password:

        {resetUrl}

        If you did not request this, you can safely ignore this message.
        """;

        await _emailService.SendAsync(
            user.Email!,
            "Inventory Platform - Password Reset",
            body,
            cancellationToken);

        return Result.Success();
    }

    public async Task<Result> ResetPasswordAsync(
        ResetPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user is null)
        {
            return Result.Failure(
                Error.Validation2(
                    "The password reset request is invalid."));
        }

        var result = await _userManager.ResetPasswordAsync(
            user,
            request.Token,
            request.NewPassword);

        if (!result.Succeeded)
        {
            return Result.Failure(
                Error.Validation2(
                    string.Join(
                        Environment.NewLine,
                        result.Errors.Select(e => e.Description))));
        }

        return Result.Success();
    }

    public async Task<Result<RequestEmailVerificationResponse>>
        RequestEmailVerificationAsync(
            RequestEmailVerificationRequest request,
            CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(
            request.UserId.ToString());

        if (user is null)
        {
            return Result<RequestEmailVerificationResponse>.Failure(
                AccountErrors.NotFound(request.UserId));
        }

        if (user.EmailConfirmed)
        {
            return Result<RequestEmailVerificationResponse>.Success(
                new RequestEmailVerificationResponse
                {
                    AlreadyVerified = true
                });
        }

        var token = await _userManager
            .GenerateEmailConfirmationTokenAsync(user);

        var verificationUrl =
            $"/Account/ConfirmEmail?userId={Uri.EscapeDataString(user.Id.ToString())}&token={Uri.EscapeDataString(token)}";

        var body = $"""
        Please verify your email address for your Inventory Platform account.

        Use the following link to verify your email:

        {verificationUrl}

        If you did not request this, you can safely ignore this message.
        """;

        await _emailService.SendAsync(
            user.Email!,
            "Inventory Platform - Verify Your Email",
            body,
            cancellationToken);

        return Result<RequestEmailVerificationResponse>.Success(
            new RequestEmailVerificationResponse
            {
                AlreadyVerified = false
            });
    }

    public async Task<Result> ConfirmEmailAsync(
        ConfirmEmailRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(
            request.UserId.ToString());

        if (user is null)
        {
            return Result.Failure(
                AccountErrors.NotFound(request.UserId));
        }

        if (user.EmailConfirmed)
        {
            return Result.Success();
        }

        var result = await _userManager.ConfirmEmailAsync(
            user,
            request.Token);

        if (!result.Succeeded)
        {
            return Result.Failure(
                Error.Validation2(
                    string.Join(
                        Environment.NewLine,
                        result.Errors.Select(e => e.Description))));
        }

        return Result.Success();
    }

    public async Task<Result<GetTwoFactorStatusResponse>>
    GetTwoFactorStatusAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(
            userId.ToString());

        if (user is null)
        {
            return Result<GetTwoFactorStatusResponse>.Failure(
                AccountErrors.NotFound(userId));
        }

        return Result<GetTwoFactorStatusResponse>.Success(
            new GetTwoFactorStatusResponse
            {
                TwoFactorEnabled = user.TwoFactorEnabled
            });
    }

    public async Task<Result<SetupTwoFactorResponse>>
    SetupTwoFactorAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(
            userId.ToString());

        if (user is null)
        {
            return Result<SetupTwoFactorResponse>.Failure(
                AccountErrors.NotFound(userId));
        }

        var authenticatorKey =
            await _userManager.GetAuthenticatorKeyAsync(user);

        if (string.IsNullOrWhiteSpace(authenticatorKey))
        {
            await _userManager.ResetAuthenticatorKeyAsync(user);

            authenticatorKey =
                await _userManager.GetAuthenticatorKeyAsync(user);
        }

        if (string.IsNullOrWhiteSpace(authenticatorKey))
        {
            return Result<SetupTwoFactorResponse>.Failure(
                Error.Validation2(
                    "Unable to initialize authenticator setup."));
        }

        return Result<SetupTwoFactorResponse>.Success(
            new SetupTwoFactorResponse
            {
                AuthenticatorKey = authenticatorKey
            });
    }

    public async Task<Result<VerifyTwoFactorResponse>>
        VerifyTwoFactorAsync(
            Guid userId,
            string code,
            CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(
            userId.ToString());

        if (user is null)
        {
            return Result<VerifyTwoFactorResponse>.Failure(
                AccountErrors.NotFound(userId));
        }

        var isValid = await _userManager.VerifyTwoFactorTokenAsync(
            user,
            _userManager.Options.Tokens.AuthenticatorTokenProvider,
            code);

        if (!isValid)
        {
            return Result<VerifyTwoFactorResponse>.Failure(
                Error.Validation2(
                    "The verification code is invalid."));
        }

        var result = await _userManager.SetTwoFactorEnabledAsync(
            user,
            true);

        var recoveryCodes =
        await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(
            user,
            10);

        if (!result.Succeeded)
        {
            return Result<VerifyTwoFactorResponse>.Failure(
                Error.Validation2(
                    "Unable to enable two-factor authentication."));
        }

        return Result<VerifyTwoFactorResponse>.Success(
            new VerifyTwoFactorResponse
            {
                Enabled = true,
                RecoveryCodes = recoveryCodes.ToArray()
            });
    }

    public async Task<Result<GenerateTwoFactorRecoveryCodesResponse>>
    GenerateTwoFactorRecoveryCodesAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(
            userId.ToString());

        if (user is null)
        {
            return Result<GenerateTwoFactorRecoveryCodesResponse>.Failure(
                AccountErrors.NotFound(userId));
        }

        if (!user.TwoFactorEnabled)
        {
            return Result<GenerateTwoFactorRecoveryCodesResponse>.Failure(
                Error.Validation2(
                    "Two-factor authentication is not enabled."));
        }

        var recoveryCodes =
            await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(
                user,
                10);

        return Result<GenerateTwoFactorRecoveryCodesResponse>.Success(
            new GenerateTwoFactorRecoveryCodesResponse
            {
                RecoveryCodes = recoveryCodes.ToArray()
            });
    }

    public async Task<Result> DisableTwoFactorAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(
            userId.ToString());

        if (user is null)
        {
            return Result.Failure(
                AccountErrors.NotFound(userId));
        }

        if (!user.TwoFactorEnabled)
        {
            return Result.Failure(
                Error.Validation2(
                    "Two-factor authentication is already disabled."));
        }

        var result = await _userManager.SetTwoFactorEnabledAsync(
            user,
            false);

        if (!result.Succeeded)
        {
            return Result.Failure(
                Error.Validation2(
                    "Unable to disable two-factor authentication."));
        }

        return Result.Success();
    }

    public async Task<Result<RegenerateTwoFactorRecoveryCodesResponse>>
        RegenerateTwoFactorRecoveryCodesAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(
            userId.ToString());

        if (user is null)
        {
            return Result<RegenerateTwoFactorRecoveryCodesResponse>.Failure(
                AccountErrors.NotFound(userId));
        }

        if (!user.TwoFactorEnabled)
        {
            return Result<RegenerateTwoFactorRecoveryCodesResponse>.Failure(
                Error.Validation2(
                    "Two-factor authentication is not enabled."));
        }

        var recoveryCodes =
            await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(
                user,
                10);

        return Result<RegenerateTwoFactorRecoveryCodesResponse>.Success(
            new RegenerateTwoFactorRecoveryCodesResponse
            {
                RecoveryCodes = recoveryCodes.ToArray()
            });
    }
}