namespace Tracksys.Modules.Fleet.Domain.Enums;

public enum VehicleStatus
{
    Idle,
    Active,
    Off,
}

public static class VehicleStatusExtensions
{
    public static string ToCode(this VehicleStatus status) => status switch
    {
        VehicleStatus.Active => "active",
        VehicleStatus.Idle => "idle",
        VehicleStatus.Off => "off",
        _ => throw new ArgumentOutOfRangeException(nameof(status)),
    };

    public static VehicleStatus FromCode(string code) => code switch
    {
        "active" => VehicleStatus.Active,
        "idle" => VehicleStatus.Idle,
        "off" => VehicleStatus.Off,
        _ => throw new ArgumentOutOfRangeException(nameof(code)),
    };
}
