using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Shared.Paging;
using InventoryPlatform.Shared.Results;
namespace InventoryPlatform.Application.Features.Customers.GetCustomers;

public sealed class GetCustomersHandler
{
    private readonly ICustomerRepository _customerRepository;

    public GetCustomersHandler(
        ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<Result<PagedResult<GetCustomersResponse>>> HandleAsync(
        GetCustomersRequest request,
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

        var customers = await _customerRepository.GetPagedAsync(
            query,
            cancellationToken);

        var response = new PagedResult<GetCustomersResponse>
        {
            Items = customers.Items
                .Select(c => new GetCustomersResponse
                {
                    Id = c.Id,
                    Name = c.Name,
                    ContactPerson = c.ContactPerson,
                    Email = c.Email,
                    Phone = c.Phone,
                    Address = c.Address,
                    IsActive = c.IsActive
                }
                    )
                .ToList(),

            Page = customers.Page,
            PageSize = customers.PageSize,
            TotalCount = customers.TotalCount
        };

        return Result<PagedResult<GetCustomersResponse>>.Success(response);
    }
}