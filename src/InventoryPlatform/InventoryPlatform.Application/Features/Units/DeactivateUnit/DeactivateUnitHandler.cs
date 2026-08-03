using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Shared.Results;
namespace InventoryPlatform.Application.Features.Units.DeactivateUnit;

public sealed class DeactivateUnitHandler
{
    private readonly IUnitRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateUnitHandler(
        IUnitRepository categoryRepository,
        IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<DeactivateUnitResponse>> HandleAsync(
        DeactivateUnitRequest request,
        CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByIdAsync(request.Id,cancellationToken);

        if (category is null)
        {
            return Result<DeactivateUnitResponse>.Failure(
                UnitErrors.NotFound);
        }

        category.Deactivate();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<DeactivateUnitResponse>.Success(
            new DeactivateUnitResponse(
                category.Id,
                category.Name,
                category.IsActive));
    }
}