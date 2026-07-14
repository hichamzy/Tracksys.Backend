/* =====================================================================
   TRACKSYS — Télémétrie GPS (schéma ingestion), PostgreSQL + PostGIS

   Base unique : le lookup des colonnes enrichies (chariot_id, planning_id,
   circuit_id, delegataire_id, type_prestation_id, chariot_numero,
   boitier_id) se fait par JOINTURE SQL DIRECTE vers fleet.vehicles /
   fleet.chariots / fleet.plannings, dans LA MÊME transaction que l'INSERT
   de télémétrie. Aucune table ref_* dupliquée, aucun hosted service de
   synchronisation : ç'aurait été une resynchronisation de la base avec
   elle-même, sans utilité en base unique.

   Un `ident` ne doit matcher qu'UNE seule entité (vehicle OU chariot).
   Si les deux matchent, c'est une anomalie de configuration à logger
   (raison 'ident_ambigu') — voir le mapper applicatif (module Ingestion).

   Deux scénarios selon l'hébergement cible (pas encore tranché) :
     A. TimescaleDB édition TSL disponible (auto-hébergé, Docker,
        timescale/timescaledb-ha) → hypertable + compression native.
     B. TimescaleDB Apache-2 seul ou absent (ex. Azure Database for
        PostgreSQL Flexible Server, historiquement Apache-2 uniquement)
        → partitionnement natif PostgreSQL RANGE (device_ts) mensuel,
        purge via pg_partman ou job manuel.

   PostGIS est obligatoire dans les deux cas (colonne `position` GEOGRAPHY).

   Exécuter UNIQUEMENT la section correspondant à l'hébergement retenu
   (section 2A ou section 2B), jamais les deux. Le choix se fait au
   provisioning — voir database/postgresql/003_provisioning.md.
   ===================================================================== */

CREATE EXTENSION IF NOT EXISTS postgis;

CREATE SCHEMA IF NOT EXISTS ingestion;

/* =====================================================================
   1. LOOKUP À L'INGESTION — jointure directe, exemple de requête
   Ce bloc est un GABARIT documentant la requête que le module .NET
   Ingestion exécutera (dans la même transaction que l'INSERT), pas du
   DDL à exécuter tel quel. Un `ident` ne doit matcher que fleet.vehicles
   OU fleet.chariots, jamais les deux.

   SELECT
       v.id            AS vehicle_id,
       c.id            AS chariot_id,
       c.numero        AS chariot_numero,
       c.boitier_id,
       c.delegataire_id,
       p.id            AS planning_id,
       p.circuit_id,
       p.type_prestation_id
   FROM (VALUES (:ident)) AS src(ident)
   LEFT JOIN fleet.vehicles  v ON v.flespi_ident = src.ident
   LEFT JOIN fleet.chariots  c ON c.flespi_ident = src.ident
   LEFT JOIN fleet.plannings p ON p.chariot_id = c.id
                               AND :device_ts BETWEEN p.debut_utc AND p.fin_utc;

   Si v.id ET c.id sont tous deux non NULL → anomalie 'ident_ambigu', ne
   pas assigner les colonnes enrichies (les deux à NULL), point conservé.
   ===================================================================== */

/* =====================================================================
   2A. SCÉNARIO A — TimescaleDB TSL (hypertable + compression native)
   Décommenter et exécuter UNIQUEMENT si `timescaledb` (édition TSL) est
   installée : SELECT extversion FROM pg_extension WHERE extname = 'timescaledb';
   et que la licence n'est pas 'apache' (SHOW timescaledb.license;).
   ===================================================================== */

-- CREATE EXTENSION IF NOT EXISTS timescaledb;
--
-- CREATE TABLE ingestion.telemetry (
--     ident                   TEXT NOT NULL,
--     device_ts               TIMESTAMPTZ NOT NULL,
--     server_ts               TIMESTAMPTZ NOT NULL,
--     position                GEOGRAPHY(POINT, 4326) NOT NULL,
--     position_speed          REAL,
--     battery_level           REAL,
--     battery_voltage         REAL,
--     is_powerbank_connected  BOOLEAN,
--     chariot_id              INT,
--     chariot_numero          TEXT,
--     boitier_id              INT,
--     delegataire_id          INT,
--     planning_id             BIGINT,
--     circuit_id              INT,
--     type_prestation_id      INT,
--     CONSTRAINT uq_telemetry_ident_ts UNIQUE (ident, device_ts)
-- );
--
-- SELECT create_hypertable('ingestion.telemetry', by_range('device_ts'));
--
-- CREATE INDEX ix_telemetry_ident_ts ON ingestion.telemetry (ident, device_ts DESC);
-- CREATE INDEX ix_telemetry_chariot_ts ON ingestion.telemetry (chariot_id, device_ts DESC);
-- CREATE INDEX ix_telemetry_position ON ingestion.telemetry USING GIST (position);
--
-- ALTER TABLE ingestion.telemetry SET (
--     timescaledb.compress,
--     timescaledb.compress_segmentby = 'ident',
--     timescaledb.compress_orderby = 'device_ts DESC'
-- );
-- SELECT add_compression_policy('ingestion.telemetry', INTERVAL '7 days');
-- SELECT add_retention_policy('ingestion.telemetry', INTERVAL '90 days'); -- rétention par défaut, ajustable

