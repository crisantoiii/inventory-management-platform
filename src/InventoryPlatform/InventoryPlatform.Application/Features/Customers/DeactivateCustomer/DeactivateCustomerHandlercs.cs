using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Shared.Results;
namespace InventoryPlatform.Application.Features.Customers.DeactivateCustomer;

public sealed class DeactivateCustomerHandler
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateCustomerHandler(
        ICustomerRepository customerRepository,
        IUnitOfWork unitOfWork)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<DeactivateCustomerResponse>> HandleAsync(
        DeactivateCustomerRequest request,
        CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdAsync(request.Id, cancellationToken);

        if (customer is null)
        {
            return Result<DeactivateCustomerResponse>.Failure(
                CustomerErrors.NotFound);
        }

        customer.Deactivate();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<DeactivateCustomerResponse>.Success(
            new DeactivateCustomerResponse(
                customer.Id,
                customer.Name,
                customer.IsActive));
    }
}