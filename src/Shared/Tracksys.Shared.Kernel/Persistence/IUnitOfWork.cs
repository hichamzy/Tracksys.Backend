namespace Tracksys.Shared.Kernel.Persistence;

/// <summary>
/// Unité de travail par module : chaque module (Fleet, Citizen, Alerting, Identity)
/// possède son propre DbContext/IUnitOfWork, mappé sur son propre schéma SQL Server.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    Task<IDisposable> BeginTransactionAsync(CancellationToken cancellationToken = default);
}
