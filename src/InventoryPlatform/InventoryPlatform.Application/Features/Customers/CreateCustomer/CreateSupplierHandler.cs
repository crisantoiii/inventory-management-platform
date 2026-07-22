using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Domain.Entities;
using InventoryPlatform.Shared.Results;

namespace InventoryPlatform.Application.Features.Customers.CreateCustomer;

public sealed class CreateCustomerHandler
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCustomerHandler(
        ICustomerRepository customerRepository,
        IUnitOfWork unitOfWork)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CreateCustomerResponse>> HandleAsync(
        CreateCustomerRequest request,
        CancellationToken cancellationToken = default)
    {
        if (await _customerRepository.ExistsByNameAsync(
            request.Name,
            cancellationToken))
        {
            return Result<CreateCustomerResponse>.Failure(
                CustomerErrors.DuplicateName);
        }

        var customer = new Customer(
                        request.Name, 
                        request.ContactPerson,
                        request.Email,
                        request.Phone,
                        request.Address);

        await _customerRepository.AddAsync(customer, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<CreateCustomerResponse>.Success(
            new CreateCustomerResponse(
                customer.Id,
                customer.Name));
    }

}