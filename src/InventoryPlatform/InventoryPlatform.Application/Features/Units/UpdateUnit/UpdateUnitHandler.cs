using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Shared.Results;
namespace InventoryPlatform.Application.Features.Units.UpdateUnit;

public sealed class UpdateUnitHandler
{
    private readonly IUnitRepository _unitRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateUnitHandler(
        IUnitRepository unitRepository,
        IUnitOfWork unitOfWork)
    {
        _unitRepository = unitRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<UpdateUnitResponse>> HandleAsync(
        UpdateUnitRequest request,
        CancellationToken cancellationToken = default)
    {
        var unit = await _unitRepository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (unit is null)
        {
            return Result<UpdateUnitResponse>.Failure(
                        UnitErrors.NotFound);
        }

        unit.Update(
            request.Code,
            request.Name, 
            request.Symbol);

        _unitRepository.Update(unit);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<UpdateUnitResponse>.Success(
            new UpdateUnitResponse(
                unit.Id,
                unit.Name));
    }
}