using System;
using System.Collections.Generic;
using System.Text;

namespace InventoryPlatform.Application.DTOs.Reporting;

public sealed record InventoryValuationDto(
    int ProductId,
    string ProductName,
    string? CategoryName,
    decimal QuantityOnHand,
    decimal CostPrice,
    decimal InventoryValue);
