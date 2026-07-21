using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Shared.Results;
namespace InventoryPlatform.Application.Features.Suppliers.ActivateSupplier;

public sealed class ActivateSupplierHandler
{
    private readonly ISupplierRepository _supplierRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ActivateSupplierHandler(
        ISupplierRepository supplierRepository,
        IUnitOfWork unitOfWork)
    {
        _supplierRepository = supplierRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ActivateSupplierResponse>> HandleAsync(
        ActivateSupplierRequest request,
        CancellationToken cancellationToken = default)
    {
        var supplier = await _supplierRepository.GetByIdAsync(request.Id);

        if (supplier is null)
        {
            return Result<ActivateSupplierResponse>.Failure(
                SupplierErrors.NotFound);
        }

        supplier.Activate();

        await _unitOfWork.SaveChangesAsync();

        return Result<ActivateSupplierResponse>.Success(
            new ActivateSupplierResponse(
                supplier.Id,
                supplier.Name,
                supplier.IsActive));
    }
}