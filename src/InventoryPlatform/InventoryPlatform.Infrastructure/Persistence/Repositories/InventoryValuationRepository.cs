using InventoryPlatform.Application.DTOs.Reporting;
using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace InventoryPlatform.Infrastructure.Persistence.Repositories;

public sealed class InventoryValuationRepository : IInventoryValuationRepository
{
    private readonly ApplicationDbContext _context;

    public InventoryValuationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<InventoryValuationDto>> GetInventoryValuationAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .Select(p => new InventoryValuationDto(
                p.Id,
                p.Name,
                p.Category != null ? p.Category.Name : null,
                p.QuantityOnHand,
                p.CostPrice,
                p.QuantityOnHand * p.CostPrice))
            .ToListAsync(cancellationToken);
    }
}