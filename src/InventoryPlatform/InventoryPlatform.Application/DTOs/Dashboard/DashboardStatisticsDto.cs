using System;
using System.Collections.Generic;
using System.Text;

namespace InventoryPlatform.Application.DTOs.Dashboard;

public sealed class DashboardStatisticsDto
{
    public int TotalProducts { get; init; }

    public int ActiveProducts { get; init; }

    public int InactiveProducts { get; init; }

    public int LowStockProducts { get; init; }

    public int OutOfStockProducts { get; init; }

    public decimal InventoryValue { get; init; }
}
