using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Shared.Results;
namespace InventoryPlatform.Application.Features.Suppliers.DeactivateSupplier;

public sealed class DeactivateSupplierHandler
{
    private readonly ISupplierRepository _supplierRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateSupplierHandler(
        ISupplierRepository supplierRepository,
        IUnitOfWork unitOfWork)
    {
        _supplierRepository = supplierRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<DeactivateSupplierResponse>> HandleAsync(
        DeactivateSupplierRequest request,
        CancellationToken cancellationToken = default)
    {
        var supplier = await _supplierRepository.GetByIdAsync(request.Id, cancellationToken);

        if (supplier is null)
        {
            return Result<DeactivateSupplierResponse>.Failure(
                SupplierErrors.NotFound);
        }

        supplier.Deactivate();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<DeactivateSupplierResponse>.Success(
            new DeactivateSupplierResponse(
                supplier.Id,
                supplier.Name,
                supplier.IsActive));
    }
}