# Current State:

Status:

- Functional Prototype
- Active Development

---

# Current Development Focus:

## Phase 4: Vaccination Tracker and Baby Profile Page (concluded)

- Baby Profile Page: shipped. Read-only profile screen with age, weight, height, and last feed/sleep. It also shows the read-only vaccine catalog with a disclaimer, and each vaccine's status (a pill plus a next-dose or gentle overdue line).
- Vaccine Catalog: shipped. Hardcoded CDC-based vaccine list served through IVaccineCatalog.
- Vaccination Records: shipped. Per-baby vaccination progress (doses given, last and next dose dates), owned by the IVaccineService cache service.
- AddVaccineRecordPage: shipped. Single editor for vaccination records, opened from a vaccine on the profile.
- Vaccine status display: shipped. Each vaccine on the profile shows a status pill and a next-dose or gentle overdue line.
- Clean-up: done. Catalog descriptions are final, task-card edit and delete are icon buttons, and deleting a task or appointment asks first.
- Due soon tag: shipped (#81).
- Next, in order: #82 Recurring vaccines (closes #80), #71 CI mobile targets, #72 Mobile UI/UX polish, #76 EF Core migrations.

See ROADMAP.md for the full Phase 4 breakdown.

---
