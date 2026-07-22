using InventoryPlatform.Shared.Results;

namespace InventoryPlatform.Application.Features.Customers;

public static class CustomerErrors
{
    public static readonly Error NotFound =
        new(
            "Customer.NotFound",
            "Customer not found.");

    public static readonly Error DuplicateName =
        new(
            "Customer.DuplicateName",
            "A customer with the same Name already exists.");
}