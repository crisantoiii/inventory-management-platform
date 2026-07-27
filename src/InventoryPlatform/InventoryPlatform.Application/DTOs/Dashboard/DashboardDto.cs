using System;
using System.Collections.Generic;
using System.Text;

namespace InventoryPlatform.Application.DTOs.Dashboard;

public sealed class DashboardDto
{
    public DashboardStatisticsDto Statistics { get; init; } = new();

    public IReadOnlyList<RecentTransactionDto> RecentTransactions { get; init; }
        = [];

    public IReadOnlyList<LowStockProductDto> LowStockProducts { get; init; }
        = [];
}
