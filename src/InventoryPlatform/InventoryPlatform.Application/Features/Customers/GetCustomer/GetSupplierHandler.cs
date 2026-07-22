using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Shared.Results;

namespace InventoryPlatform.Application.Features.Customers.GetCustomer;

public sealed class GetCustomerHandler
{
    private readonly ICustomerRepository _customerRepository;

    public GetCustomerHandler(
        ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<Result<GetCustomerResponse>> HandleAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdAsync(
            id,
            cancellationToken);

        if (customer is null)
            return Result<GetCustomerResponse>.Failure(
            CustomerErrors.NotFound);

        return Result<GetCustomerResponse>.Success(new GetCustomerResponse(
                                                    customer.Id,
                                                    customer.Name,
                                                    customer.ContactPerson,
                                                    customer.Email,
                                                    customer.Phone,
                                                    customer.Address,
                                                    customer.IsActive));
    }
}