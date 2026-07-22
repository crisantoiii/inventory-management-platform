using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Shared.Results;
namespace InventoryPlatform.Application.Features.Customers.UpdateCustomer;

public sealed class UpdateCustomerHandler
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCustomerHandler(
        ICustomerRepository customerRepository,
        IUnitOfWork unitOfWork)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<UpdateCustomerResponse>> HandleAsync(
        UpdateCustomerRequest request,
        CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (customer is null)
        {
            return Result<UpdateCustomerResponse>.Failure(
                        CustomerErrors.NotFound);
        }

        if (await _customerRepository.ExistsByNameAsync(
            request.Id,
            request.Name,
            cancellationToken))
        {
            return Result<UpdateCustomerResponse>.Failure(
                CustomerErrors.DuplicateName);
        }

        customer.Update(request.Name, 
            request.ContactPerson,
            request.Email,
            request.Phone,
            request.Address);

        _customerRepository.Update(customer);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<UpdateCustomerResponse>.Success(
            new UpdateCustomerResponse(
                customer.Id,
                customer.Name));
    }
}