using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Shared.Results;
namespace InventoryPlatform.Application.Features.Suppliers.UpdateSupplier;

public sealed class UpdateSupplierHandler
{
    private readonly ISupplierRepository _supplierRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateSupplierHandler(
        ISupplierRepository supplierRepository,
        IUnitOfWork unitOfWork)
    {
        _supplierRepository = supplierRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<UpdateSupplierResponse>> HandleAsync(
        UpdateSupplierRequest request,
        CancellationToken cancellationToken = default)
    {
        var supplier = await _supplierRepository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (supplier is null)
        {
            return Result<UpdateSupplierResponse>.Failure(
                        SupplierErrors.NotFound);
        }

        if (await _supplierRepository.ExistsByNameAsync(
            request.Id,
            request.Name,
            cancellationToken))
        {
            return Result<UpdateSupplierResponse>.Failure(
                SupplierErrors.DuplicateName);
        }

        supplier.Update(request.Name, 
            request.ContactPerson,
            request.Email,
            request.Phone,
            request.Address);

        _supplierRepository.Update(supplier);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<UpdateSupplierResponse>.Success(
            new UpdateSupplierResponse(
                supplier.Id,
                supplier.Name));
    }
}