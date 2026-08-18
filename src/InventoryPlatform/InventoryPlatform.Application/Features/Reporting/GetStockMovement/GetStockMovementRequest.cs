using InventoryPlatform.Domain.Enums;
using InventoryPlatform.Shared.Paging;

namespace InventoryPlatform.Application.Features.Reporting.GetStockMovement;

public sealed record GetStockMovementRequest(
    PagedQuery Query,
    DateOnly? FromDate,
    DateOnly? ToDate,
    TransactionType? TransactionType);