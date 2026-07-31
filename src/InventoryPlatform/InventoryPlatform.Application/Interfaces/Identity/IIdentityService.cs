using InventoryPlatform.Application.DTOs.Role;
using InventoryPlatform.Application.Features.Users.CreateUser;
using InventoryPlatform.Application.Features.Users.GetUser;
using InventoryPlatform.Application.Features.Users.GetUsers;
using InventoryPlatform.Shared.Paging;
using InventoryPlatform.Shared.Results;

namespace InventoryPlatform.Application.Interfaces.Identity;

public interface IIdentityService
{
    Task<IReadOnlyList<RoleOption>> GetRolesAsync(
        CancellationToken cancellationToken = default);

    Task<PagedResult<GetUsersResponse>> GetUsersAsync(
        PagedQuery request,
        CancellationToken cancellationToken = default);

    Task<Result<GetUserResponse>> GetUserAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<Result<Guid>> CreateUserAsync(
        CreateUserRequest request, 
        CancellationToken cancellationToken);
}