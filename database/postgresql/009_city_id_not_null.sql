/* =====================================================================
   TRACKSYS — Verrouillage city_id en NOT NULL

   À exécuter séparément, APRÈS déploiement du backend filtrant par ville
   (HasQueryFilter, claim JWT city_id) et vérification qu'aucune ligne
   orpheline ne subsiste :

     SELECT count(*) FROM fleet.vehicles WHERE city_id IS NULL;
     -- idem pour chaque table ci-dessous — chaque SELECT doit retourner 0

   identity.asp_net_users.city_id N'EST PAS inclus ici : reste nullable
   en base pour permettre le compte SuperAdmin (city_id IS NULL = "toutes
   villes"), contrainte appliquée côté application.
   ===================================================================== */

ALTER TABLE fleet.vehicles         ALTER COLUMN city_id SET NOT NULL;
ALTER TABLE fleet.drivers          ALTER COLUMN city_id SET NOT NULL;
ALTER TABLE fleet.chariots         ALTER COLUMN city_id SET NOT NULL;
ALTER TABLE fleet.plannings        ALTER COLUMN city_id SET NOT NULL;
ALTER TABLE fleet.delegataires     ALTER COLUMN city_id SET NOT NULL;
ALTER TABLE fleet.circuits         ALTER COLUMN city_id SET NOT NULL;
ALTER TABLE fleet.types_prestation ALTER COLUMN city_id SET NOT NULL;

ALTER TABLE citizen.complaints ALTER COLUMN city_id SET NOT NULL;

ALTER TABLE alerting.alerts      ALTER COLUMN city_id SET NOT NULL;
ALTER TABLE alerting.alert_rules ALTER COLUMN city_id SET NOT NULL;

ALTER TABLE reporting.saved_reports       ALTER COLUMN city_id SET NOT NULL;
ALTER TABLE reporting.fleet_monthly_stats ALTER COLUMN city_id SET NOT NULL;
