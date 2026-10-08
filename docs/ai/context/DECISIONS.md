# Current Architectural Decisions

### ADR-001

TaskListService remains the single source of truth for Tasks and Appointments.
BabyProfileService remains the single source of truth for Baby Profiles.
VaccineService remains the single source of truth for Vaccination Records.

Reason: 
- Keeps domain model simple and updating the UI is easier. 
- TaskListService is the only writer that can initiate add, update, or remove transactions for tasks and appointments.
- BabyProfileService is the only writer that can initiate add, update, or remove transactions for baby profiles.
- VaccineService is the only writer that can initiate add, update, or remove transactions for vaccination records.
- This reduces the risk of inconsistencies in the application state, specially as data persistance arrives.

Data Flow:
Page -> TaskListService / BabyProfileService / VaccineService (in-memory cache) -> ITrackerDbService (currently implemented by EfTrackerDbService over EF Core + SQLite)

Status: Accepted, current.

### ADR-002

EF Core + SQLite implementation is active through `EfTrackerDbService`.

Implementation notes:
- Database lifecycle uses `EnsureCreated` (no EF migrations yet).
- While the app is not live, schema changes may reset the local database.
- Debug-only `ResetDatabaseOnStartup` in `Core/Persistence/EfTrackerDbService.cs` exists for one-run schema resets.
- Persistence operations use one short-lived `TrackerContext` per call through `IDbContextFactory<TrackerContext>`.

Status: Accepted, current.

### ADR-003

Scheduler uses SchedulerAppointment projection layer.

Reason:
- Appointment mapping integration without middle layer was unreliable.

Status: Accepted, permanent.

### ADR-004

AddTaskPage is the single adding/editing experience for Tasks and Appointments.
AddBabyPage is the single adding/editing experience for Baby Profiles.
AddVaccineRecordPage is the single adding/editing experience for Vaccination Records.

AddTaskPage can be accessed from:
- ChecklistPage (Add / Edit)
- CalendarPage (Add / Edit Appointment)

AddBabyPage can be accessed from:
- MainPage (Add)
- BabyProfilePage (Edit), reached from the MainPage baby card

AddVaccineRecordPage can be accessed from: 
- BabyProfilePage (Vaccine row / Pencil icon; Add when the vaccine has no record, Edit or Remove when it has one)

Reason:
- Enables consistency and uniformity when creating / editing instances, specially as data persistence arrives.

Status: Accepted, current.

### ADR-005

Proper architecture pattern deferred until application complexity justifies it.

Candidate patterns include:
- MVVM
- Service-based architecture
- Feature-based folder structure

Status: Accepted, current.

### ADR-006

Modular components that are highly reusable are to be developed as Services and used through Interfaces

Examples:
- Filtering (BabyFilterService)
- TaskList Management (TaskListService)
- Baby Profile Management (BabyProfileService)
- Database interactions (ITrackerDbService consuming EfTrackerDbService)
- Vaccine catalog (IVaccineCatalog)

Status: Accepted, current.

### ADR-007

Domain data is located, retrieved, updated, and removed by Id whenever possible, never by object reference.

Reason:
- Guid identifiers exist precisely so a record can be found regardless of which instance a caller happens to hold.
- Reference equality fails silently. A collection entry that has been swapped for a new instance will not match, so the operation becomes a no-op instead of an error.
- Services already replace instances during normal operation.
- Pages hold references across modal and push navigation, which is exactly when those references go stale.
- Reference identity carries no meaning once records round-trip through a database. Id-based access is the pattern that survives persistence.

Examples:
- Service lookups, updates, and removals
- Future database CRUD operations
- Matching UI selections back to domain records

Status: Accepted, permanent.

### ADR-008

`TaskListService`, `BabyProfileService`, and `VaccineService` are cache services for UI state.
`ITrackerDbService` is the persistence contract, and `EfTrackerDbService` is its current implementation.
`IVaccineCatalog` is the fixed reference data contract, and `VaccineCatalog` is its current implementation.
Only cache services should interact with the persistence boundary.

Reason:
- Keeps pages out of storage concerns.
- Write, read, and failure order, and the full data path: see ADR-014
- Keeps persistence isolated behind `ITrackerDbService`, so implementation swaps remain localized.
- Supports one-context-per-operation persistence through `IDbContextFactory<TrackerContext>`.

Status: Accepted, current.

### ADR-009

Mock data is seeded only through TaskListService, only in Debug builds, and only when the task list is empty.

Reason:
- Keeps the app in a known state for development and testing.
- Keeps clear separation between development and production builds and features.
- Mock data still obeys the same rules as real data, so it is a valid test of the app's behavior.
- Data flow is still through the cache services, so the persistence boundary is exercised even with mock data.

Status: Accepted, permanent.

### ADR-010

Secret Rules:
- Secrets are NEVER committed to source control.
- The project's own secrets are compiled in at build time, file is a build input only. The one exception is a user-supplied key (below).
- No Server-side credentials in the client app.

User-supplied keys:
- A key the user enters in the app (for example their AI provider key) belongs to the user, not to the project. It is the one secret that is not compiled in at build time.
- It is stored only in the device's secure storage. It is never written to Preferences, the database, logs or source control.
- It is sent only to the provider it belongs to.
- The user can change or remove it at any time.

Status: Accepted, permanent.

### ADR-011

Database referential integrity is enforced in the database model, and cache services mirror the persisted result.

