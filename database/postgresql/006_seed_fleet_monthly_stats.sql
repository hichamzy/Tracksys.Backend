-- Complète reporting.fleet_monthly_stats sur 12 mois (seul 2026-06 existait, seedé par
-- 999_seed_dev_data.sql) pour donner un vrai historique aux séries du module Reports (Phase 5).
INSERT INTO reporting.fleet_monthly_stats (year_month, total_distance_km, resolution_rate_pct, tours_completed, complaints_handled, avg_response_minutes)
VALUES
    ('2025-07', 8200, 71, 210, 140, 55),
    ('2025-08', 8900, 74, 225, 148, 52),
    ('2025-09', 9400, 72, 231, 151, 53),
    ('2025-10', 9100, 78, 228, 160, 48),
    ('2025-11', 8700, 80, 219, 165, 45),
    ('2025-12', 7600, 76, 198, 158, 47),
    ('2026-01', 8300, 79, 214, 170, 44),
    ('2026-02', 9800, 83, 248, 182, 42),
    ('2026-03', 10200, 85, 256, 188, 40),
    ('2026-04', 10900, 84, 268, 191, 41),
    ('2026-05', 11100, 86, 279, 195, 40)
ON CONFLICT (year_month) DO NOTHING;
