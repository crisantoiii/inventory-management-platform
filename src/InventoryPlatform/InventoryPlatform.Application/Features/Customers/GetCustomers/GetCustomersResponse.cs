namespace InventoryPlatform.Application.Features.Customers.GetCustomers;

public sealed record GetCustomersResponse
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string? ContactPerson { get; init; }
    
    public string? Email { get; init; }
    
    public string? Phone { get; init; }

    public string? Address { get; init; }

    public bool IsActive { get; init; }
}