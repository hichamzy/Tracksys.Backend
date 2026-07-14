-- citizen.complaint_categories.default_prio ne correspondait pas au nom snake_case attendu
-- par EFCore.NamingConventions pour ComplaintCategory.DefaultPriority (default_priority) —
-- gap révélé par la première requête EF sur ICitizenUnitOfWork.ComplaintCategories (Phase 2).
ALTER TABLE citizen.complaint_categories RENAME COLUMN default_prio TO default_priority;
