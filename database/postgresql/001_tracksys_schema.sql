/* =====================================================================
   TRACKSYS — Schéma PostgreSQL (données métier + télémétrie GPS, base unique)
   Backend : .NET 9 / ASP.NET Core Identity + JWT / EF Core (Npgsql)

   Convention de nommage : snake_case partout, sans exception (tables et
   colonnes), y compris les tables ASP.NET Core Identity (asp_net_users,
   etc.). Le backend utilise EFCore.NamingConventions
   (UseSnakeCaseNamingConvention()) pour dériver ces mêmes noms depuis les
   entités C# — ce script DOIT rester en accord strict avec cette
   convention. Le chemin d'ingestion (002_telemetry.sql) écrit en SQL brut
   (Npgsql/COPY) : en snake_case sans guillemets, aucun risque de casse
   contrairement à un identifiant quoté CamelCase.

   Pré-requis : être connecté à la base cible (déjà créée), utilisateur avec
   droits DDL sur le schéma public au minimum (voir 003_provisioning.md pour
   la création d'un rôle applicatif à droits restreints).
   ===================================================================== */

CREATE EXTENSION IF NOT EXISTS postgis;

CREATE SCHEMA IF NOT EXISTS identity;
CREATE SCHEMA IF NOT EXISTS fleet;
CREATE SCHEMA IF NOT EXISTS citizen;
CREATE SCHEMA IF NOT EXISTS alerting;
CREATE SCHEMA IF NOT EXISTS reporting;

/* =====================================================================
   1. ASP.NET CORE IDENTITY (schéma identity) — auth JWT
   Tables au format standard Microsoft.AspNetCore.Identity (IdentityDbContext<ApplicationUser>),
   noms dérivés en snake_case par UseSnakeCaseNamingConvention() côté backend.
   ===================================================================== */

CREATE TABLE identity.asp_net_roles (
    id                VARCHAR(450)  NOT NULL,
    name              VARCHAR(256)  NULL,
    normalized_name   VARCHAR(256)  NULL,
    concurrency_stamp TEXT          NULL,
    CONSTRAINT pk_asp_net_roles PRIMARY KEY (id)
);
CREATE UNIQUE INDEX role_name_index ON identity.asp_net_roles (normalized_name) WHERE normalized_name IS NOT NULL;

