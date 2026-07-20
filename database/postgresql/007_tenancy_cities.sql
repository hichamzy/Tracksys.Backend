/* =====================================================================
   TRACKSYS — Multi-tenant par ville (schéma tenancy)

   Introduit la notion de ville (tenant) : chaque ville cliente a ses
   propres véhicules/utilisateurs/réclamations/alertes/rapports. Schéma
   dédié plutôt que identity.cities — la ville est un concept transverse
   référencé depuis identity, fleet, citizen, alerting et reporting, un
   couplage vers identity serait artificiel.

   Une ville "DEFAULT" est créée ici pour servir de point de rattachement
   provisoire aux données déjà existantes (voir 008_add_city_id_columns.sql).
   ===================================================================== */

CREATE SCHEMA IF NOT EXISTS tenancy;

CREATE TABLE tenancy.cities (
    id             UUID NOT NULL DEFAULT gen_random_uuid(),
    name           VARCHAR(150) NOT NULL,
    code           VARCHAR(20)  NOT NULL,          -- identifiant court, ex. "CASA", "RABAT"
    is_active      BOOLEAN NOT NULL DEFAULT TRUE,
    created_at_utc TIMESTAMPTZ NOT NULL DEFAULT (timezone('utc', now())),
    updated_at_utc TIMESTAMPTZ NULL,
    CONSTRAINT pk_cities PRIMARY KEY (id)
);
CREATE UNIQUE INDEX ux_cities_code ON tenancy.cities (code);

-- Ville de bascule pour les données existantes avant l'introduction du multi-tenant
INSERT INTO tenancy.cities (id, name, code, is_active)
VALUES ('00000000-0000-0000-0000-000000000001', 'Ville par défaut (migration)', 'DEFAULT', TRUE);

GRANT USAGE ON SCHEMA tenancy TO tracksys_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA tenancy TO tracksys_app;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA tenancy TO tracksys_app;
ALTER DEFAULT PRIVILEGES IN SCHEMA tenancy GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO tracksys_app;
ALTER DEFAULT PRIVILEGES IN SCHEMA tenancy GRANT USAGE, SELECT ON SEQUENCES TO tracksys_app;
