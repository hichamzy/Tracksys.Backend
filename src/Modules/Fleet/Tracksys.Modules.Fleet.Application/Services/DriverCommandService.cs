using Tracksys.Modules.Fleet.Application.Abstractions;
using Tracksys.Modules.Fleet.Application.Dtos;
using Tracksys.Modules.Fleet.Domain.Entities;
using Tracksys.Shared.Kernel.Results;

namespace Tracksys.Modules.Fleet.Application.Services;

public class DriverCommandService(IFleetUnitOfWork unitOfWork)
{
    public async Task<Result<int>> CreateAsync(CreateDriverRequest request, CancellationToken cancellationToken = default)
    {
        Driver driver = Driver.Create(request.FullName, request.Phone, request.LicenceNumber);

        await unitOfWork.Drivers.AddAsync(driver, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(driver.Id);
    }

    public async Task<Result> ChangeStatusAsync(int driverId, string status, CancellationToken cancellationToken = default)
    {
        Driver? driver = await unitOfWork.Drivers.GetByIdAsync(driverId, cancellationToken);
        if (driver is null) return Result.Failure("Chauffeur introuvable.");

        driver.ChangeStatus(status);
        unitOfWork.Drivers.Update(driver);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> AssignVehicleAsync(int driverId, int? vehicleId, CancellationToken cancellationToken = default)
    {
        Driver? driver = await unitOfWork.Drivers.GetByIdAsync(driverId, cancellationToken);
        if (driver is null) return Result.Failure("Chauffeur introuvable.");

        if (vehicleId is null)
        {
            driver.Unassign();
        }
        else
        {
            Vehicle? vehicle = await unitOfWork.Vehicles.GetByIdAsync(vehicleId.Value, cancellationToken);
            if (vehicle is null) return Result.Failure("Véhicule introuvable.");
            driver.AssignToVehicle(vehicleId.Value);
        }

        unitOfWork.Drivers.Update(driver);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