Reason:
- Deleting a baby profile must not leave orphaned task associations.
- Deleting a baby profile must not leave orphaned vaccination records.
- `TrackerContext` enforces `ON DELETE SET NULL` for `TaskModel.AssociatedBabyId`.
- `TrackerContext` enforces `ON DELETE CASCADE` for `VaccinationRecordModel.BabyId`;
- Services continue using Id-based operations while persisted constraints guarantee data consistency.
- A unique (BabyId, VaccineId) index keeps at most one record per baby per vaccine.

Status: Accepted, permanent.

### ADR-012

The vaccine catalog is fixed reference data and lives outside the database.

Reason:
- Vaccine definitions are not user-owned records.
- The catalog should remain stable and reusable while vaccination records stay per-baby.
- Keeping the catalog outside the database avoids unnecessary churn when local persistence evolves.

Status: Accepted, current.

### ADR-013

ToastService confirms successful user actions, AlertService reports failures that need acknowledgement, and the companion stays out of alert flows.

Reason:
- Positive actions should feel encouraging and lightweight.
- Failures need a blocking, acknowledged response so the user knows the app did not save or load as expected.
- Alert text belongs to recovery and safety, not to the companion persona.
- Cub remains part of positive feedback only.

Status: Accepted, current.

### ADR-014
Data flows through one fixed path. Every layer talks only to the interface of the layer below it.

Write path:
Page (UI) -> ITaskListService / IBabyProfileService / IVaccineService (in-memory cache services) -> ITrackerDbService (EfTrackerDbService) -> TrackerContext -> babybuddy.db3 (SQLite)

Return path:
The database commits -> the call returns up the same path -> the cache service updates its collection -> the UI refreshes through bindings.

Failure path:
EfTrackerDbService throws DbCommunicationException -> the cache is left untouched -> the page tells the user through an alert.

Read path:
Pages read from the cache services only. The database is read once, at startup, to fill the cache.
Fixed reference data (the vaccine catalog, ADR-012) is read directly from its service; it has no cache or database path.

Rules:
- The database is the source of truth. The cache only mirrors what the database has already committed.
- Pages never skip a layer. They never talk to ITrackerDbService, TrackerContext or the database (ADR-001, ADR-008).
- Each database call uses its own short-lived TrackerContext, created through IDbContextFactory (ADR-002).

Reason:
- One write order means no rollback: a failed save leaves nothing to undo.
- The UI can never show data that wasn't stored.
- Constraints enforced by the database (ADR-011) reach the cache as results, not predictions.
- Every layer can be swapped behind its interface without touching the layers above it.

Diagram: docs/DataFlow.excalidraw

Status: Accepted, current.

### ADR-015

Source code is grouped by layer, not by feature. Namespaces follow folders.

- `Core/` holds domain models, cache services, interfaces, the persistence boundary (`TrackerContext`, `EfTrackerDbService`), device storage outside the database (`Core/Storage`: `SecureApiKeyStore`, the user's key in secure storage), collections and exceptions.
- `UI/` holds pages, controls, UI-only services (`ToastService`, `AlertService`, `PaletteService`) and value converters.
- `Remote/` holds off-device implementations only: clients for external services, their DTOs and mapping (namespace `BabyBuddyHelper.Remote`). Their contracts stay in `Core/Interfaces`. Only `MauiProgram` references `Remote/`, for DI registration.
- Namespaces match folders (e.g. `BabyBuddyHelper.Core.Services`, `BabyBuddyHelper.UI.Pages`).
- `App`, `AppShell`, `MauiProgram`, `Platforms/`, `Properties/` and `Resources/` stay at the root, following MAUI conventions.

Reason:
- The flat root folders were slowing navigation, and Phase 4 adds several new models, services and pages.
- Every new file has one obvious home, and its namespace tells you which layer it belongs to.
- Feature-based folders stay deferred (ADR-005) until real feature slices exist.

Status: Accepted, current.

### ADR-016

Detail and read-only pages are pushed onto the navigation stack (`Navigation.PushAsync`).
Add/edit editors open modally (`Navigation.PushModalAsync`).

Reason:
- A profile is a place the user browses and returns from, so it gets the native back arrow.
- Editors are focused tasks that end in save or cancel, so they stay modal.
- BabyProfilePage is the first pushed page (MainPage -> BabyProfilePage). AddBabyPage stays modal on top of it, and deleting a profile pops back to MainPage.

Status: Accepted, current.

### ADR-017

Off-device calls go through one fixed path, separate from the data path (ADR-014).

Path:
Page (UI) -> contract in `Core/Interfaces` -> implementation in `Remote/` -> external provider

Rules:
- The app stays offline-first. Everything that works without a remote call keeps working with no network and no key.
- Nothing leaves the device without a user action, and the user is told what is sent before it is sent.
- Only what the user chose to share for that request is sent.
- Pages never reference `Remote/` or a provider SDK. They talk to the contract.
- Remote implementations never write to the database or the cache services (ADR-001, ADR-008). What comes back is shown, not stored, unless a later ADR says otherwise.
- A failed call throws one exception type and leaves local data untouched. The app says so gently and carries on.
- Keys follow ADR-010.

Reason:
- The app was fully local until Phase 5. A single path keeps it obvious which code can send data off the device.
- A provider can be swapped or added behind the contract without touching the pages.
- Caregivers' data is personal. Explicit, per-request sharing keeps the user in control.

Status: Accepted, current.
