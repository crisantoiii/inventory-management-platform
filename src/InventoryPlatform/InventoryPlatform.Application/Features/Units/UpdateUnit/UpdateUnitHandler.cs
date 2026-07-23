using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Shared.Results;
namespace InventoryPlatform.Application.Features.Units.UpdateUnit;

public sealed class UpdateUnitHandler
{
    private readonly IUnitRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateUnitHandler(
        IUnitRepository categoryRepository,
        IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<UpdateUnitResponse>> HandleAsync(
        UpdateUnitRequest request,
        CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (category is null)
        {
            return Result<UpdateUnitResponse>.Failure(
                        UnitErrors.NotFound);
        }

        category.Update(
            request.Code,
            request.Name, 
            request.Symbol);

        _categoryRepository.Update(category);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<UpdateUnitResponse>.Success(
            new UpdateUnitResponse(
                category.Id,
                category.Name));
    }
}