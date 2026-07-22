namespace InventoryPlatform.Application.Features.Customers.GetCustomer;

public sealed record GetCustomerResponse(
    int Id,
    string Name,
    string? ContactPerson,
    string? Email,
    string? Phone,
    string? Address,
    bool IsActive);