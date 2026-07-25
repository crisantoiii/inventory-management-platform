using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Shared.Paging;
using InventoryPlatform.Shared.Results;
namespace InventoryPlatform.Application.Features.InventoryTransactions.GetInventoryTransactions;

public sealed class GetInventoryTransactionsHandler
{
    private readonly IInventoryTransactionRepository _inventoryTransactionRepository;

    public GetInventoryTransactionsHandler(
        IInventoryTransactionRepository inventoryTransactionRepository)
    {
        _inventoryTransactionRepository = inventoryTransactionRepository;
    }

    public async Task<Result<PagedResult<GetInventoryTransactionsResponse>>> HandleAsync(
        GetInventoryTransactionsRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = new PagedQuery
        {
            Search = request.Search,
            Page = request.Page,
            PageSize = request.PageSize,
            SortBy = request.SortBy,
            Descending = request.Descending,
            Status = request.Status
        };

        var inventoryTransactions = await _inventoryTransactionRepository.GetPagedAsync(
            query,
            cancellationToken);

        var response = new PagedResult<GetInventoryTransactionsResponse>
        {
            Items = inventoryTransactions.Items
                .Select(inventoryTransaction => new GetInventoryTransactionsResponse
                {
                    Id = inventoryTransaction.Id,
                    ProductId = inventoryTransaction.ProductId,
                    ProductName = inventoryTransaction.Product.Name,
                    Sku = inventoryTransaction.Product.Sku,
                    TransactionType = inventoryTransaction.TransactionType,
                    Quantity = inventoryTransaction.Quantity,
                    ReferenceNumber = inventoryTransaction.ReferenceNumber,
                    Remarks = inventoryTransaction.Remarks,
                    TransactionDateUtc = inventoryTransaction.TransactionDateUtc
                }
                    )
                .ToList(),

            Page = inventoryTransactions.Page,
            PageSize = inventoryTransactions.PageSize,
            TotalCount = inventoryTransactions.TotalCount
        };

        return Result<PagedResult<GetInventoryTransactionsResponse>>.Success(response);
    }
}