namespace Tracksys.Modules.Citizen.Application.Dtos;

public record CreateComplaintCategoryRequest(string Label, string? Icon, string DefaultPriority, int SlaHours);
