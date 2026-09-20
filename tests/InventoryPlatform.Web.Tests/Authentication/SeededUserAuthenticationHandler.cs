using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace InventoryPlatform.Web.Tests.Authentication;

public sealed class SeededUserAuthenticationHandler
    : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly SeededTestUserResolver _userResolver;

    public SeededUserAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        SeededTestUserResolver userResolver)
        : base(options, logger, encoder)
    {
        _userResolver = userResolver;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(
                TestAuthenticationDefaults.UserHeader,
                out var headerValue))
        {
            return AuthenticateResult.NoResult();
        }

        var selector = headerValue.ToString().Trim();
        var resolution = await _userResolver.ResolveAsync(selector);

        if (!resolution.IsSuccess)
        {
            return AuthenticateResult.Fail(
                resolution.FailureMessage ?? "Test-user authentication failed.");
        }

        var user = resolution.User!;
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName ?? selector)
        };
        var identity = new ClaimsIdentity(
            claims,
            TestAuthenticationDefaults.Scheme,
            ClaimTypes.Name,
            ClaimTypes.Role);
        var principal = new ClaimsPrincipal(identity);

        return AuthenticateResult.Success(
            new AuthenticationTicket(
                principal,
                TestAuthenticationDefaults.Scheme));
    }
}
