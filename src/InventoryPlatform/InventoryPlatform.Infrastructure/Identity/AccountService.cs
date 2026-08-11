using InventoryPlatform.Application.Features.Account;
using InventoryPlatform.Application.Features.Account.ResetPassword;
using InventoryPlatform.Application.Features.Account.ChangePassword;
using InventoryPlatform.Application.Features.Account.ForgotPassword;
using InventoryPlatform.Application.Features.Account.GetProfile;
using InventoryPlatform.Application.Features.Account.UpdateProfile;
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
                PhoneNumberConfirmed = user.PhoneNumberConfirmed
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
                AccountErrors.UserNotFound( request.UserId ));
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
}