using Tracksys.Modules.Fleet.Application.Abstractions;
using Tracksys.Modules.Fleet.Application.Dtos;
using Tracksys.Modules.Fleet.Domain.Entities;
using Tracksys.Modules.Fleet.Domain.Enums;
using Tracksys.Shared.Kernel.Auth;
using Tracksys.Shared.Kernel.Results;

namespace Tracksys.Modules.Fleet.Application.Services;

public class VehicleCommandService(IFleetUnitOfWork unitOfWork, ICurrentTenantAccessor tenant)
{
    public async Task<Result<int>> CreateAsync(CreateVehicleRequest request, CancellationToken cancellationToken = default)
    {
        if (tenant.CityId is not Guid cityId)
            return Result.Failure<int>("Aucune ville associée à l'utilisateur courant.");

        if (await unitOfWork.Vehicles.AnyAsync(v => v.Code == request.Code, cancellationToken))
            return Result.Failure<int>($"Un véhicule avec le code '{request.Code}' existe déjà.");

        Vehicle vehicle = Vehicle.Create(
            cityId, request.Code, request.PlateNumber, request.VehicleTypeId, request.Zone,
            request.ImeiTracker, request.FlespiIdent);

        await unitOfWork.Vehicles.AddAsync(vehicle, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(vehicle.Id);
    }

    public async Task<Result> ChangeStatusAsync(int vehicleId, string statusCode, CancellationToken cancellationToken = default)
    {
        Vehicle? vehicle = await unitOfWork.Vehicles.GetByIdAsync(vehicleId, cancellationToken);
        if (vehicle is null) return Result.Failure("Véhicule introuvable.");

        VehicleStatus status;
        try
        {
            status = VehicleStatusExtensions.FromCode(statusCode);
        }
        catch (ArgumentOutOfRangeException)
        {
            return Result.Failure($"Statut '{statusCode}' invalide.");
        }

        vehicle.ChangeStatus(status);
        unitOfWork.Vehicles.Update(vehicle);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> AssignDriverAsync(int vehicleId, int? driverId, CancellationToken cancellationToken = default)
    {
        Vehicle? vehicle = await unitOfWork.Vehicles.GetByIdAsync(vehicleId, cancellationToken);
        if (vehicle is null) return Result.Failure("Véhicule introuvable.");

        if (driverId is null)
        {
            vehicle.UnassignDriver();
        }
        else
        {
            Driver? driver = await unitOfWork.Drivers.GetByIdAsync(driverId.Value, cancellationToken);
            if (driver is null) return Result.Failure("Chauffeur introuvable.");
            vehicle.AssignDriver(driverId.Value);
        }

        unitOfWork.Vehicles.Update(vehicle);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
