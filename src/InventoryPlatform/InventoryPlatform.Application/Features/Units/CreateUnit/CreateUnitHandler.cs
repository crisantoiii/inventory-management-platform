using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Domain.Entities;
using InventoryPlatform.Shared.Results;

namespace InventoryPlatform.Application.Features.Units.CreateUnit;

public sealed class CreateUnitHandler
{
    private readonly IUnitRepository _unitRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateUnitHandler(
        IUnitRepository unitRepository,
        IUnitOfWork unitOfWork)
    {
        _unitRepository = unitRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CreateUnitResponse>> HandleAsync(
        CreateUnitRequest request,
        CancellationToken cancellationToken = default)
    {
        if (await _unitRepository.ExistsByNameAsync(
            request.Name,
            cancellationToken))
        {
            return Result<CreateUnitResponse>.Failure(
                UnitErrors.DuplicateName);
        }

        var unit = new Unit(
                        request.Code,
                        request.Name, 
                        request.Symbol);

        await _unitRepository.AddAsync(unit, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<CreateUnitResponse>.Success(
            new CreateUnitResponse(
                unit.Id,
                unit.Name));
    }

}