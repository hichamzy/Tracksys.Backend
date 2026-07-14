namespace Tracksys.Modules.Citizen.Application.Dtos;

public record ComplaintCategoryDto(
    int Id,
    string Label,
    string? Icon,
    string DefaultPriority,
    int SlaHours,
    bool IsActive);
