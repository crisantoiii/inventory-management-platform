using InventoryPlatform.Application.DTOs.Dashboard;
using System;
using System.Collections.Generic;
using System.Text;

namespace InventoryPlatform.Application.Interfaces.Persistence;

public interface IDashboardRepository
{
    Task<DashboardDto> GetDashboardAsync(
        CancellationToken cancellationToken = default);
}
