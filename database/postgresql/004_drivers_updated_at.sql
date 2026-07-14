-- fleet.drivers manquait updated_at_utc, présent sur toutes les autres tables AuditableEntity
-- (vehicles, chariots, complaints, alert_rules...) — gap de schéma initial, révélé par la
-- première requête EF sur IFleetUnitOfWork.Drivers (Phase 1 du chantier front).
ALTER TABLE fleet.drivers ADD COLUMN updated_at_utc TIMESTAMPTZ NULL;
