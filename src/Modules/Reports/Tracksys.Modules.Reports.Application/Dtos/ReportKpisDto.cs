namespace Tracksys.Modules.Reports.Application.Dtos;

// KPIs "tournées effectuées" et "délai moyen d'intervention" volontairement omis :
// aucun concept de trajet/intervention n'existe côté backend à ce jour (décision actée).
public record ReportKpisDto(
    decimal FleetDistanceTodayKm,
    int ResolvedComplaintsCount,
    int TotalComplaintsCount,
    int AlertsCount);
