using InventoryPlatform.Application.Features.Units.ActivateUnit;
using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Shared.Results;
namespace InventoryPlatform.Application.Features.Units.ActivateUnit;

public sealed class ActivateUnitHandler
{
    private readonly IUnitRepository _unitRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ActivateUnitHandler(
        IUnitRepository unitRepository,
        IUnitOfWork unitOfWork)
    {
        _unitRepository = unitRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ActivateUnitResponse>> HandleAsync(
        ActivateUnitRequest request,
        CancellationToken cancellationToken = default)
    {
        var unit = await _unitRepository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (unit is null)
        {
            return Result<ActivateUnitResponse>.Failure(
                UnitErrors.NotFound);
        }

        unit.Activate();

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return Result<ActivateUnitResponse>.Success(
            new ActivateUnitResponse(
                unit.Id,
                unit.Name,
                unit.IsActive));
    }
}