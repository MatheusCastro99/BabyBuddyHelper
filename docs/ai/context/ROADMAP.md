# Future features and Roadmap.

## Phase 1: CONCLUDED

UI Redesign: CONCLUDED

- Complete redesign for all pages
- Calm and Encouraging soft colors palette and theme
- Card-based, rounded corners layouts for tasks and appointments
- Companion character theme and idealization
	> Current candidates: "Buddy Bear", "Cuddly Lion Cub", "Fox", "Robot"

Design System: CONCLUDED

- Design system remap for consistent UI components and layouts
- Reusable styles, templates, and controls

Main Page: CONCLUDED
- Turn Main Page into an informative day dashboard.
- Dashboard will include Today's appointment, Baby(ies) age, Baby(ies) weight

## Phase 2: CONCLUDED

BabyModel: CONCLUDED
- New model that will serve as different profiles for different babies / children
- Initial properties:
	- Name (string), DateOfBirth (DateTime), WeightInLbs (double), HeightInFt (double), LastFeed (DateTime), LastSleep (DateTime)

Baby Profiles: CONCLUDED

- Ability to add, edit, and delete babies profiles
- Refactor Tasks and Appointments to hold an optional BabyName property, indicating which baby the task is related to 

Companion Character: CONCLUDED

- Introduce a companion character to guide and accompany users through the app
- Character will provide tips, encouragement, and feedback
- Static SVG or Lottie animations for character interactions
- Simple and short animations to avoid overwhelming users
- Reside on MainPage only (at least initially)

Toast Messages: CONCLUDED

- Implement toast messages for Task Completion, added, deleted, and appointment scheduled
- Toasts will be designed to be non-intrusive and visually appealing
- Mascot interactions will be included in toast messages for added engagement

Toast Messages for Baby Profile Management: CONCLUDED

Filters Update: CONCLUDED

---

## Phase 3: CONCLUDED

> CodeBase Refactor: CONCLUDED

- Oversized-file audit completed.
- Filtering processes split into dedicated services.
- Baby profile and checklist/calendar filtering now run through service interfaces.
- Repository-wide secret audit completed; remaining hardening follow-ups are tracked in issue #51.

> Data Persistence: CONCLUDED

- BabyModel logic has been migrated from stored Age to DateOfBirth with computed age display.
- `ITrackerDbService` defines the persistence boundary.
- `TaskListService`, `BabyProfileService` and `VaccineService` are the cache services backed by `ITrackerDbService`.
- `EfTrackerDbService` is the active EF Core + SQLite implementation.
- Database-first writes are complete: `EfTrackerDbService` commits first, `DbCommunicationException` reports database failures, and startup load failures alert the user so they can retry.
- Debug-only `SimulateDbFailure` and `ResetDatabaseOnStartup` support development-time failure and schema-reset testing.
- Phase 3 is complete.

---

## Phase 3.5: CONCLUDED

> Folder Restructure: CONCLUDED
- Layered Structure: 
	- Core/:
		- Models/ 
			- TaskModel, AppointmentModel, BabyModel, VaccineModel, VaccinationRecordModel
		- Services/ 
			- TaskListService, BabyProfileService, BabyFilterService, TrackerDataSeeder, VaccineCatalog, VaccineService
		- Interfaces/ 
			- ITrackerDbService, IBabyProfileService, ITaskListService, IBabyFilterService, IVaccineCatalog, IVaccineService
		- Persistence/ 
			- TrackerContext, EfTrackerDbService
		- Collections/ 
			- RangeObservableCollection
		- Exceptions/ 
			- DbCommunicationException
	- UI/:
		- Pages/
			- MainPage, ChecklistPage, AddTaskPage, AddBabyPage, BabyProfilePage, AddVaccineRecordPage, CalendarPage
		- Controls/
			- CompanionView, ToastView
		- Services/ 
			- ToastService, AlertService
		- Converters/
			- AssociatedBabyNameConverter, IsAppointmentModelConverter, VaccineTagConverter
- Rest remain unchanged (App.xaml, AppShell.xaml, Resources/, etc.)

