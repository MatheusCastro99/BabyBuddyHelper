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
- A stored key the device cannot read is treated as "no key" and is never deleted by the app; the user removes or replaces it. A read can fail for a passing reason, and deleting a good key would cost the user a new one.

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

- `Core/` holds domain models, cache services, interfaces, the persistence boundary (`TrackerContext`, `EfTrackerDbService`), device storage outside the database (`Core/Storage`: `SecureApiKeyStore`, the user's key in secure storage; `PreferencesCubPersonalityStore`, Cub's style and reply length in Preferences), collections and exceptions.
- `UI/` holds pages, controls, UI-only services (`ToastService`, `AlertService`, `PaletteService`) and value converters.
- `Remote/` holds off-device implementations only: clients for external services, their DTOs and mapping (namespace `BabyBuddyHelper.Remote`). Their contracts stay in `Core/Interfaces`. Only `MauiProgram` references `Remote/`, for DI registration.
- Namespaces match folders (e.g. `BabyBuddyHelper.Core.Services.Cub`, `BabyBuddyHelper.UI.Pages`).
- Inside `Core/Interfaces`, `Core/Models` and `Core/Services`, files sit in feature subfolders (`Baby`, `TaskList`, `Vaccine`, `Cub`); cross-feature files stay at the layer root.
- `App`, `AppShell`, `MauiProgram`, `Platforms/`, `Properties/` and `Resources/` stay at the root, following MAUI conventions.
- `test/` holds the automated tests, outside the app project (ADR-018).

