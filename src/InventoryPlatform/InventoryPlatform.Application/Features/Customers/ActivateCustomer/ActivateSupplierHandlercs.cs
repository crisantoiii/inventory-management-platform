using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Shared.Results;
namespace InventoryPlatform.Application.Features.Customers.ActivateCustomer;

public sealed class ActivateCustomerHandler
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ActivateCustomerHandler(
        ICustomerRepository customerRepository,
        IUnitOfWork unitOfWork)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ActivateCustomerResponse>> HandleAsync(
        ActivateCustomerRequest request,
        CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdAsync(request.Id, cancellationToken);

        if (customer is null)
        {
            return Result<ActivateCustomerResponse>.Failure(
                CustomerErrors.NotFound);
        }

        customer.Activate();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<ActivateCustomerResponse>.Success(
            new ActivateCustomerResponse(
                customer.Id,
                customer.Name,
                customer.IsActive));
    }
}