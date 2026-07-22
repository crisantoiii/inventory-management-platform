namespace InventoryPlatform.Application.Features.Customers.ActivateCustomer;

public sealed record ActivateCustomerResponse(
    int Id,
    string Name,
    bool IsActive);