## Phase 4: CONCLUDED

> Baby Profile Page: CONCLUDED

- Baby's name as the title
- Age (computed from DateOfBirth), weight and height
- Last feed and last sleep
- A read-only Vaccines section (full catalog, "Not started", disclaimer) shipped with the catalog (#43). Vaccination records and their editor have shipped; the per-vaccine status display has shipped too (#47): a pill (Not started / X of N / Complete), the next dose date, and a gentle overdue reminder; seasonal vaccines show Not started / Current / Last: {date} (#82).
- Reads baby data only from IBabyProfileService (ADR-001/008); the Vaccines section reads the fixed catalog from IVaccineCatalog (ADR-012). The profile also reads vaccination records from IVaccineService.
- Profile Navigation: CONCLUDED
	- MainPage -> Tap Baby Profile Card -> BabyProfilePage
	- BabyProfilePage -> Tap Edit Button -> AddBabyPage

> Vaccine Catalog: CONCLUDED

- VaccineModel: 
	- Guid Id (Not Instantiated, these will be fixed and hardcoded)
	- string CvxCode
	- string Name
	- string Description
	- bool IsRecurrent (seasonal vaccine: no total, never complete)
- IVaccineCatalog: Vaccines (display order), RecurrentVaccines (seasonal, display order), GetById (looks in both lists), SourceName, SourceVersion, SourceUrl, LastReviewed
- Source: US CDC schedule 2025 (in effect under the March 2026 court stay), parent-facing CDC link, last reviewed 2026-09-28
- 10 routine entries and 2 seasonal entries (flu, COVID-19); descriptions stored with their "Helps protect against …" lead-in (final wording); RSV is described as an antibody shot
- Catalog does not touch the database, it is a static list of vaccines
- Explicit Disclaimers:
	- Vaccine catalog is for personal tracking purposes only and not a substitute for professional medical advice
	- Mention source of information (CDC, WHO, etc.) and provide links to official resources

> Vaccination Records: CONCLUDED

- VaccinationRecordModel:
	- Guid Id
	- Guid BabyId
	- Guid VaccineId
	- int? TotalDoses (empty for a seasonal vaccine)
	- int CompletedDoses
	- DateTime? LastAdministered
	- DateTime? NextDose
	- Bool IsOverdue => Computed property: NextDose is before today and the record is not complete
	- Bool IsCompleted => Computed property based on CompletedDoses and TotalDoses (false without a total)
	- Bool IsDueToday => Computed property: NextDose is today and the record is not complete
	- Bool IsDueSoon => Computed property: NextDose is 1 to 14 days away and the record is not complete (due today is not due soon)
	- Bool IsCurrent => Computed property: a seasonal record (no total) with at least one shot, the last one less than 12 months ago (CurrentWindowMonths = 12), and no next dose due yet (none set, or still ahead; due today is not current). A date rule, not "up to date"
- IVaccineService
- VaccineService:
	- cache service and single source of truth for vaccination records
	- Handles vaccination records for each baby profile
- At most one vaccination record per baby per vaccine (unique (BabyId, VaccineId) index)
- Deleting Baby Profile deletes all associated vaccination records (No orphaned records)
- SQLite: entity mapping plus EF Core integration. Current strategy uses `EnsureCreated`; the new table reached existing databases through one debug reset run. Migrations are tracked in #76.

> AddVaccineRecordPage: CONCLUDED

- Opened by tapping a vaccine row in the profile's Vaccines section.
- Creates a record for a "Not started" vaccine, or edits an existing one; a record can also be removed, with a confirmation.
- Fields: total doses (depends on the brand), completed doses, last applied date, next dose date. A seasonal vaccine has no total; see #82.
- Total and completed doses use -/+ counters.
- Last dose date is required once a dose is given (pre-filled, and follows new doses).
- Next dose date is optional, and dropped when the record is complete.
- Footer disclaimer.

> Phase 4 Cleanup: CONCLUDED

- Documentation updates: ROADMAP, FeaturesSet, ARCHITECTURE_DESIGN, CURRENT_STATE, AI_CONTEXT (done)
- Decisions ammendments (done):
	- ADR-001: IVaccineService is the single owner of vaccination records.
	- ADR-004: AddVaccineRecordPage is the single record editor; BabyProfilePage is the entry point.
	- ADR-006: add IVaccineCatalog as an example. (done)
	- ADR-008: add IVaccineService as a cache service.
- Wording review (disclaimer, overdue label, empty states, vaccine descriptions) against UI_GUIDELINES.md using ux-design skill. Done; the catalog descriptions are final.
- Re-verify the vaccine catalog and the disclaimer's schedule version after the First Circuit rules on the stayed January 2026 CDC schedule. Moved to #79.
- Backlog issues for the epic's out-of-scope items and any annotated follow-ups. Done: Backlog Implementations (#80) holds Due soon tag (#81) and Recurring vaccines: flu, COVID (#82).
- Not planned: combination vaccines, reminders and notifications, a suggested next dose, non-US schedules.
- Task-card icon buttons (edit and delete) and a delete confirmation for tasks and appointments.

---

## Backlog Implementations (#80) CONCLUDED

> Due soon tag: CONCLUDED (#81)

- Dashboard baby cards show a tag under the age: "Due today: {vaccine}" or "Due soon: {vaccine}"; for several vaccines, "{label}: N vaccines · check baby profile"
- Due today wins over due soon. Overdue never shows on the dashboard; it stays a gentle reminder on the profile
- Profile vaccine rows: a due soon row reads "Next dose: {date}" followed by a "Due soon" pill; due today shows "Due today" with no pill
- Due soon means the next dose is 1 to 14 days away (DueSoonWindowDays = 14) and the record is not complete; computed, never stored

> Recurring vaccines: flu, COVID (#82): CONCLUDED

- Second catalog list: IVaccineCatalog.RecurrentVaccines holds Flu (Influenza, CVX 88) and COVID-19 (CVX 213), flagged IsRecurrent; Vaccines stays the 10 routine entries and GetById looks in both
- Same source as the routine list (US CDC 2025 schedule): flu is routine yearly from 6 months, COVID-19 is shared clinical decision-making from 6 months
- TotalDoses is optional (empty for a seasonal record), so a seasonal record is never complete
- "Current" (IsCurrent): at least one shot, the last one less than 12 months ago, and no next dose due yet. A date rule, not "up to date"; the profile says so in a caption above "Last reviewed"
- Profile: a "Seasonal vaccines" sub-section inside the Vaccines card; pill reads "Not started", a green "Current", or "Last: {date}"
- AddVaccineRecordPage for a seasonal vaccine: no total counter or brand note, "Doses given" capped at 30, next dose date always available, no "all doses given" state, saved with no total
- Schema change reached existing databases through one debug reset (EnsureCreated; migrations are #76)

---

## Phase 5:

Epic: Issue #92. Sub-phases run in order, one issue and one PR each.

> 5.1 Dual UI mode (Issue #93): CONCLUDED

- Instead of regular light / dark mode, the app features a boy / girl palette; the app stays pinned to the light theme
- Two palette dictionaries with identical keys: `BoyPalette` (the previous look, the default) and `GirlPalette` (soft pink, rose and plum); `Colors.xaml` keeps the shared neutrals
- `PaletteStyle` enum (Boy, Girl) and `PaletteService` swap the palette at runtime; the choice is saved in Preferences, with no database change
- A switch on the tab row (Off = Boy / On = Girl) with a boy and a girl symbol icon, on Windows. Mobile placement is part of Issue #72; the saved palette applies on every platform
- Buttons use the firmer `PrimaryDark` fill so white text reaches 4.5:1; the edit and trash icons have one SVG per palette

> 5.2 Companion animations (Issue #94): CONCLUDED

- Cub is now a Lottie animation: one file, `Resources/Raw/cub.json`, holds four animations back to back (wave, clap, dance, nod), each starting and ending on the rest pose. It replaces the static `companion_cub.png`, and Cub was redrawn from the original SVG with arms added
- The file is generated by `tools/cub/generate_cub.py` (run by hand, never by the build); `tools/cub/preview.html` previews it in a browser
- `CubView` (`UI/Controls/`) is the shared control that plays one animation at a time (`CubAnimation`: Wave, Clap, Dance, Nod); both `CompanionView` and `ToastView` use it
- Companion card: Cub waves each time MainPage appears (after a short delay) and when tapped, and dances on "Tell me something". The 5-minute automatic tip and the whole-badge transform animations were removed
- Toasts: Cub claps for a completed task and for a vaccination record that just became complete (new `ToastKind.VaccineRecordCompleted`), nods for adds and updates, and stays still for removals. The toast badge grew from 40 to 64
- Reduced-motion handling was left out on purpose (owner decision)

> 5.3 AI groundwork (Issue #95):

- Groundwork for the companion AI agent, the way Phase 3 prepared the app for data persistence. No AI call and no key handling
- ADRs for the off-device data path, the user-supplied key and the root-level `Remote/` folder (off-device implementations only; contracts stay in `Core/Interfaces`)
- CommunityToolkit.Maui and the official `Anthropic` C# SDK are added, and the app is built for Windows and Android
- A baby's weight and height each get a "last updated" date, stamped when that value changes and shown on the baby profile, so the agent can later tell how fresh they are. A new profile gets a date only for a value that was entered; profiles that already exist get none
- Replaces "Companion customization", which was dropped (appearance options cost more than they give caregivers)

> 5.4 Chat page and Cub settings (Issue #100):

- A "Talk to Cub" button on the companion card opens the chat
- A gear on the chat opens a small settings popup where the user enters, changes or removes their own API key (Claude only for now; there is no backend holding a key)
- A gentle setup state when no key is saved. No AI call yet
- The "Tell me something" tip button stays. It and the chat are completely separate

> 5.5 Companion AI agent (Issue #96):

- Introduce an AI agent for the companion character to provide personalized tips and answer basic questions. Cub is the agent
- The Claude client lives in `Remote/`, behind a contract in `Core/Interfaces`
- Enforce well rounded and structured guidelines for AI agent, this is not a doctor or therapist
- The conversation is not kept after the chat closes, and Cub replies in the language the user writes in
- The AI is optional: the app stays fully usable offline and without a key
- Planned, to confirm at kickoff: the user picks a baby profile for the chat ("General advice" by default) to share that baby's details with Cub. Cub may then use the baby's name; otherwise it says "your baby" or "the little one". Last feed and last sleep are never shared

> 5.6 Companion personality (Issue #97):

- Allow user to minimally adjust how Cub talks: conversational style (supportive, neutral, direct) and verbosity, in the settings popup

---

## Phase 6:

> Mobile Focus:

- CI/CD Pipeline for mobile targets (Issue #71)
- Optimize the app for mobile devices, ensuring a responsive and user-friendly experience (Issue #72)

> Database Preparation for Release:

- Prepare the database for release, ensuring data integrity and consistency
- Implement data migration strategies for future updates and changes to the database schema (Issue #76)

> Deployment and Distribution:

- Prepare the app for deployment
- Consider distribution channels (e.g., app stores, direct downloads) and necessary configurations for each channel
- Dockerize the application for easier distribution and deployment
- Re-verify the vaccine catalog and the disclaimer's schedule version before the first release (Issue #79)

---

## Phase 7:

> Authentication and User Accounts:

- Implement user authentication and account management features
- Allow users to create accounts, log in, and manage their profiles
- Ensure secure handling of user credentials and sensitive information

> Cloud Synchronization:

- Implement cloud synchronization for tasks and appointments
- Allow users to sync data across multiple devices
- Ensure data security and privacy during synchronization
- Azure is the preferred cloud provider for this feature

> Database Backup and Restore:

- Implement backup and restore functionality for local data
- Allow users to create backups of their data and restore them when needed
- Ensure backup files are secure and easily accessible
---

## Phase 8:

> Advanced AI Features:

- Personalized advice based on their baby's age and development stage
- Introduce machine learning algorithms to provide insights and recommendations based on user data
- Ensure AI features are safe, reliable, and adhere to privacy regulations