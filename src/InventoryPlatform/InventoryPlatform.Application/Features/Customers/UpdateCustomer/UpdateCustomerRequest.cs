namespace InventoryPlatform.Application.Features.Customers.UpdateCustomer;

public sealed record UpdateCustomerRequest(
    int Id,
    string Name,
    string? ContactPerson,
    string? Email,
    string? Phone,
    string? Address);