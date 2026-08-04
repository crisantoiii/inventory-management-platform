using InventoryPlatform.Application.DTOs.Dashboard;
using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Shared.Results;
using System;
using System.Collections.Generic;
using System.Text;

namespace InventoryPlatform.Application.Features.Dashboard.GetDashboard;

public sealed class GetDashboardHandler
{
    private readonly IDashboardRepository _repository;

    public GetDashboardHandler(IDashboardRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<DashboardDto>> HandleAsync(
        GetDashboardRequest query,
        CancellationToken cancellationToken = default)
    {
        var dashboard = await _repository.GetDashboardAsync(cancellationToken);

        return Result<DashboardDto>.Success(dashboard);
    }
}
