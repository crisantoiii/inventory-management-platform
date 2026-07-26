using System;
using System.Collections.Generic;
using System.Text;

namespace InventoryPlatform.Application.DTOs.Dashboard;

public sealed class RecentTransactionDto
{
    public int Id { get; init; }

    public string ProductName { get; init; } = string.Empty;

    public string TransactionType { get; init; } = string.Empty;

    public decimal Quantity { get; init; }

    public DateTime TransactionDate { get; init; }
}
