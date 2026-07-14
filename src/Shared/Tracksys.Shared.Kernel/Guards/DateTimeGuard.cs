namespace Tracksys.Shared.Kernel.Guards;

/// <summary>
/// Npgsql exige DateTimeKind.Utc pour les colonnes timestamptz — un DateTime avec
/// Kind=Unspecified ou Local lève une exception au runtime, jamais à la compilation.
/// À appeler à la frontière de chaque entité qui reçoit un DateTime "*Utc" en paramètre.
/// </summary>
public static class DateTimeGuard
{
    public static DateTime EnsureUtc(DateTime value, string paramName)
    {
        if (value.Kind != DateTimeKind.Utc)
            throw new ArgumentException($"'{paramName}' doit être en DateTimeKind.Utc (reçu : {value.Kind}).", paramName);
        return value;
    }
}
