using System;
using System.Collections.Generic;
using System.Text;

namespace InventoryPlatform.Application.DTOs.Dashboard;

public sealed class LowStockProductDto
{
    public int Id { get; init; }

    public string ProductName { get; init; } = string.Empty;

    public string CategoryName { get; init; } = string.Empty;

    public int QuantityOnHand { get; init; }
}