using InventoryPlatform.Shared.Paging;

namespace InventoryPlatform.Application.Features.Reporting.GetInventoryMovement;

public sealed record GetInventoryMovementRequest(
    PagedQuery Query,
    DateOnly? FromDate,
    DateOnly? ToDate);
