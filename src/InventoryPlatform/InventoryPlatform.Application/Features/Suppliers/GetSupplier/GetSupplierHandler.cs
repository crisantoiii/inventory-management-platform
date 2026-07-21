using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Shared.Results;

namespace InventoryPlatform.Application.Features.Suppliers.GetSupplier;

public sealed class GetSupplierHandler
{
    private readonly ISupplierRepository _supplierRepository;

    public GetSupplierHandler(
        ISupplierRepository supplierRepository)
    {
        _supplierRepository = supplierRepository;
    }

    public async Task<Result<GetSupplierResponse>> HandleAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var supplier = await _supplierRepository.GetByIdAsync(
            id,
            cancellationToken);

        if (supplier is null)
            return Result<GetSupplierResponse>.Failure(
            SupplierErrors.NotFound);

        return Result<GetSupplierResponse>.Success(new GetSupplierResponse(
                                                    supplier.Id,
                                                    supplier.Name,
                                                    supplier.ContactPerson,
                                                    supplier.Email,
                                                    supplier.Phone,
                                                    supplier.Address,
                                                    supplier.IsActive));
    }
}