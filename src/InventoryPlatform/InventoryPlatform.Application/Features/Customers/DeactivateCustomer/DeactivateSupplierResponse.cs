namespace InventoryPlatform.Application.Features.Customers.DeactivateCustomer;

public sealed record DeactivateCustomerResponse(
    int Id,
    string Name,
    bool IsActive);