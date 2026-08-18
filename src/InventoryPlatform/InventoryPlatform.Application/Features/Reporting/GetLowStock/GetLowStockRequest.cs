using InventoryPlatform.Shared.Paging;

namespace InventoryPlatform.Application.Features.Reporting.GetLowStock;

public sealed record GetLowStockRequest(
    PagedQuery Query);
