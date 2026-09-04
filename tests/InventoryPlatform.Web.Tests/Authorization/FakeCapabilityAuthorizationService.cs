using InventoryPlatform.Application.Interfaces.Authorization;

namespace InventoryPlatform.Web.Tests.Authorization;

/// <summary>
/// Hand-written fake implementation of <see cref="ICapabilityAuthorizationService"/>
/// for Web.Tests handler tests. Provides deterministic, configurable authorization
/// outcomes without database access or mocking frameworks.
/// </summary>
public sealed class FakeCapabilityAuthorizationService : ICapabilityAuthorizationService
{
    private readonly Dictionary<(Guid UserId, string CapabilityName), bool> _results = new();
    private bool _defaultResult;

    /// <summary>
    /// The most recent user ID passed to <see cref="HasCapabilityAsync"/>.
    /// </summary>
    public Guid? LastRequestedUserId { get; private set; }

    /// <summary>
    /// The most recent capability name passed to <see cref="HasCapabilityAsync"/>.
    /// </summary>
    public string? LastRequestedCapabilityName { get; private set; }

    /// <summary>
    /// Total number of times <see cref="HasCapabilityAsync"/> was called.
    /// </summary>
    public int CallCount { get; private set; }

    /// <summary>
    /// Sets the default result returned for any user/capability combination
    /// not explicitly configured via <see cref="SetResult"/>.
    /// </summary>
    public void SetDefaultResult(bool result)
    {
        _defaultResult = result;
    }

    /// <summary>
    /// Configures a specific authorization result for a given user and capability.
    /// </summary>
    public void SetResult(Guid userId, string capabilityName, bool result)
    {
        _results[(userId, capabilityName)] = result;
    }

    /// <summary>
    /// Configures an authorization result for all users for a given capability.
    /// </summary>
    public void SetResultForCapability(string capabilityName, bool result)
    {
        _results[(Guid.Empty, capabilityName)] = result;
    }

    /// <summary>
    /// Resets all configured results, call tracking, and default result.
    /// </summary>
    public void Reset()
    {
        _results.Clear();
        _defaultResult = false;
        LastRequestedUserId = null;
        LastRequestedCapabilityName = null;
        CallCount = 0;
    }

    public Task<bool> HasCapabilityAsync(
        Guid userId,
        string capabilityName,
        CancellationToken cancellationToken = default)
    {
        LastRequestedUserId = userId;
        LastRequestedCapabilityName = capabilityName;
        CallCount++;

        // 1. Exact user + exact capability
        if (_results.TryGetValue((userId, capabilityName), out var result))
        {
            return Task.FromResult(result);
        }

        // 2. Any user (wildcard) + exact capability (SetResultForCapability)
        if (_results.TryGetValue((Guid.Empty, capabilityName), out result))
        {
            return Task.FromResult(result);
        }

        return Task.FromResult(_defaultResult);
    }
}
