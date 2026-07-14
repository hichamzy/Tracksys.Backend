namespace Tracksys.Modules.Fleet.Application.Dtos;

public record CreateDriverRequest(string FullName, string? Phone, string? LicenceNumber);
