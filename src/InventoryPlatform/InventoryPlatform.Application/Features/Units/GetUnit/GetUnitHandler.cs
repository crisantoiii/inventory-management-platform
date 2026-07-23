using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Shared.Results;

namespace InventoryPlatform.Application.Features.Units.GetUnit;

public sealed class GetUnitHandler
{
    private readonly IUnitRepository _categoryRepository;

    public GetUnitHandler(
        IUnitRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<Result<GetUnitResponse>> HandleAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByIdAsync(
            id,
            cancellationToken);

        if (category is null)
            return Result<GetUnitResponse>.Failure(
            UnitErrors.NotFound);

        return Result<GetUnitResponse>.Success(new GetUnitResponse(
                                                    category.Id,
                                                    category.Code,
                                                    category.Name,
                                                    category.Symbol,
                                                    category.IsActive));
    }
}