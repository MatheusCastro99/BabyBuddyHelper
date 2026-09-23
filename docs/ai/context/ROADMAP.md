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

## => Phase 3: 

> CodeBase Refactor: CONCLUDED

- Oversized-file audit completed.
- Filtering processes split into dedicated services.
- Baby profile and checklist/calendar filtering now run through service interfaces.
- Repository-wide secret audit completed; remaining hardening follow-ups are tracked in issue #51.

> Data Persistence: CONCLUDED

- BabyModel logic has been migrated from stored Age to DateOfBirth with computed age display.
- `ITrackerDbService` defines the persistence boundary.
- `TaskListService` and `BabyProfileService` are the cache services backed by `ITrackerDbService`.
- `EfTrackerDbService` is the active EF Core + SQLite implementation.
- Database-first writes are complete: `EfTrackerDbService` commits first, `DbCommunicationException` reports write failures, and startup load failures alert the user so they can retry.
- Debug-only `SimulateDbFailure` and `ResetDatabaseOnStartup` support development-time failure and schema-reset testing.
- Phase 3 is complete.

---

## Phase 4:

> Baby Profile Page:

- Baby's name as the title
- Age (computed from DateOfBirth), weight and height
- Last feed and last sleep
- An empty area reserved for the Vaccines section (sub-issue 2)
- Reads data only from IBabyProfileService (ADR-001/008).
- Profile Navigation Plan: 
	- MainPage -> Tap Baby Profile Card -> BabyProfilePage
	- BabyProfilePage -> Tap Edit Button -> AddBabyPage (sub-issue 1)

> Vaccine Catalog:

- VaccineModel: 
	- Guid Id (Not Instantiated, these will be fixed and hardcoded)
	- string CvxCode
	- string Name
	- string Description
- IVaccineCatalog
- Catalog does not touches the database, it is a static list of vaccines
- Explicit Disclaimers:
	- Vaccine catalog is for personal tracking purposes only and not a substitute for professional medical advice
	- Mention source of information (CDC, WHO, etc.) and provide links to official resources

> Vaccination Records:

- VaccinationRecordModel:
	- Guid Id
	- Guid BabyId
	- Guid VaccineId
	- int TotalDoses
	- int CompletedDoses
	- DateTime LastAdministered
	- DateTime NextDose
	- Bool IsOverdue => Computed property based on NextDose and current date
	- Bool IsCompleted => Computed property based on CompletedDoses and TotalDoses
- IVaccineService
- VaccineService:
	- cache service and single source of truth for vaccination records
	- Will handle vaccination records for each baby profile
- At most one vaccination record per baby per vaccine
- Deleting Baby Profile deletes all associated vaccination records (No orphaned records)
- SQLite: entity mapping plus EF Core integration. Current strategy uses `EnsureCreated`; if migrations are still not adopted then schema changes require a debug reset run.

> AddVaccineRecordPage:

- Opened from a vaccine in the profile's Vaccines section.
- Creates a record for a "Not started" vaccine, or edits an existing one; a record can also be removed.
- Fields: total doses (depends on the brand), completed doses, last applied date, next dose date.

> Phase 4 Cleanup:

- Documentation updates: ROADMAP, FeaturesSet, ARCHITECTURE_DESIGN, CURRENT_STATE, AI_CONTEXT
- Decisions ammendments:
	- ADR-001: IVaccineService is the single owner of vaccination records.
	- ADR-004: AddVaccineRecordPage is the single record editor; BabyProfilePage is the entry point.
	- ADR-006: add IVaccineCatalog as an example.
	- ADR-008: add IVaccineService as a cache service.
- Wording review (disclaimer, overdue label, empty states) against UI_GUIDELINES.md using ux-review skill.
- Backlog issues for the epic's out-of-scope items and any annotated follow-ups.

---

## Phase 5:

Architecture Enhancements:

- Reflect on what architecture patterns are working well and what can be improved
- Candidate patterns include:
  - MVVM
  - Service-based architecture
  - Feature-based folder structure

---

## Phase 6:

Cloud Synchronization:

- Implement cloud synchronization for tasks and appointments
- Allow users to sync data across multiple devices
- Ensure data security and privacy during synchronization
- Azure is the preferred cloud provider for this feature

Database Backup and Restore:

- Implement backup and restore functionality for local data
- Allow users to create backups of their data and restore them when needed
- Ensure backup files are secure and easily accessible

---

## Phase 7: 

Companion comes to life:

- Implement interactive animations for the companion character (e.g., waving, clapping, dancing)
- Allow users to customize the companion character's appearance and behavior (basic customization options)
- Introduce an AI agent for the companion character to provide personalized tips and answer basic questions.
- Allow user to minimaly adjust companion personality and behavior (e.g., more encouraging, more playful, more serious)
	> Enforce well rounded and structured guidelines for AI agent, this is not a doctor or therapist

---

## Phase 8:

Advanced AI Features:

- Personalized advice based on their baby's age and development stage
- Introduce machine learning algorithms to provide insights and recommendations based on user data
- Ensure AI features are safe, reliable, and adhere to privacy regulations
