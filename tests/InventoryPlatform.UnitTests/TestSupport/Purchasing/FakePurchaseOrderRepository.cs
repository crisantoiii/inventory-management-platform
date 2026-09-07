using System.Linq.Expressions;
using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Domain.Entities;
using InventoryPlatform.Domain.Enums;
using InventoryPlatform.Shared.Paging;

namespace InventoryPlatform.UnitTests.TestSupport.Purchasing;

/// <summary>
/// Hand-written fake for <see cref="IPurchaseOrderRepository"/>, shared by the Sprint 13
/// Purchasing handler test tasks (T02-T05, four consumers) per the accepted Rule-of-Three
/// disposition in plan/SPRINT_13_PLANNING_REPORT.md Section 12.1.
///
/// Behavior is minimal and explicit:
/// - <see cref="GetByIdAsync"/> returns the configured aggregate (null = not found) and
///   records every requested id. The same prepared instance is returned on every call,
///   mirroring the tracked-aggregate contract the Submit/Approve/Receive handlers rely on.
/// - <see cref="AddAsync"/> records the entities passed by the handler.
/// - <see cref="GetPagedAsync"/> records the query/filter arguments and returns the
///   configured <see cref="PagedResult{PurchaseOrder}"/> (or a truthful empty page for the
///   requested page when no result is configured).
/// - every other interface member fails fast with <see cref="NotSupportedException"/>:
///   no accepted Sprint 13 handler test exercises it (verified against the current
///   Purchasing handlers), and silent misleading defaults are forbidden.
///
/// Extend the supported surface deliberately (with external review) if a future test
/// needs more. This is not a generic in-memory repository or a fake framework.
///
/// For cross-fake interaction-order assertions (T04), the test may create one
/// <see cref="CallOrder"/> instance per test and pass that same instance to every
/// participating fake; this fake then records its supported interactions into it. The
/// recorder is test-scoped state - no static or process-global mutable state is involved.
/// </summary>
public sealed class FakePurchaseOrderRepository : IPurchaseOrderRepository
{
    private readonly PurchaseOrder? _purchaseOrder;
    private readonly PagedResult<PurchaseOrder>? _pagedResult;
    private readonly CallOrder? _callOrder;

    public FakePurchaseOrderRepository(
        PurchaseOrder? purchaseOrder = null,
        PagedResult<PurchaseOrder>? pagedResult = null,
        CallOrder? callOrder = null)
    {
        _purchaseOrder = purchaseOrder;
        _pagedResult = pagedResult;
        _callOrder = callOrder;
    }

    // --- Interaction recording ---

    public int GetByIdAsyncCallCount { get; private set; }

    public List<int> GetByIdAsyncRequests { get; } = new();

    public int? LastGetByIdAsyncRequestId =>
        GetByIdAsyncRequests.Count > 0 ? GetByIdAsyncRequests[^1] : null;

    public int AddAsyncCallCount { get; private set; }

    public List<PurchaseOrder> AddedPurchaseOrders { get; } = new();

    public PurchaseOrder? LastAddedPurchaseOrder =>
        AddedPurchaseOrders.Count > 0 ? AddedPurchaseOrders[^1] : null;

    public int GetPagedAsyncCallCount { get; private set; }

    public List<GetPagedAsyncCall> GetPagedAsyncCalls { get; } = new();

    public GetPagedAsyncCall? LastGetPagedAsyncCall =>
        GetPagedAsyncCalls.Count > 0 ? GetPagedAsyncCalls[^1] : null;

    // --- IRepository<PurchaseOrder> / IPurchaseOrderRepository ---

    public Task<PurchaseOrder?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        GetByIdAsyncCallCount++;
        GetByIdAsyncRequests.Add(id);
        _callOrder?.Record("PurchaseOrderRepository.GetByIdAsync");

        return Task.FromResult(_purchaseOrder);
    }

    public Task<IReadOnlyList<PurchaseOrder>> GetAllAsync(
        CancellationToken cancellationToken = default)
        => throw new NotSupportedException(
            "FakePurchaseOrderRepository does not support GetAllAsync: no accepted Sprint 13 handler test exercises this member. Extend the fake deliberately if that changes.");

    public Task<IReadOnlyList<PurchaseOrder>> FindAsync(
        Expression<Func<PurchaseOrder, bool>> predicate,
        CancellationToken cancellationToken = default)
        => throw new NotSupportedException(
            "FakePurchaseOrderRepository does not support FindAsync: no accepted Sprint 13 handler test exercises this member. Extend the fake deliberately if that changes.");

    public Task AddAsync(
        PurchaseOrder entity,
        CancellationToken cancellationToken = default)
    {
        AddAsyncCallCount++;
        AddedPurchaseOrders.Add(entity);
        _callOrder?.Record("PurchaseOrderRepository.AddAsync");

        return Task.CompletedTask;
    }

    public void Update(PurchaseOrder entity)
        => throw new NotSupportedException(
            "FakePurchaseOrderRepository does not support Update: no accepted Sprint 13 handler test exercises this member. Extend the fake deliberately if that changes.");

    public void Remove(PurchaseOrder entity)
        => throw new NotSupportedException(
            "FakePurchaseOrderRepository does not support Remove: no accepted Sprint 13 handler test exercises this member. Extend the fake deliberately if that changes.");

    public Task<bool> ExistsAsync(
        Expression<Func<PurchaseOrder, bool>> predicate,
        CancellationToken cancellationToken = default)
        => throw new NotSupportedException(
            "FakePurchaseOrderRepository does not support ExistsAsync: no accepted Sprint 13 handler test exercises this member. Extend the fake deliberately if that changes.");

    public Task<PagedResult<PurchaseOrder>> GetPagedAsync(
        PagedQuery query,
        DateOnly? fromDate = null,
        DateOnly? toDate = null,
        PurchaseOrderStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        GetPagedAsyncCallCount++;
        GetPagedAsyncCalls.Add(
            new GetPagedAsyncCall(query, fromDate, toDate, status));
        _callOrder?.Record("PurchaseOrderRepository.GetPagedAsync");

        return Task.FromResult(
            _pagedResult ?? EmptyPage(query));
    }

    private static PagedResult<PurchaseOrder> EmptyPage(PagedQuery query)
        => new()
        {
            Items = [],
            Page = query.PageNum,
            PageSize = query.PageSize,
            TotalCount = 0
        };
}

/// <summary>
/// Recorded arguments of one <c>GetPagedAsync</c> call, for pass-through assertions (T05).
/// </summary>
public sealed record GetPagedAsyncCall(
    PagedQuery Query,
    DateOnly? FromDate,
    DateOnly? ToDate,
    PurchaseOrderStatus? Status);