CREATE TABLE identity.asp_net_users (
    id                     VARCHAR(450)  NOT NULL,
    full_name              VARCHAR(200)  NOT NULL,               -- extension métier (ex. "A. Tarhine")
    scope                  VARCHAR(200)  NULL,                   -- périmètre affiché, ex. "Anfa · Maârif" (pas de relation pour l'instant)
    is_active              BOOLEAN       NOT NULL DEFAULT TRUE,
    user_name              VARCHAR(256)  NULL,
    normalized_user_name   VARCHAR(256)  NULL,
    email                  VARCHAR(256)  NULL,
    normalized_email       VARCHAR(256)  NULL,
    email_confirmed        BOOLEAN       NOT NULL DEFAULT FALSE,
    password_hash          TEXT          NULL,
    security_stamp         TEXT          NULL,
    concurrency_stamp      TEXT          NULL,
    phone_number           VARCHAR(32)   NULL,
    phone_number_confirmed BOOLEAN       NOT NULL DEFAULT FALSE,
    two_factor_enabled     BOOLEAN       NOT NULL DEFAULT FALSE,
    lockout_end             TIMESTAMPTZ  NULL,
    lockout_enabled         BOOLEAN      NOT NULL DEFAULT TRUE,
    access_failed_count     INT          NOT NULL DEFAULT 0,
    created_at_utc          TIMESTAMPTZ  NOT NULL DEFAULT (timezone('utc', now())),
    CONSTRAINT pk_asp_net_users PRIMARY KEY (id)
);
CREATE UNIQUE INDEX user_name_index ON identity.asp_net_users (normalized_user_name) WHERE normalized_user_name IS NOT NULL;
CREATE INDEX email_index ON identity.asp_net_users (normalized_email);

CREATE TABLE identity.asp_net_user_roles (
    user_id VARCHAR(450) NOT NULL,
    role_id VARCHAR(450) NOT NULL,
    CONSTRAINT pk_asp_net_user_roles PRIMARY KEY (user_id, role_id),
    CONSTRAINT fk_asp_net_user_roles_user FOREIGN KEY (user_id) REFERENCES identity.asp_net_users (id) ON DELETE CASCADE,
    CONSTRAINT fk_asp_net_user_roles_role FOREIGN KEY (role_id) REFERENCES identity.asp_net_roles (id) ON DELETE CASCADE
);

CREATE TABLE identity.asp_net_user_claims (
    id          INT GENERATED ALWAYS AS IDENTITY,
    user_id     VARCHAR(450) NOT NULL,
    claim_type  TEXT NULL,
    claim_value TEXT NULL,
    CONSTRAINT pk_asp_net_user_claims PRIMARY KEY (id),
    CONSTRAINT fk_asp_net_user_claims_user FOREIGN KEY (user_id) REFERENCES identity.asp_net_users (id) ON DELETE CASCADE
);

CREATE TABLE identity.asp_net_role_claims (
    id          INT GENERATED ALWAYS AS IDENTITY,
    role_id     VARCHAR(450) NOT NULL,
    claim_type  TEXT NULL,
    claim_value TEXT NULL,
    CONSTRAINT pk_asp_net_role_claims PRIMARY KEY (id),
    CONSTRAINT fk_asp_net_role_claims_role FOREIGN KEY (role_id) REFERENCES identity.asp_net_roles (id) ON DELETE CASCADE
);

CREATE TABLE identity.asp_net_user_logins (
    login_provider        VARCHAR(450) NOT NULL,
    provider_key           VARCHAR(450) NOT NULL,
    provider_display_name  TEXT NULL,
    user_id                VARCHAR(450) NOT NULL,
    CONSTRAINT pk_asp_net_user_logins PRIMARY KEY (login_provider, provider_key),
    CONSTRAINT fk_asp_net_user_logins_user FOREIGN KEY (user_id) REFERENCES identity.asp_net_users (id) ON DELETE CASCADE
);

CREATE TABLE identity.asp_net_user_tokens (
    user_id        VARCHAR(450) NOT NULL,
    login_provider VARCHAR(450) NOT NULL,
    name           VARCHAR(450) NOT NULL,
    value          TEXT NULL,
    CONSTRAINT pk_asp_net_user_tokens PRIMARY KEY (user_id, login_provider, name),
    CONSTRAINT fk_asp_net_user_tokens_user FOREIGN KEY (user_id) REFERENCES identity.asp_net_users (id) ON DELETE CASCADE
);

-- Refresh tokens JWT (rotation) — table applicative, pas Identity standard
CREATE TABLE identity.refresh_tokens (
    id                     BIGINT GENERATED ALWAYS AS IDENTITY,
    user_id                VARCHAR(450) NOT NULL,
    token_hash             VARCHAR(256) NOT NULL,          -- hash du refresh token (jamais le token en clair)
    expires_at_utc         TIMESTAMPTZ NOT NULL,
    created_at_utc         TIMESTAMPTZ NOT NULL DEFAULT (timezone('utc', now())),
    revoked_at_utc         TIMESTAMPTZ NULL,
    replaced_by_token_hash VARCHAR(256) NULL,
    created_by_ip          VARCHAR(64) NULL,
    CONSTRAINT pk_refresh_tokens PRIMARY KEY (id),
    CONSTRAINT fk_refresh_tokens_user FOREIGN KEY (user_id) REFERENCES identity.asp_net_users (id) ON DELETE CASCADE
);
CREATE INDEX ix_refresh_tokens_user_id ON identity.refresh_tokens (user_id);
CREATE UNIQUE INDEX ux_refresh_tokens_token_hash ON identity.refresh_tokens (token_hash);

/* =====================================================================
   2. RÉFÉRENTIELS FLOTTE (schéma fleet)
   Deux familles d'engins, chacune avec sa propre table de tracker :
     - vehicles   : Teltonika (bennes, ampliroll, laveuse voirie...)
     - chariots   : SinoTrack, opérés par un délégataire, affectés à un
                    planning (circuit + type de prestation + période)
   ===================================================================== */

CREATE TABLE fleet.vehicle_types (
    id    INT GENERATED ALWAYS AS IDENTITY,
    label VARCHAR(100) NOT NULL,          -- "Benne 12 m³", "Ampliroll", "Laveuse voirie"...
    CONSTRAINT pk_vehicle_types PRIMARY KEY (id),
    CONSTRAINT uq_vehicle_types_label UNIQUE (label)
);

CREATE TABLE fleet.vehicle_statuses (
    code  VARCHAR(10) NOT NULL,           -- 'active' | 'idle' | 'off'
    label VARCHAR(50) NOT NULL,           -- "En tournée", "À l'arrêt", "Hors service"
    CONSTRAINT pk_vehicle_statuses PRIMARY KEY (code)
);

CREATE TABLE fleet.drivers (
    id                  INT GENERATED ALWAYS AS IDENTITY,
    full_name           VARCHAR(150) NOT NULL,
    phone               VARCHAR(32) NULL,
    licence_number      VARCHAR(50) NULL,
    licence_valid       BOOLEAN NOT NULL DEFAULT TRUE,
    status              VARCHAR(30) NOT NULL DEFAULT 'En service', -- "En service" | "Repos" | "Absent"
    application_user_id VARCHAR(450) NULL,  -- si le chauffeur a un compte de connexion
    current_vehicle_id  INT NULL,           -- FK ajoutée après création de fleet.vehicles
    created_at_utc      TIMESTAMPTZ NOT NULL DEFAULT (timezone('utc', now())),
    CONSTRAINT pk_drivers PRIMARY KEY (id),
    CONSTRAINT fk_drivers_application_user FOREIGN KEY (application_user_id) REFERENCES identity.asp_net_users (id) ON DELETE SET NULL
);

CREATE TABLE fleet.vehicles (
    id                  INT GENERATED ALWAYS AS IDENTITY,
    code                VARCHAR(20) NOT NULL,       -- "BN-02", identifiant métier affiché
    plate_number        VARCHAR(20) NOT NULL,
    vehicle_type_id     INT NOT NULL,
    driver_id           INT NULL,
    status_code         VARCHAR(10) NOT NULL DEFAULT 'idle',
    zone                VARCHAR(100) NULL,          -- libellé libre, ex. "Anfa" (pas de table zones pour l'instant)
    imei_tracker        VARCHAR(50) NULL,           -- libellé d'affichage libre, ex. "FMC650·8671" — PAS une clé technique
    flespi_ident        VARCHAR(50) NULL,           -- identifiant déclaré par le tracker à Flespi (`ident`) — PAS nécessairement l'IMEI. Clé de corrélation avec la télémétrie GPS.
    speed_kmh           DECIMAL(6,2) NOT NULL DEFAULT 0,
    distance_today_km   DECIMAL(8,2) NOT NULL DEFAULT 0,
    drive_time_today    VARCHAR(20) NULL,           -- "4 h 05" (affichage) — calculable côté service si besoin
    last_stop_label     VARCHAR(20) NULL,           -- "6 min" (affichage)
    last_known_lat      DECIMAL(9,6) NULL,          -- cache de la dernière position connue (source de vérité : ingestion.last_position)
    last_known_lng      DECIMAL(9,6) NULL,
    last_position_at_utc TIMESTAMPTZ NULL,
    created_at_utc      TIMESTAMPTZ NOT NULL DEFAULT (timezone('utc', now())),
    updated_at_utc      TIMESTAMPTZ NULL,
    CONSTRAINT pk_vehicles PRIMARY KEY (id),
    CONSTRAINT uq_vehicles_code UNIQUE (code),
    CONSTRAINT uq_vehicles_plate_number UNIQUE (plate_number),
    CONSTRAINT fk_vehicles_vehicle_type FOREIGN KEY (vehicle_type_id) REFERENCES fleet.vehicle_types (id),
    CONSTRAINT fk_vehicles_driver FOREIGN KEY (driver_id) REFERENCES fleet.drivers (id) ON DELETE SET NULL,
    CONSTRAINT fk_vehicles_status FOREIGN KEY (status_code) REFERENCES fleet.vehicle_statuses (code)
);
CREATE INDEX ix_vehicles_status_code ON fleet.vehicles (status_code);
CREATE INDEX ix_vehicles_driver_id ON fleet.vehicles (driver_id);
-- Index partiel : unicité seulement quand renseigné (plusieurs véhicules peuvent ne pas avoir de balise encore associée)
CREATE UNIQUE INDEX ux_vehicles_flespi_ident ON fleet.vehicles (flespi_ident) WHERE flespi_ident IS NOT NULL;

ALTER TABLE fleet.drivers ADD CONSTRAINT fk_drivers_current_vehicle
    FOREIGN KEY (current_vehicle_id) REFERENCES fleet.vehicles (id) ON DELETE SET NULL;

-- Sociétés délégataires opérant les chariots
CREATE TABLE fleet.delegataires (
    id    INT GENERATED ALWAYS AS IDENTITY,
    label VARCHAR(150) NOT NULL,
    CONSTRAINT pk_delegataires PRIMARY KEY (id),
    CONSTRAINT uq_delegataires_label UNIQUE (label)
);

-- Circuits/zones de collecte parcourus par les chariots
CREATE TABLE fleet.circuits (
    id    INT GENERATED ALWAYS AS IDENTITY,
    label VARCHAR(150) NOT NULL,
    CONSTRAINT pk_circuits PRIMARY KEY (id),
    CONSTRAINT uq_circuits_label UNIQUE (label)
);

-- Types de prestation (référentiel, ex. "Balayage", "Lavage", "Collecte manuelle")
CREATE TABLE fleet.types_prestation (
    id    INT GENERATED ALWAYS AS IDENTITY,
    label VARCHAR(150) NOT NULL,
    CONSTRAINT pk_types_prestation PRIMARY KEY (id),
    CONSTRAINT uq_types_prestation_label UNIQUE (label)
);

-- Chariots (famille SinoTrack) — distincts des vehicles (Teltonika)
CREATE TABLE fleet.chariots (
    id                   INT GENERATED ALWAYS AS IDENTITY,
    numero               VARCHAR(50) NOT NULL,       -- "chariot_numero", identifiant métier affiché
    delegataire_id       INT NULL,
    boitier_id           INT NULL,
    flespi_ident         VARCHAR(50) NULL,           -- identifiant déclaré par le tracker à Flespi (`ident`) — PAS nécessairement l'IMEI
    last_known_lat       DECIMAL(9,6) NULL,
    last_known_lng       DECIMAL(9,6) NULL,
    last_position_at_utc TIMESTAMPTZ NULL,
    created_at_utc       TIMESTAMPTZ NOT NULL DEFAULT (timezone('utc', now())),
    updated_at_utc       TIMESTAMPTZ NULL,
    CONSTRAINT pk_chariots PRIMARY KEY (id),
    CONSTRAINT uq_chariots_numero UNIQUE (numero),
    CONSTRAINT fk_chariots_delegataire FOREIGN KEY (delegataire_id) REFERENCES fleet.delegataires (id) ON DELETE SET NULL
);
CREATE UNIQUE INDEX ux_chariots_flespi_ident ON fleet.chariots (flespi_ident) WHERE flespi_ident IS NOT NULL;

-- Planning de prestation d'un chariot : circuit + type de prestation sur une période.
-- Résolu à l'écriture de la télémétrie (chariot_id + device_ts BETWEEN debut AND fin)
-- et figé dans la ligne de télémétrie — ne jamais réécrire l'historique après coup.
CREATE TABLE fleet.plannings (
    id                   BIGINT GENERATED ALWAYS AS IDENTITY,
    chariot_id           INT NOT NULL,
    circuit_id           INT NULL,
    type_prestation_id   INT NULL,
    debut_utc            TIMESTAMPTZ NOT NULL,
    fin_utc              TIMESTAMPTZ NOT NULL,
    CONSTRAINT pk_plannings PRIMARY KEY (id),
    CONSTRAINT ck_plannings_periode CHECK (fin_utc > debut_utc),
    CONSTRAINT fk_plannings_chariot FOREIGN KEY (chariot_id) REFERENCES fleet.chariots (id) ON DELETE CASCADE,
    CONSTRAINT fk_plannings_circuit FOREIGN KEY (circuit_id) REFERENCES fleet.circuits (id) ON DELETE SET NULL,
    CONSTRAINT fk_plannings_type_prestation FOREIGN KEY (type_prestation_id) REFERENCES fleet.types_prestation (id) ON DELETE SET NULL
);
CREATE INDEX ix_plannings_chariot_periode ON fleet.plannings (chariot_id, debut_utc, fin_utc);

/* =====================================================================
   3. RÉCLAMATIONS CITOYENNES (schéma citizen)
   ===================================================================== */

CREATE TABLE citizen.complaint_categories (
    id           INT GENERATED ALWAYS AS IDENTITY,
    label        VARCHAR(100) NOT NULL,       -- "Dépôt sauvage", "Bac endommagé"...
    icon         VARCHAR(10) NULL,            -- emoji d'illustration
    default_prio VARCHAR(10) NOT NULL,        -- 'Haute' | 'Moyenne' | 'Basse'
    sla_hours    INT NOT NULL,                -- délai cible en heures (4, 24, 12, 72...)
    is_active    BOOLEAN NOT NULL DEFAULT TRUE,
    CONSTRAINT pk_complaint_categories PRIMARY KEY (id),
    CONSTRAINT uq_complaint_categories_label UNIQUE (label),
    CONSTRAINT ck_complaint_categories_prio CHECK (default_prio IN ('Haute','Moyenne','Basse'))
);

CREATE TABLE citizen.complaint_statuses (
    code       VARCHAR(10) NOT NULL,               -- 'received' | 'inprogress' | 'resolved'
    label      VARCHAR(30) NOT NULL,               -- "Reçue", "En cours", "Résolue"
    sort_order SMALLINT NOT NULL,
    CONSTRAINT pk_complaint_statuses PRIMARY KEY (code)
);

CREATE TABLE citizen.complaints (
    id                  INT GENERATED ALWAYS AS IDENTITY,
    code                VARCHAR(20) NOT NULL,        -- "RC-2087"
    category_id         INT NOT NULL,
    priority            VARCHAR(10) NOT NULL,        -- copie modifiable à la création (peut différer du défaut catégorie)
    status_code         VARCHAR(10) NOT NULL DEFAULT 'received',
    zone_label          VARCHAR(150) NOT NULL,        -- "Bd Anfa, Maârif" — adresse affichée
    lat                 DECIMAL(9,6) NOT NULL,
    lng                 DECIMAL(9,6) NOT NULL,
    assigned_vehicle_id INT NULL,
    reporter_name       VARCHAR(100) NULL,            -- "Anonyme" par défaut côté front
    reported_at_utc     TIMESTAMPTZ NOT NULL DEFAULT (timezone('utc', now())),
    resolved_at_utc     TIMESTAMPTZ NULL,
    photo_before_url    VARCHAR(500) NULL,
    photo_after_url     VARCHAR(500) NULL,
    created_at_utc      TIMESTAMPTZ NOT NULL DEFAULT (timezone('utc', now())),
    updated_at_utc      TIMESTAMPTZ NULL,
    CONSTRAINT pk_complaints PRIMARY KEY (id),
    CONSTRAINT uq_complaints_code UNIQUE (code),
    CONSTRAINT ck_complaints_priority CHECK (priority IN ('Haute','Moyenne','Basse')),
    CONSTRAINT fk_complaints_category FOREIGN KEY (category_id) REFERENCES citizen.complaint_categories (id),
    CONSTRAINT fk_complaints_status FOREIGN KEY (status_code) REFERENCES citizen.complaint_statuses (code),
    CONSTRAINT fk_complaints_vehicle FOREIGN KEY (assigned_vehicle_id) REFERENCES fleet.vehicles (id) ON DELETE SET NULL
);
CREATE INDEX ix_complaints_status_code ON citizen.complaints (status_code);
CREATE INDEX ix_complaints_category_id ON citizen.complaints (category_id);
CREATE INDEX ix_complaints_assigned_vehicle_id ON citizen.complaints (assigned_vehicle_id);

-- Timeline de suivi (Reçue → En cours → Résolue), historise chaque changement de statut
CREATE TABLE citizen.complaint_status_history (
    id               BIGINT GENERATED ALWAYS AS IDENTITY,
    complaint_id     INT NOT NULL,
    status_code      VARCHAR(10) NOT NULL,
    changed_at_utc   TIMESTAMPTZ NOT NULL DEFAULT (timezone('utc', now())),
    changed_by_user_id VARCHAR(450) NULL,
    CONSTRAINT pk_complaint_status_history PRIMARY KEY (id),
    CONSTRAINT fk_complaint_status_history_complaint FOREIGN KEY (complaint_id) REFERENCES citizen.complaints (id) ON DELETE CASCADE,
    CONSTRAINT fk_complaint_status_history_status FOREIGN KEY (status_code) REFERENCES citizen.complaint_statuses (code),
    CONSTRAINT fk_complaint_status_history_user FOREIGN KEY (changed_by_user_id) REFERENCES identity.asp_net_users (id) ON DELETE SET NULL
);
CREATE INDEX ix_complaint_status_history_complaint_id ON citizen.complaint_status_history (complaint_id);

/* =====================================================================
   4. ALERTES & RÈGLES (schéma alerting)
   ===================================================================== */

CREATE TABLE alerting.alert_types (
    code     VARCHAR(20) NOT NULL,          -- 'speed' | 'stop' | 'idle' | 'brake' | 'gps' | 'battery' | 'hours' | 'maint'
    label    VARCHAR(100) NOT NULL,         -- "Excès de vitesse"...
    severity VARCHAR(2) NOT NULL,           -- 'hi' | 'md' | 'lo'
    CONSTRAINT pk_alert_types PRIMARY KEY (code),
    CONSTRAINT ck_alert_types_severity CHECK (severity IN ('hi','md','lo'))
);

CREATE TABLE alerting.notification_channels (
    code        VARCHAR(20) NOT NULL,      -- 'app' | 'mail' | 'sms' | 'daily'
    name        VARCHAR(150) NOT NULL,
    description VARCHAR(300) NULL,
    is_enabled  BOOLEAN NOT NULL DEFAULT TRUE,
    CONSTRAINT pk_notification_channels PRIMARY KEY (code)
);

CREATE TABLE alerting.alert_rules (
    id              INT GENERATED ALWAYS AS IDENTITY,
    alert_type_code VARCHAR(20) NOT NULL,
    is_enabled      BOOLEAN NOT NULL DEFAULT TRUE,
    threshold       DECIMAL(10,2) NOT NULL,       -- valeur du seuil (50, 20, 10, 8, 30, 15, 20, 15000...)
    unit            VARCHAR(10) NOT NULL,         -- "km/h", "min", "m/s²", "%", "h", "km"
    description     VARCHAR(300) NULL,
    updated_at_utc  TIMESTAMPTZ NULL,
    CONSTRAINT pk_alert_rules PRIMARY KEY (id),
    CONSTRAINT uq_alert_rules_alert_type_code UNIQUE (alert_type_code),
    CONSTRAINT fk_alert_rules_alert_type FOREIGN KEY (alert_type_code) REFERENCES alerting.alert_types (code)
);

-- Canaux activés par règle (many-to-many alert_rules <-> notification_channels)
CREATE TABLE alerting.alert_rule_channels (
    alert_rule_id INT NOT NULL,
    channel_code  VARCHAR(20) NOT NULL,
    is_enabled    BOOLEAN NOT NULL DEFAULT TRUE,
    CONSTRAINT pk_alert_rule_channels PRIMARY KEY (alert_rule_id, channel_code),
    CONSTRAINT fk_alert_rule_channels_rule FOREIGN KEY (alert_rule_id) REFERENCES alerting.alert_rules (id) ON DELETE CASCADE,
    CONSTRAINT fk_alert_rule_channels_channel FOREIGN KEY (channel_code) REFERENCES alerting.notification_channels (code) ON DELETE CASCADE
);

CREATE TABLE alerting.alerts (
    id              BIGINT GENERATED ALWAYS AS IDENTITY,
    code            VARCHAR(20) NOT NULL,        -- "AL-1042"
    alert_type_code VARCHAR(20) NOT NULL,
    vehicle_id      INT NOT NULL,
    detail_text     VARCHAR(500) NOT NULL,        -- texte complet (le front distingue segments gras côté UI uniquement)
    occurred_at_utc TIMESTAMPTZ NOT NULL DEFAULT (timezone('utc', now())),
    is_unread       BOOLEAN NOT NULL DEFAULT TRUE,
    read_at_utc     TIMESTAMPTZ NULL,
    read_by_user_id VARCHAR(450) NULL,
    CONSTRAINT pk_alerts PRIMARY KEY (id),
    CONSTRAINT uq_alerts_code UNIQUE (code),
    CONSTRAINT fk_alerts_alert_type FOREIGN KEY (alert_type_code) REFERENCES alerting.alert_types (code),
    CONSTRAINT fk_alerts_vehicle FOREIGN KEY (vehicle_id) REFERENCES fleet.vehicles (id),
    CONSTRAINT fk_alerts_read_by_user FOREIGN KEY (read_by_user_id) REFERENCES identity.asp_net_users (id) ON DELETE SET NULL
);
CREATE INDEX ix_alerts_vehicle_id ON alerting.alerts (vehicle_id);
CREATE INDEX ix_alerts_is_unread ON alerting.alerts (is_unread);
CREATE INDEX ix_alerts_occurred_at_utc ON alerting.alerts (occurred_at_utc DESC);

/* =====================================================================
   5. RAPPORTS (schéma reporting)
   ===================================================================== */

CREATE TABLE reporting.report_types (
    id    INT GENERATED ALWAYS AS IDENTITY,
    label VARCHAR(100) NOT NULL,         -- "Activité de la flotte", "Réclamations citoyennes"...
    CONSTRAINT pk_report_types PRIMARY KEY (id),
    CONSTRAINT uq_report_types_label UNIQUE (label)
);

CREATE TABLE reporting.saved_reports (
    id                 INT GENERATED ALWAYS AS IDENTITY,
    report_type_id     INT NOT NULL,
    name               VARCHAR(200) NOT NULL,
    period_label       VARCHAR(50) NOT NULL,       -- "Juin 2026", "T2 2026"...
    format             VARCHAR(10) NOT NULL,       -- 'XLSX' | 'PDF' | 'CSV'
    file_url           VARCHAR(500) NULL,
    generated_at_utc   TIMESTAMPTZ NOT NULL DEFAULT (timezone('utc', now())),
    generated_by_user_id VARCHAR(450) NULL,
    CONSTRAINT pk_saved_reports PRIMARY KEY (id),
    CONSTRAINT ck_saved_reports_format CHECK (format IN ('XLSX','PDF','CSV')),
    CONSTRAINT fk_saved_reports_report_type FOREIGN KEY (report_type_id) REFERENCES reporting.report_types (id),
    CONSTRAINT fk_saved_reports_user FOREIGN KEY (generated_by_user_id) REFERENCES identity.asp_net_users (id) ON DELETE SET NULL
);
CREATE INDEX ix_saved_reports_report_type_id ON reporting.saved_reports (report_type_id);

-- Agrégats mensuels flotte (alimente les graphiques Distance/Résolution du dashboard & rapports)
CREATE TABLE reporting.fleet_monthly_stats (
    id                   INT GENERATED ALWAYS AS IDENTITY,
    year_month           CHAR(7) NOT NULL,   -- format 'YYYY-MM'
    total_distance_km    DECIMAL(10,2) NOT NULL DEFAULT 0,
    resolution_rate_pct  DECIMAL(5,2) NOT NULL DEFAULT 0,
    tours_completed      INT NOT NULL DEFAULT 0,
    complaints_handled   INT NOT NULL DEFAULT 0,
    avg_response_minutes INT NOT NULL DEFAULT 0,
    CONSTRAINT pk_fleet_monthly_stats PRIMARY KEY (id),
    CONSTRAINT uq_fleet_monthly_stats_year_month UNIQUE (year_month)
);

/* =====================================================================
   6. DONNÉES DE RÉFÉRENCE (seed) — valeurs fixes issues du front
   ===================================================================== */

INSERT INTO fleet.vehicle_statuses (code, label) VALUES
    ('active', 'En tournée'),
    ('idle',   'À l''arrêt'),
    ('off',    'Hors service');

INSERT INTO fleet.vehicle_types (label) VALUES
    ('Benne 12 m³'), ('Benne 6 m³'), ('Ampliroll'), ('Laveuse voirie'), ('Véhicule léger');

INSERT INTO citizen.complaint_statuses (code, label, sort_order) VALUES
    ('received',   'Reçue',    1),
    ('inprogress', 'En cours', 2),
    ('resolved',   'Résolue',  3);

INSERT INTO citizen.complaint_categories (label, icon, default_prio, sla_hours, is_active) VALUES
    ('Dépôt sauvage',           '🗑️', 'Haute',   4,  TRUE),
    ('Bac endommagé',           '♻️', 'Moyenne', 24, TRUE),
    ('Collecte manquée',        '🚛', 'Moyenne', 12, TRUE),
    ('Éclairage public',        '💡', 'Basse',   72, TRUE),
    ('Voirie (nid-de-poule)',   '🕳️', 'Basse',   72, FALSE);

INSERT INTO alerting.alert_types (code, label, severity) VALUES
    ('speed',   'Excès de vitesse',         'hi'),
    ('stop',    'Arrêt prolongé',           'md'),
    ('idle',    'Moteur au ralenti',        'md'),
    ('brake',   'Freinage brusque',         'hi'),
    ('gps',     'Perte de signal GPS',      'hi'),
    ('battery', 'Batterie balise faible',   'lo'),
    ('hours',   'Circulation hors horaires','md'),
    ('maint',   'Seuil kilométrique atteint','lo');

INSERT INTO alerting.notification_channels (code, name, description, is_enabled) VALUES
    ('app',   'Notification dans la plateforme', 'Badge et centre d''alertes', TRUE),
    ('mail',  'E-mail au superviseur',            'Alertes critiques uniquement', TRUE),
    ('sms',   'SMS d''astreinte',                  'Alertes critiques hors horaires ouvrés', FALSE),
    ('daily', 'Rapport quotidien d''alertes',       'Synthèse envoyée chaque matin à 08:00', TRUE);

INSERT INTO alerting.alert_rules (alert_type_code, is_enabled, threshold, unit, description) VALUES
    ('speed',   TRUE, 50,    'km/h', 'Déclenche si la vitesse dépasse le seuil pendant plus de 10 s'),
    ('stop',    TRUE, 20,    'min',  'Déclenche si le véhicule reste immobile au-delà du seuil'),
    ('idle',    TRUE, 10,    'min',  'Moteur tournant véhicule à l''arrêt (surconsommation)'),
    ('brake',   TRUE, 8,     'm/s²', 'Décélération supérieure au seuil (conduite à risque)'),
    ('gps',     TRUE, 30,    'min',  'Aucune position reçue de la balise au-delà du seuil'),
    ('battery', TRUE, 15,    '%',    'Niveau de batterie de la balise sous le seuil'),
    ('hours',   TRUE, 20,    'h',    'Circulation détectée en dehors de la plage autorisée'),
    ('maint',   TRUE, 15000, 'km',   'Kilométrage atteint depuis la dernière révision');

INSERT INTO alerting.alert_rule_channels (alert_rule_id, channel_code, is_enabled)
SELECT r.id, c.code, (c.code IN ('app','mail'))
FROM alerting.alert_rules r
CROSS JOIN alerting.notification_channels c
WHERE c.code IN ('app','mail','sms');

INSERT INTO reporting.report_types (label) VALUES
    ('Activité de la flotte'),
    ('Réclamations citoyennes'),
    ('Tournées & trajets'),
    ('KPI communal (synthèse)');

/* =====================================================================
   7. RÔLES APPLICATIFS (seed Identity) — correspond aux rôles vus dans le front
   Les mots de passe/utilisateurs concrets sont créés via le backend (UserManager),
   pas en clair dans ce script. gen_random_uuid() est natif depuis PostgreSQL 13
   (pas besoin de l'extension uuid-ossp).
   ===================================================================== */

INSERT INTO identity.asp_net_roles (id, name, normalized_name, concurrency_stamp) VALUES
    (gen_random_uuid()::text, 'Superviseur',      'SUPERVISEUR',      gen_random_uuid()::text),
    (gen_random_uuid()::text, 'AgentTraitement',  'AGENTTRAITEMENT',  gen_random_uuid()::text),
    (gen_random_uuid()::text, 'ExploitantFlotte', 'EXPLOITANTFLOTTE', gen_random_uuid()::text),
    (gen_random_uuid()::text, 'Administrateur',   'ADMINISTRATEUR',   gen_random_uuid()::text);