Reason:
- The flat root folders were slowing navigation, and Phase 4 adds several new models, services and pages.
- Every new file has one obvious home, and its namespace tells you which layer it belongs to.
- The layer folders had grown cluttered, so each is split into feature subfolders (#133). The layer stays the top level.
- Feature-based folders stay deferred (ADR-005) until real feature slices exist.

Status: Accepted, current.

### ADR-016

Detail and read-only pages are pushed onto the navigation stack (`Navigation.PushAsync`).
Add/edit editors open modally (`Navigation.PushModalAsync`).

Reason:
- A profile is a place the user browses and returns from, so it gets the native back arrow.
- Editors are focused tasks that end in save or cancel, so they stay modal.
- BabyProfilePage is the first pushed page (MainPage -> BabyProfilePage). AddBabyPage stays modal on top of it, and deleting a profile pops back to MainPage.
- ChatPage is the second pushed page (MainPage -> ChatPage). Its settings open in a popup (`CubSettingsPopup`), not as a page.

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
- Remote implementations never write to the database or the cache services (ADR-001, ADR-008). What comes back is shown, not stored, unless a later ADR says otherwise. Cub's chat replies are shown only after the checks in ADR-020.
- A failed call throws one exception type and leaves local data untouched. The app says so gently and carries on.
- Keys follow ADR-010.

Reason:
- The app was fully local until Phase 5. A single path keeps it obvious which code can send data off the device.
- A provider can be swapped or added behind the contract without touching the pages.
- Caregivers' data is personal. Explicit, per-request sharing keeps the user in control.

Status: Accepted, current.

### ADR-018

Core is covered by automated unit tests. The tests compile the Core/ source files directly and run against a real SQLite database held in memory.

Rules:
- Tests live in `test/BabyBuddyHelper.Tests` (xUnit, plain `net10.0`). The project links `Core/**/*.cs`; it does not reference the app project.
- Tests go through the cache services (`TaskListService`, `BabyProfileService`, `VaccineService`) on top of the real `EfTrackerDbService`, the same path the pages use (ADR-014). The persistence boundary is not mocked.
- Each test owns its database. Tests do not depend on each other or on run order, and never touch `babybuddy.db3`.
- A stored-data test checks the cache and then checks again after a simulated restart.
- Core code that needs a MAUI API is left out of the test project and named there (today: `SecureApiKeyStore` and `PreferencesCubPersonalityStore`).
- A change to Core/ behaviour comes with tests, and a bug fix in Core/ comes with a test that reproduces the bug. An exception is stated in the PR.
- `Remote/` is covered the same way (#96): its sources are linked, and the network is replaced by a fake. No automated test calls a real provider or uses a real key (ADR-010). `UI/` is not covered.
- CI runs the tests in Debug and in Release on every pull request ("xUnit Test Validation").
- The tests add to the maintainer's manual pass; they do not replace it.

Reason:
- The app project targets platform frameworks only, so a plain test project cannot reference it. Linking the sources adds tests without changing the app's structure (ADR-015).
- Testing through the real database exercises the constraints the cache mirrors (ADR-011) instead of a prediction of them.
- Debug and Release differ (`#if DEBUG` paths), so both are run.
- Tests written with the change keep the suite in step with the app; a test for each fixed bug keeps that bug from returning.
- A real provider call costs money, needs the user's key and answers differently each time, so it stays in the manual pass.

Status: Accepted, current.

### ADR-019

Cub's chat replies come from one fixed Claude model, under rules fixed in the app, and Cub sees only what the caregiver chose to share (#96).

Rules:
- The model is fixed to Claude Haiku 5.5. There is no model setting.
- Cub's rules, the baby context and the caps live in `Core` (`CubGuidelines`, `BabyChatContext`). `Remote/` only maps them to the provider's SDK, so they are tested without a provider and stay the same if the provider changes.
- A reply is capped by the reply length the caregiver picks (#97): 250 output tokens for Short, 400 for Balanced (the default) and 700 for Detailed. At most the last 20 turns are sent; the first turn sent is always the caregiver's.
- With "General advice" (the default), no baby data is sent. A picked baby shares its name, age, height and weight (each with its date, "date unknown" or "not recorded") and a vaccine summary. The birth date, the last feed and the last sleep are never sent.
- Every key save shows a consent confirm first, listing what is sent; declining stores nothing.
- A failed reply shows one neutral inline line, not an alert and not in Cub's voice (ADR-013). The failed message is never sent again.
- The chat is never stored. Leaving the page drops a reply still on its way.
- The rules are checked by hand against `CUB_GUIDELINES_CHECKLIST.md` with a real key; the tests check that each rule is in the prompt.

Reason:
- The caregiver pays with their own key, so one small, fast model and a short reply keep the cost low. Since #97 each message also pays for a grade (ADR-020): two calls when the reply passes, up to four when it is retried.
- Rules in `Core` keep ADR-017's path thin: the provider client holds no product decisions.
- Caregivers' data is personal. Sharing only the picked baby, only after consent, and never fast-changing routine data keeps the caregiver in control and the context accurate.

Status: Accepted, current.

### ADR-020

Every chat reply of Cub's is checked before the caregiver sees it, and a reply that fails is never shown (#97).

Path:
`ChatPage` -> `CubReplyService` (Core) -> `ICubChatClient` -> Claude, called again for the grade

Rules:
- Code checks come first: a reply fails if it is empty, if it was cut off at the token cap, or if it has markdown (`**`, a code fence, or a `#` heading at the start of a line).
- A reply that passes them goes to a grader: a second call to the same fixed model, with Cub's rules, the chosen style and length, and a language check in its prompt (with "General advice", its rule too; never the baby's details), is told to fail only a clear break of a rule that applies to the message (with a picked baby, also that it can't see the details), and the parent's last message and the reply as its one turn. Only a grade whose first word is `PASS` (in any case, ignoring `*`, `.` or `:` around it) passes; anything else, an unreadable grade included, is a fail.
- A failed reply gets exactly one retry: the same request, with a fixed note for the reason added to the rules (none for an empty reply). The failed reply and the grader's words are never sent back, and never shown.
- If the retry fails too, the caregiver sees the neutral inline line "Cub couldn't find a good answer to that. Try asking another way." It is not an alert and not Cub's voice (ADR-013), and the turn stays out of the history sent to Claude.
- Fail closed: if the grader can't be reached, nothing is shown but today's "Cub couldn't answer just now" line (ADR-019).
- In Debug builds, `CubReplyService` writes why a try failed (the code check, or the grader's words) to the debug output, never the parent's message or the reply; Release builds drop it (#155).
- The checks, the grader's prompt and the notes live in `Core` (`CubReplyChecks`, `CubGuidelines`, `CubReplyService`); `Remote/` only maps the call (ADR-017).

Reason:
- The parent is often an expecting or new parent asking about their baby: a reply that breaks the rules must not reach them, even rarely.
- Code catches the cheap, certain failures for free; only the grader can judge tone, language and the rules, so it is paid for only when the code finds nothing.
- One retry turns most cut-offs and slips into a good reply without making the parent wait for more than four calls.
- Fixed notes keep model text from steering the next call.

Status: Accepted, current.
