# Provisioning PostgreSQL — TRACKSYS

## Extensions requises

| Extension | Obligatoire ? | Rôle |
|---|---|---|
| `postgis` | **Oui, dans tous les cas** | Type `GEOGRAPHY(POINT,4326)` pour la télémétrie GPS (`fleet.chariots`/`vehicles.last_known_*`, `ingestion.telemetry`/`last_position`) |
| `timescaledb` | Selon hébergement — voir ci-dessous | Hypertable + compression native (scénario 2A de `002_telemetry.sql`) |
| `pg_partman` | Optionnel, si TSL absent | Automatise la création/purge des partitions mensuelles (scénario 2B) |

`gen_random_uuid()` (seed des rôles Identity) est natif depuis PostgreSQL 13 — aucune extension requise pour ça.

## TimescaleDB : TSL vs Apache-2 — à vérifier avant de choisir le scénario 2A ou 2B

```sql
SELECT extversion FROM pg_extension WHERE extname = 'timescaledb';
SHOW timescaledb.license;  -- 'timescale' (TSL, compression/rétention natives) ou 'apache' (hypertables seules, pas de compression)
```

- **Auto-hébergé / Docker / VM** : l'image `timescale/timescaledb-ha` embarque l'édition TSL complète (compression, `add_retention_policy`) → scénario **2A**.
- **Azure Database for PostgreSQL Flexible Server** : historiquement Apache-2 uniquement (pas de compression Timescale native) → scénario **2B** (partitionnement natif + `pg_partman` si disponible côté offre Azure, sinon job de maintenance applicatif).
- **Hébergement pas encore décidé** : les deux scénarios sont écrits et commentés dans `002_telemetry.sql` — décommenter la section correspondante (2A ou 2B) au moment du choix, jamais les deux en même temps.

## Ordre d'exécution des scripts

1. `001_tracksys_schema.sql` — schémas + tables métier (identity/fleet/citizen/alerting/reporting) + seed référentiels
2. `002_telemetry.sql` — schéma `ingestion`, choisir la section 2A (Timescale TSL) ou 2B (partitionnement natif, actif par défaut dans le fichier)
3. `003_provisioning.sql` — rôle applicatif `tracksys_app` à droits restreints (CRUD, pas de DDL, pas de SUPERUSER/CREATEDB/CREATEROLE)

Ces trois fichiers sont numérotés pour être exécutés automatiquement dans cet ordre par `docker-entrypoint-initdb.d` (voir plus bas) ou par n'importe quel outil de migration manuel (`psql -f`).

## Rôle applicatif — pourquoi et comment

Le backend .NET (EF Core + Npgsql brut pour le module Ingestion à venir) se connecte **exclusivement** avec un rôle dédié `tracksys_app`, jamais avec le superutilisateur `postgres`. Ce rôle :
- a uniquement `SELECT/INSERT/UPDATE/DELETE` sur les tables des 6 schémas applicatifs
- n'a **aucun droit DDL** (pas de `CREATE TABLE`/`ALTER TABLE`/`DROP TABLE`) — les migrations de schéma restent un acte manuel ou CI, exécuté avec des identifiants distincts (superutilisateur ou rôle propriétaire), jamais avec les identifiants applicatifs de production
- n'a pas `CREATEDB`, `CREATEROLE`, `SUPERUSER`

`003_provisioning.sql` utilise la méta-commande `psql` `\gexec` pour créer le rôle avec un mot de passe **jamais en dur dans le fichier** — il doit être fourni via une variable `psql` au moment de l'exécution :

```bash
psql -h localhost -U postgres -d tracksys -v tracksys_app_password="$(openssl rand -base64 24)" -f 003_provisioning.sql
```

**Testé** : `docker-entrypoint-initdb.d` ne passe aucune variable `-v` aux scripts qu'il exécute automatiquement. Le script gère ce cas via `\if :{?tracksys_app_password}` : si la variable est absente, un mot de passe aléatoire (`gen_random_uuid()`) est généré à la place d'échouer — un `NOTICE` l'indique dans les logs du conteneur. Voir étape 3 ci-dessous pour fixer ensuite ce mot de passe à une valeur connue.

## Dev local — Docker Compose

`docker-compose.yml` (racine du repo) démarre `timescale/timescaledb-ha:pg16-all` (PostGIS + TimescaleDB TSL inclus) et monte `database/postgresql/*.sql` dans `/docker-entrypoint-initdb.d` — ces scripts s'exécutent automatiquement, dans l'ordre alphabétique, **uniquement au tout premier démarrage** (volume de données vide).

### 1. Créer `.env` à la racine du repo (jamais commité — voir `.gitignore`)

```env
POSTGRES_SUPERUSER_PASSWORD=change_me_dev_only
```

### 2. Démarrer

```bash
docker compose up -d
docker compose logs -f postgres   # vérifier que les 3 scripts s'exécutent sans erreur au premier démarrage
```

### 3. Fixer le mot de passe du rôle applicatif à une valeur connue

Au premier démarrage automatique, `003_provisioning.sql` a créé `tracksys_app` avec un mot de passe aléatoire (non récupérable — voir `\if :{?tracksys_app_password}` dans le script). Le fixer explicitement :

```bash
docker compose exec postgres psql -U postgres -d tracksys \
    -c "ALTER ROLE tracksys_app PASSWORD '$(openssl rand -base64 24 | tee tracksys_app_password.local.txt)';"
```

Conserver le mot de passe généré (`tracksys_app_password.local.txt`, déjà exclu par `.gitignore`) pour construire la connection string du backend :

```
ConnectionStrings:PostgreSql = Host=localhost;Port=5432;Database=tracksys;Username=tracksys_app;Password=<mot de passe généré>
```

À stocker via `dotnet user-secrets` en dev, jamais dans `appsettings.json`.

### 4. Réinitialiser complètement (rejouer les scripts d'init)

```bash
docker compose down -v   # supprime le volume — DESTRUCTIF, dev local uniquement
docker compose up -d
```

## Production / hébergement managé

- Ne pas s'appuyer sur `docker-entrypoint-initdb.d` en production — exécuter les 3 scripts manuellement ou via une étape CI dédiée, avec un rôle superutilisateur/propriétaire temporaire.
- Vérifier le support TimescaleDB TSL de l'offre retenue **avant** d'exécuter `002_telemetry.sql` (voir section TSL vs Apache-2 ci-dessus) — le choix 2A/2B n'est pas réversible sans réécriture de la table `ingestion.telemetry`.
- Générer le mot de passe `tracksys_app` avec un gestionnaire de secrets (Key Vault, variables d'environnement CI protégées) — jamais en clair dans un script versionné ni dans les logs de déploiement.
- La connection string finale va dans la configuration du backend (variables d'environnement ou Key Vault), jamais dans `appsettings.json` commité.
