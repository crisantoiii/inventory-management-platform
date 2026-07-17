using Microsoft.EntityFrameworkCore;

namespace InventoryPlatform.Infrastructure.Persistence.Context;

public sealed class ApplicationDbContext
    : DbContext
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
}