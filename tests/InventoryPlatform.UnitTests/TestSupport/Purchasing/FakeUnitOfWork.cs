using InventoryPlatform.Application.Interfaces.Persistence;

namespace InventoryPlatform.UnitTests.TestSupport.Purchasing;

/// <summary>
/// Hand-written fake for <see cref="IUnitOfWork"/>, shared by the Sprint 13 Purchasing
/// handler test tasks (T02-T04, three consumers) per the accepted Rule-of-Three
/// disposition in plan/SPRINT_13_PLANNING_REPORT.md Section 12.1.
///
/// The real <see cref="IUnitOfWork"/> contract is a single
/// <see cref="SaveChangesAsync(CancellationToken)"/> method - no transaction/begin-commit
/// API exists, so none is faked here. The fake records the number of save calls.
///
/// For cross-fake interaction-order assertions (T04: PO load → product load → Receive →
/// IncreaseStock → transaction add → save), the test creates one <see cref="CallOrder"/>
/// instance per test, passes that same instance to every participating fake, and each
/// fake records its interaction into it. The recorder is test-scoped state owned by the
/// test - no static or process-global mutable state is involved, so independently
/// executing xUnit tests cannot interfere.
///
/// The return value mirrors the real contract (the number of state entries written) and
/// defaults to 1 for a successful save; it is configurable only for tests that need to
/// exercise a non-default value.
/// </summary>
public sealed class FakeUnitOfWork : IUnitOfWork
{
    private readonly int _saveChangesResult;
    private readonly CallOrder? _callOrder;

    public FakeUnitOfWork(
        CallOrder? callOrder = null,
        int saveChangesResult = 1)
    {
        _callOrder = callOrder;
        _saveChangesResult = saveChangesResult;
    }

    // --- Interaction recording ---

    public int SaveChangesAsyncCallCount { get; private set; }

    // --- IUnitOfWork ---

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveChangesAsyncCallCount++;
        _callOrder?.Record("UnitOfWork.SaveChangesAsync");

        return Task.FromResult(_saveChangesResult);
    }
}

/// <summary>
/// Minimal test-scoped call-order recorder for Sprint 13 Purchasing interaction-order
/// assertions (e.g., T04: PO load → product load → Receive → IncreaseStock →
/// transaction add → save).
///
/// A test creates one instance, shares it with the fakes participating in that test, and
/// asserts on the recorded <see cref="Events"/> order. All state is instance-scoped:
/// independently executing tests never share recorded interactions. This is not a fake
/// framework - it is the smallest mechanism that lets T04 compare interaction order
/// across participating fakes (shared and test-local alike). Fakes record
/// fake-prefixed labels so interactions from different fakes remain distinguishable.
/// </summary>
public sealed class CallOrder
{
    private readonly List<string> _events = new();

    /// <summary>All recorded interaction labels, in the order they occurred.</summary>
    public IReadOnlyList<string> Events => _events;

    /// <summary>Records one interaction label. Called by participating fakes once per interaction.</summary>
    public void Record(string interaction) => _events.Add(interaction);
}
