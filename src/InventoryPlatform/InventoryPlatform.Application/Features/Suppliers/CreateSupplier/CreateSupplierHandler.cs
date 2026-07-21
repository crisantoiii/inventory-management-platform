using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Domain.Entities;
using InventoryPlatform.Shared.Results;

namespace InventoryPlatform.Application.Features.Suppliers.CreateSupplier;

public sealed class CreateSupplierHandler
{
    private readonly ISupplierRepository _supplierRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateSupplierHandler(
        ISupplierRepository supplierRepository,
        IUnitOfWork unitOfWork)
    {
        _supplierRepository = supplierRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CreateSupplierResponse>> HandleAsync(
        CreateSupplierRequest request,
        CancellationToken cancellationToken = default)
    {
        if (await _supplierRepository.ExistsByNameAsync(
            request.Name,
            cancellationToken))
        {
            return Result<CreateSupplierResponse>.Failure(
                SupplierErrors.DuplicateName);
        }

        var supplier = new Supplier(
                        request.Name, 
                        request.ContactPerson,
                        request.Email,
                        request.Phone,
                        request.Address);

        await _supplierRepository.AddAsync(supplier, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<CreateSupplierResponse>.Success(
            new CreateSupplierResponse(
                supplier.Id,
                supplier.Name));
    }

}