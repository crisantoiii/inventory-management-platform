using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Domain.Entities;
using InventoryPlatform.Shared.Paging;

public interface ISupplierRepository
    : IRepository<Supplier>
{
    Task<PagedResult<Supplier>> GetPagedAsync(
        PagedQuery request,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByNameAsync(
        int excludingSupplierId,
        string name,
    CancellationToken cancellationToken = default);

    Task<bool> ExistsByNameAsync(
        string name,
        CancellationToken cancellationToken = default);
}