/* =====================================================================
   2B. SCÉNARIO B — Partitionnement natif PostgreSQL (fallback Apache-2 / Azure Flexible Server)
   Partitions mensuelles créées par pg_partman (extension à activer si
   disponible côté hébergeur) ou par un job de maintenance manuel/CRON
   qui exécute CREATE TABLE ... PARTITION OF ... à l'avance.
   ===================================================================== */

CREATE TABLE ingestion.telemetry (
    ident                   TEXT NOT NULL,
    device_ts               TIMESTAMPTZ NOT NULL,
    server_ts               TIMESTAMPTZ NOT NULL,
    position                GEOGRAPHY(POINT, 4326) NOT NULL,
    position_speed          REAL,
    battery_level           REAL,
    battery_voltage         REAL,
    is_powerbank_connected  BOOLEAN,
    chariot_id              INT,
    chariot_numero          TEXT,
    boitier_id              INT,
    delegataire_id          INT,
    planning_id             BIGINT,
    circuit_id              INT,
    type_prestation_id      INT,
    CONSTRAINT uq_telemetry_ident_ts UNIQUE (ident, device_ts)
) PARTITION BY RANGE (device_ts);

-- Partition par défaut pour absorber tout ce qui tombe hors des partitions
-- mensuelles créées à l'avance (garde-fou, à surveiller — ne doit jamais
-- grossir en usage normal si le job de création de partitions tourne).
CREATE TABLE ingestion.telemetry_default PARTITION OF ingestion.telemetry DEFAULT;

-- Exemple de partition mensuelle (à générer à l'avance, un mois +1 minimum,
-- par pg_partman ou un job planifié — gabarit ci-dessous pour le mois courant) :
-- CREATE TABLE ingestion.telemetry_2026_07 PARTITION OF ingestion.telemetry
--     FOR VALUES FROM ('2026-07-01') TO ('2026-08-01');

CREATE INDEX ix_telemetry_ident_ts ON ingestion.telemetry (ident, device_ts DESC);
CREATE INDEX ix_telemetry_chariot_ts ON ingestion.telemetry (chariot_id, device_ts DESC);
CREATE INDEX ix_telemetry_position ON ingestion.telemetry USING GIST (position);

-- Si pg_partman est disponible côté hébergeur, l'activer et configurer ainsi :
-- CREATE EXTENSION IF NOT EXISTS pg_partman;
-- SELECT partman.create_parent(
--     p_parent_table => 'ingestion.telemetry',
--     p_control => 'device_ts',
--     p_interval => 'monthly',
--     p_premake => 2
-- );
-- UPDATE partman.part_config SET retention = '90 days', retention_keep_table = false
--     WHERE parent_table = 'ingestion.telemetry';
-- Sans pg_partman : prévoir un job applicatif/CRON qui (1) crée les partitions
-- futures à l'avance, (2) DROP les partitions plus vieilles que la rétention
-- configurée (défaut 90 jours) — pas de compression native disponible dans ce
-- scénario (limitation assumée du fallback Apache-2).

/* =====================================================================
   3. LAST_POSITION — une ligne par device, lue par la carte live
   Jamais l'hypertable/table partitionnée : cette table est le SEUL accès
   en lecture pour l'affichage temps réel.
   ===================================================================== */

CREATE TABLE ingestion.last_position (
    ident                   TEXT PRIMARY KEY,
    device_ts               TIMESTAMPTZ NOT NULL,
    server_ts               TIMESTAMPTZ NOT NULL,
    position                GEOGRAPHY(POINT, 4326) NOT NULL,
    position_speed          REAL,
    battery_level           REAL,
    battery_voltage         REAL,
    is_powerbank_connected  BOOLEAN,
    chariot_id              INT,
    chariot_numero          TEXT,
    boitier_id              INT,
    delegataire_id          INT,
    planning_id             BIGINT,
    circuit_id              INT,
    type_prestation_id      INT
);
CREATE INDEX ix_last_position_position ON ingestion.last_position USING GIST (position);
CREATE INDEX ix_last_position_chariot ON ingestion.last_position (chariot_id);

/* =====================================================================
   4. ANOMALIES D'INGESTION
   Raisons attendues : 'position_invalide', 'timestamp_aberrant',
   'device_inconnu', 'planning_introuvable', 'ident_ambigu' (ident matche
   à la fois un vehicle et un chariot — erreur de configuration).
   ===================================================================== */

CREATE TABLE ingestion.ingest_anomaly (
    id           BIGINT GENERATED ALWAYS AS IDENTITY,
    time         TIMESTAMPTZ NOT NULL DEFAULT (timezone('utc', now())),
    ident        TEXT,
    raison       TEXT NOT NULL,
    payload_brut JSONB NOT NULL,
    CONSTRAINT pk_ingest_anomaly PRIMARY KEY (id)
);
CREATE INDEX ix_ingest_anomaly_time ON ingestion.ingest_anomaly (time DESC);
CREATE INDEX ix_ingest_anomaly_ident ON ingestion.ingest_anomaly (ident);
CREATE INDEX ix_ingest_anomaly_raison ON ingestion.ingest_anomaly (raison);
