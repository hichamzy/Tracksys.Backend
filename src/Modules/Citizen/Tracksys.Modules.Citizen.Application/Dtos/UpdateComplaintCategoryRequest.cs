namespace Tracksys.Modules.Citizen.Application.Dtos;

public record UpdateComplaintCategoryRequest(string Label, string? Icon, string DefaultPriority, int SlaHours);
