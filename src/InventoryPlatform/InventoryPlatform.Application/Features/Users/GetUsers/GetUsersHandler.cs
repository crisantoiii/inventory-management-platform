using InventoryPlatform.Application.Interfaces.Identity;
using InventoryPlatform.Domain.Entities;
using InventoryPlatform.Shared.Paging;
using InventoryPlatform.Shared.Results;

namespace InventoryPlatform.Application.Features.Users.GetUsers;

public sealed class GetUsersHandler
{
    private readonly IIdentityService _identityService;

    public GetUsersHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<Result<PagedResult<GetUsersResponse>>> HandleAsync(
        GetUsersRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = new PagedQuery
        {
            Search = request.Search,
            Page = request.Page,
            PageSize = request.PageSize,
            SortBy = request.SortBy,
            Descending = request.Descending,
            Status = request.Status
        };

        var users = await _identityService.GetUsersAsync(
            query,
            cancellationToken);

        var response = new PagedResult<GetUsersResponse>
        {
            Items = users.Items,
            Page = users.Page,
            PageSize = users.PageSize,
            TotalCount = users.TotalCount
        };

        return Result<PagedResult<GetUsersResponse>>.Success(response);
    }
}