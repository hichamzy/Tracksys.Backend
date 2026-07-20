/* =====================================================================
   TRACKSYS — Ajout de city_id sur les tables scopées par ville

   Colonnes nullable dans un premier temps : toutes les lignes existantes
   sont rattachées à la ville DEFAULT créée par 007_tenancy_cities.sql,
   NOT NULL appliqué séparément par 009_city_id_not_null.sql une fois le
   backend redéployé et vérifié (voir ce script pour la raison du délai).

   Référentiels volontairement NON scopés (restent globaux, partagés par
   toutes les villes) : fleet.vehicle_types, citizen.complaint_categories,
   reporting.report_types — nomenclatures génériques, pas de donnée.

   identity.asp_net_users.city_id reste nullable en base même après 009 :
   un compte SuperAdmin n'appartient à aucune ville (sentinel "toutes
   villes"), la contrainte "NOT NULL sauf SuperAdmin" est appliquée côté
   application, pas en base.

   UUID de la ville DEFAULT écrit en dur (pas de variable \set) pour rester
   exécutable depuis n'importe quel client SQL, pas seulement psql.
   ===================================================================== */

-- identity
ALTER TABLE identity.asp_net_users ADD COLUMN city_id UUID NULL REFERENCES tenancy.cities (id);
UPDATE identity.asp_net_users SET city_id = '00000000-0000-0000-0000-000000000001' WHERE city_id IS NULL;
CREATE INDEX ix_asp_net_users_city_id ON identity.asp_net_users (city_id);

-- fleet
ALTER TABLE fleet.vehicles ADD COLUMN city_id UUID NULL REFERENCES tenancy.cities (id);
UPDATE fleet.vehicles SET city_id = '00000000-0000-0000-0000-000000000001' WHERE city_id IS NULL;
CREATE INDEX ix_vehicles_city_id ON fleet.vehicles (city_id);

ALTER TABLE fleet.drivers ADD COLUMN city_id UUID NULL REFERENCES tenancy.cities (id);
UPDATE fleet.drivers SET city_id = '00000000-0000-0000-0000-000000000001' WHERE city_id IS NULL;
CREATE INDEX ix_drivers_city_id ON fleet.drivers (city_id);

ALTER TABLE fleet.chariots ADD COLUMN city_id UUID NULL REFERENCES tenancy.cities (id);
UPDATE fleet.chariots SET city_id = '00000000-0000-0000-0000-000000000001' WHERE city_id IS NULL;
CREATE INDEX ix_chariots_city_id ON fleet.chariots (city_id);

ALTER TABLE fleet.plannings ADD COLUMN city_id UUID NULL REFERENCES tenancy.cities (id);
UPDATE fleet.plannings SET city_id = '00000000-0000-0000-0000-000000000001' WHERE city_id IS NULL;
CREATE INDEX ix_plannings_city_id ON fleet.plannings (city_id);

ALTER TABLE fleet.delegataires ADD COLUMN city_id UUID NULL REFERENCES tenancy.cities (id);
UPDATE fleet.delegataires SET city_id = '00000000-0000-0000-0000-000000000001' WHERE city_id IS NULL;
CREATE INDEX ix_delegataires_city_id ON fleet.delegataires (city_id);

ALTER TABLE fleet.circuits ADD COLUMN city_id UUID NULL REFERENCES tenancy.cities (id);
UPDATE fleet.circuits SET city_id = '00000000-0000-0000-0000-000000000001' WHERE city_id IS NULL;
CREATE INDEX ix_circuits_city_id ON fleet.circuits (city_id);

ALTER TABLE fleet.types_prestation ADD COLUMN city_id UUID NULL REFERENCES tenancy.cities (id);
UPDATE fleet.types_prestation SET city_id = '00000000-0000-0000-0000-000000000001' WHERE city_id IS NULL;
CREATE INDEX ix_types_prestation_city_id ON fleet.types_prestation (city_id);

-- citizen
ALTER TABLE citizen.complaints ADD COLUMN city_id UUID NULL REFERENCES tenancy.cities (id);
UPDATE citizen.complaints SET city_id = '00000000-0000-0000-0000-000000000001' WHERE city_id IS NULL;
CREATE INDEX ix_complaints_city_id ON citizen.complaints (city_id);

-- alerting
ALTER TABLE alerting.alerts ADD COLUMN city_id UUID NULL REFERENCES tenancy.cities (id);
UPDATE alerting.alerts SET city_id = '00000000-0000-0000-0000-000000000001' WHERE city_id IS NULL;
CREATE INDEX ix_alerts_city_id ON alerting.alerts (city_id);

ALTER TABLE alerting.alert_rules ADD COLUMN city_id UUID NULL REFERENCES tenancy.cities (id);
UPDATE alerting.alert_rules SET city_id = '00000000-0000-0000-0000-000000000001' WHERE city_id IS NULL;
CREATE INDEX ix_alert_rules_city_id ON alerting.alert_rules (city_id);

-- Une règle par type ET par ville désormais (au lieu d'une règle par type globalement)
ALTER TABLE alerting.alert_rules DROP CONSTRAINT uq_alert_rules_alert_type_code;
ALTER TABLE alerting.alert_rules ADD CONSTRAINT uq_alert_rules_city_alert_type UNIQUE (city_id, alert_type_code);

-- reporting
ALTER TABLE reporting.saved_reports ADD COLUMN city_id UUID NULL REFERENCES tenancy.cities (id);
UPDATE reporting.saved_reports SET city_id = '00000000-0000-0000-0000-000000000001' WHERE city_id IS NULL;
CREATE INDEX ix_saved_reports_city_id ON reporting.saved_reports (city_id);

ALTER TABLE reporting.fleet_monthly_stats ADD COLUMN city_id UUID NULL REFERENCES tenancy.cities (id);
UPDATE reporting.fleet_monthly_stats SET city_id = '00000000-0000-0000-0000-000000000001' WHERE city_id IS NULL;
CREATE INDEX ix_fleet_monthly_stats_city_id ON reporting.fleet_monthly_stats (city_id);

-- Un agrégat par mois ET par ville désormais (au lieu d'un agrégat par mois globalement)
ALTER TABLE reporting.fleet_monthly_stats DROP CONSTRAINT uq_fleet_monthly_stats_year_month;
ALTER TABLE reporting.fleet_monthly_stats ADD CONSTRAINT uq_fleet_monthly_stats_city_year_month UNIQUE (city_id, year_month);
