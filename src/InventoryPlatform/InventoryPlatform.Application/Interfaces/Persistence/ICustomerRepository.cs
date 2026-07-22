using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Domain.Entities;
using InventoryPlatform.Shared.Paging;

public interface ICustomerRepository
    : IRepository<Customer>
{
    Task<PagedResult<Customer>> GetPagedAsync(
        PagedQuery request,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByNameAsync(
        int excludingCustomerId,
        string name,
    CancellationToken cancellationToken = default);

    Task<bool> ExistsByNameAsync(
        string name,
        CancellationToken cancellationToken = default);
}