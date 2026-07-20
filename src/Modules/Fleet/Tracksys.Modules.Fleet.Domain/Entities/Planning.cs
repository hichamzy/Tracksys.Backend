using Tracksys.Shared.Kernel.Entities;
using Tracksys.Shared.Kernel.Guards;

namespace Tracksys.Modules.Fleet.Domain.Entities;

/// <summary>
/// Planning de prestation d'un chariot sur un circuit, pour une période donnée.
/// Résolu à l'écriture de la télémétrie (chariot_id + device_ts BETWEEN debut AND fin)
/// et figé dans la ligne de télémétrie — ne jamais réécrire l'historique après coup.
/// </summary>
public class Planning : Entity<long>, IAggregateRoot
{
    public int ChariotId { get; private set; }
    public int? CircuitId { get; private set; }
    public int? TypePrestationId { get; private set; }
    public DateTime DebutUtc { get; private set; }
    public DateTime FinUtc { get; private set; }
    public Guid CityId { get; private set; }

    private Planning() { }

    public static Planning Create(Guid cityId, int chariotId, int? circuitId, int? typePrestationId, DateTime debutUtc, DateTime finUtc)
    {
        debutUtc = DateTimeGuard.EnsureUtc(debutUtc, nameof(debutUtc));
        finUtc = DateTimeGuard.EnsureUtc(finUtc, nameof(finUtc));
        if (finUtc <= debutUtc)
            throw new ArgumentException("FinUtc doit être postérieur à DebutUtc.");

        return new Planning
        {
            CityId = cityId,
            ChariotId = chariotId,
            CircuitId = circuitId,
            TypePrestationId = typePrestationId,
            DebutUtc = debutUtc,
            FinUtc = finUtc,
        };
    }
}
