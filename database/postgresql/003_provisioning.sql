/* =====================================================================
   TRACKSYS — Provisioning : rôle applicatif à droits restreints

   Le backend (.NET, EF Core + Npgsql brut pour l'ingestion) ne doit
   JAMAIS se connecter avec le rôle superutilisateur. Ce script crée un
   rôle dédié, propriétaire d'aucun schéma, avec uniquement les droits
   nécessaires à l'exécution (pas de CREATEDB, pas de CREATEROLE, pas de
   SUPERUSER).

   Pré-requis : exécuté par un rôle superutilisateur (ou propriétaire de
   la base), après 001_tracksys_schema.sql et 002_telemetry.sql (les
   schémas et tables doivent déjà exister pour que les GRANT ciblés
   fonctionnent).

   Mot de passe : fourni via `psql -v tracksys_app_password=...` pour un
   provisioning contrôlé (voir 003_provisioning.md). Si absent (ex.
   exécution automatique par docker-entrypoint-initdb.d, qui ne passe
   aucune variable), `\set ... \if :{?...}` ci-dessous génère un mot de
   passe aléatoire de secours via gen_random_uuid() — JAMAIS un mot de
   passe fixe en dur. En dev local avec ce filet de sécurité, retrouver
   le mot de passe généré via :
     docker compose exec postgres psql -U postgres -d tracksys -c \
       "ALTER ROLE tracksys_app PASSWORD 'nouveau_mot_de_passe_connu';"
   (readable dans les logs du conteneur au premier démarrage — voir NOTICE ci-dessous).
   ===================================================================== */

\if :{?tracksys_app_password}
\else
    \set tracksys_app_password `echo`
    SELECT gen_random_uuid()::text AS generated_password \gset
    \set tracksys_app_password :generated_password
    \echo 'NOTICE: tracksys_app_password non fourni — mot de passe aléatoire généré (voir ALTER ROLE ci-dessus pour le réinitialiser à une valeur connue).'
\endif

SELECT format('CREATE ROLE tracksys_app LOGIN PASSWORD %L', :'tracksys_app_password')
WHERE NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'tracksys_app') \gexec

-- Connexion à la base + usage des schémas applicatifs
GRANT CONNECT ON DATABASE tracksys TO tracksys_app;
GRANT USAGE ON SCHEMA identity, fleet, citizen, alerting, reporting, ingestion TO tracksys_app;

-- CRUD complet sur les tables existantes de chaque schéma (pas de DDL : pas de
-- CREATE/ALTER/DROP TABLE — les migrations restent un acte manuel/CI, jamais
-- exécuté avec les identifiants applicatifs).
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA identity, fleet, citizen, alerting, reporting, ingestion TO tracksys_app;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA identity, fleet, citizen, alerting, reporting, ingestion TO tracksys_app;

-- Droits par défaut pour les objets créés APRÈS ce script (migrations futures
-- exécutées par un rôle propriétaire/superutilisateur) : tracksys_app doit
-- pouvoir lire/écrire les nouvelles tables sans re-GRANT manuel à chaque migration.
ALTER DEFAULT PRIVILEGES IN SCHEMA identity, fleet, citizen, alerting, reporting, ingestion
    GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO tracksys_app;
ALTER DEFAULT PRIVILEGES IN SCHEMA identity, fleet, citizen, alerting, reporting, ingestion
    GRANT USAGE, SELECT ON SEQUENCES TO tracksys_app;
