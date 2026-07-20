/* =====================================================================
   TRACKSYS — Modules activables par ville (droits d'accès)

   Chaque ville a une liste de modules activés (identifiants de vue déjà
   utilisés côté front : dash, fleet, hist, alerts, report, cit, settings).
   Un module non activé pour une ville rend son controller backend associé
   inaccessible (403) — voir RequireModuleAttribute — et masque l'item
   correspondant dans la sidebar. Le SuperAdmin n'est jamais concerné par
   ce filtre (accès total à tous les modules, toutes villes).

   Toute ville nouvellement créée (CityCommandService.CreateAsync) a ses 7
   modules activés par défaut à la création — ce script seed les mêmes 7
   modules pour les villes déjà existantes avant l'introduction de cette
   fonctionnalité, pour éviter qu'elles se retrouvent sans aucun module
   accessible après déploiement du RequireModuleAttribute.
   ===================================================================== */

-- Clé technique (id) plutôt qu'une PK composite (city_id, module_code) : reste
-- compatible avec Entity<TId>/Repository<TEntity,TId> génériques déjà utilisés
-- partout dans le projet. L'unicité fonctionnelle est garantie par l'index ci-dessous.
CREATE TABLE tenancy.city_modules (
    id          UUID NOT NULL DEFAULT gen_random_uuid(),
    city_id     UUID NOT NULL REFERENCES tenancy.cities (id) ON DELETE CASCADE,
    module_code VARCHAR(20) NOT NULL,
    CONSTRAINT pk_city_modules PRIMARY KEY (id)
);
CREATE UNIQUE INDEX ux_city_modules_city_module ON tenancy.city_modules (city_id, module_code);

INSERT INTO tenancy.city_modules (city_id, module_code)
SELECT c.id, m.code
FROM tenancy.cities c
CROSS JOIN (VALUES ('dash'), ('fleet'), ('hist'), ('alerts'), ('report'), ('cit'), ('settings')) AS m(code)
ON CONFLICT (city_id, module_code) DO NOTHING;

GRANT SELECT, INSERT, UPDATE, DELETE ON tenancy.city_modules TO tracksys_app;
