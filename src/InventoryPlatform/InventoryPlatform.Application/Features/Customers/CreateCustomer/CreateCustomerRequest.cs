namespace InventoryPlatform.Application.Features.Customers.CreateCustomer;

public sealed record CreateCustomerRequest(
    string Name,
    string? ContactPerson,
    string? Email,
    string? Phone,
    string? Address);