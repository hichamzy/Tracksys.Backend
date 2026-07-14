namespace Tracksys.Modules.Citizen.Application.Dtos;

public record ComplaintCategoryBreakdownDto(
    int CategoryId,
    string CategoryLabel,
    int Count,
    decimal Percentage);
