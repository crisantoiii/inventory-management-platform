using System.Security.Claims;
using InventoryPlatform.Application.Features.Account.UpdateProfile;
using InventoryPlatform.Application.Features.Account.GetProfile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InventoryPlatform.Web.Pages.Account;

public sealed class ProfileModel : PageModel
{
    private readonly GetProfileHandler _getProfileHandler;
    private readonly UpdateProfileHandler _updateProfileHandler;

    public ProfileModel(
        GetProfileHandler getProfileHandler,
        UpdateProfileHandler updateProfileHandler)
    {
        _getProfileHandler = getProfileHandler;
        _updateProfileHandler = updateProfileHandler;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public sealed class InputModel
    {
        public string? PhoneNumber { get; set; }
    }

    public GetProfileResponse? Profile { get; private set; }

    public async Task<IActionResult> OnGetAsync(
        CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out var userId))
        {
            return Challenge();
        }

        var result = await _getProfileHandler.HandleAsync(
            userId,
            cancellationToken);

        if (result.IsFailure)
        {
            return NotFound();
        }

        Profile = result.Value;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(
    CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out var userId))
        {
            return Challenge();
        }

        if (!ModelState.IsValid)
        {
            var profileResult = await _getProfileHandler.HandleAsync(
                userId,
                cancellationToken);

            if (profileResult.IsSuccess)
            {
                Profile = profileResult.Value;
            }

            return Page();
        }

        var request = new UpdateProfileRequest
        {
            UserId = userId,
            PhoneNumber = Input.PhoneNumber
        };

        var result = await _updateProfileHandler.HandleAsync(
            request,
            cancellationToken);

        if (result.IsFailure)
        {
            ModelState.AddModelError(
                string.Empty,
                result.Error.Message);

            var profileResult = await _getProfileHandler.HandleAsync(
                userId,
                cancellationToken);

            if (profileResult.IsSuccess)
            {
                Profile = profileResult.Value;
            }

            return Page();
        }

        return RedirectToPage();
    }
}