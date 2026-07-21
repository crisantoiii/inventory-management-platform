using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Domain.Entities;
using InventoryPlatform.Shared.Paging;

public interface ICategoryRepository
    : IRepository<Category>
{
    Task<PagedResult<Category>> GetPagedAsync(
        PagedQuery request,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByNameAsync(
        string name,
        CancellationToken cancellationToken = default);
}