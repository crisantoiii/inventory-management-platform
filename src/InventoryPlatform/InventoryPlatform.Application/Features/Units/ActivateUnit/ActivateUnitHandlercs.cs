using InventoryPlatform.Application.Features.Units.ActivateUnit;
using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Shared.Results;
namespace InventoryPlatform.Application.Features.Units.ActivateUnit;

public sealed class ActivateUnitHandler
{
    private readonly IUnitRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ActivateUnitHandler(
        IUnitRepository categoryRepository,
        IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ActivateUnitResponse>> HandleAsync(
        ActivateUnitRequest request,
        CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByIdAsync(request.Id,cancellationToken);

        if (category is null)
        {
            return Result<ActivateUnitResponse>.Failure(
                UnitErrors.NotFound);
        }

        category.Activate();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<ActivateUnitResponse>.Success(
            new ActivateUnitResponse(
                category.Id,
                category.Name,
                category.IsActive));
    }
}