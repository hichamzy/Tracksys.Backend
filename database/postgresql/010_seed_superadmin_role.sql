/* =====================================================================
   TRACKSYS — Rôle SuperAdmin (multi-tenant)

   Rôle transverse au-dessus des rôles existants (Superviseur,
   AgentTraitement, ExploitantFlotte, Administrateur) : gère les villes
   elles-mêmes (tenancy.cities) et a accès à toutes les villes sans
   filtre (HasQueryFilter contourné via ICurrentTenantAccessor.IsSuperAdmin).

   Bootstrap du premier compte SuperAdmin : script manuel séparé, à
   exécuter une fois après déploiement (voir README ou notes de
   déploiement) — remplacer EMAIL_CIBLE_EN_MAJUSCULES par l'email
   normalisé (majuscules) du compte Administrateur existant à promouvoir :

     INSERT INTO identity.asp_net_user_roles (user_id, role_id)
     SELECT u.id, r.id
     FROM identity.asp_net_users u, identity.asp_net_roles r
     WHERE u.normalized_email = 'EMAIL_CIBLE_EN_MAJUSCULES'
       AND r.normalized_name = 'SUPERADMIN'
     ON CONFLICT DO NOTHING;

     UPDATE identity.asp_net_users SET city_id = NULL
     WHERE normalized_email = 'EMAIL_CIBLE_EN_MAJUSCULES';
   ===================================================================== */

INSERT INTO identity.asp_net_roles (id, name, normalized_name, concurrency_stamp)
VALUES (gen_random_uuid()::text, 'SuperAdmin', 'SUPERADMIN', gen_random_uuid()::text)
ON CONFLICT DO NOTHING;
