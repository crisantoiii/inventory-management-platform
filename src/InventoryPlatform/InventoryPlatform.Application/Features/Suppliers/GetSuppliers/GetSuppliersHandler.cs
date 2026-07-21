using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Shared.Paging;
using InventoryPlatform.Shared.Results;
namespace InventoryPlatform.Application.Features.Suppliers.GetSuppliers;

public sealed class GetSuppliersHandler
{
    private readonly ISupplierRepository _supplierRepository;

    public GetSuppliersHandler(
        ISupplierRepository supplierRepository)
    {
        _supplierRepository = supplierRepository;
    }

    public async Task<Result<PagedResult<GetSuppliersResponse>>> HandleAsync(
        GetSuppliersRequest request,
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

        var suppliers = await _supplierRepository.GetPagedAsync(
            query,
            cancellationToken);

        var response = new PagedResult<GetSuppliersResponse>
        {
            Items = suppliers.Items
                .Select(supplier => new GetSuppliersResponse
                {
                    Id = supplier.Id,
                    Name = supplier.Name,
                    ContactPerson = supplier.ContactPerson,
                    Email = supplier.Email,
                    Phone = supplier.Phone,
                    Address = supplier.Address,
                    IsActive = supplier.IsActive
                }
                    )
                .ToList(),

            Page = suppliers.Page,
            PageSize = suppliers.PageSize,
            TotalCount = suppliers.TotalCount
        };

        return Result<PagedResult<GetSuppliersResponse>>.Success(response);
    }
}