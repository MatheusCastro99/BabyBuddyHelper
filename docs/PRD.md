# Product Requirements Document: BabyBuddyHelper

## Purpose
BabyBuddyHelper is an offline-first parenting companion that helps parents and caregivers manage and track essential baby care activities and preparation tasks. The app organizes tasks by priority and completion status, schedules appointments on an integrated calendar, keeps a profile for each baby (tasks and appointments can be linked to one) with a built-in, read-only US CDC vaccine list, a disclaimer, per-baby vaccination records, and each vaccine's status, and offers a home dashboard with a countdown timer to the expected due date. A companion character and gentle toast feedback keep the experience warm and encouraging. Data is stored locally on the device (SQLite).

## Technical Specifications
- **Language**: C#
- **Framework**: .NET MAUI
- **Version**: .NET 10
- **App Type**: Cross-platform mobile and desktop application (iOS, Android, Windows, macOS)

## Custom Data Types
```
TaskModel
├── Id (Guid) - Unique identifier, created by the app
├── AssociatedBabyId (Guid?) - Baby profile the task belongs to (optional; only the Id is stored)
├── TaskName (string) - Name of the task
├── TaskDescription (string) - Detailed description
├── TaskPriority (int) - Priority level for sorting
└── IsCompleted (bool) - Completion status

AppointmentModel
├── Extends (inherits from) TaskModel
├── AppointmentDate (DateTime?) - Date of appointment
├── AppointmentStartTime (TimeSpan?) - Start time
├── AppointmentEndTime (TimeSpan?) - End time
├── AppointmentLocation (string) - Location of appointment
└── SchedulerStartTime / SchedulerEndTime (DateTime, computed) - Combined date and time for the calendar

BabyModel
├── Id (Guid) - Unique identifier, created by the app
├── Name (string) - Baby's name
├── DateOfBirth (DateTime) - Date of birth
├── WeightInLbs (double) - Weight in pounds
├── HeightInFt (double) - Height in feet
├── LastFeed (DateTime) - Time of the last feed
├── LastSleep (DateTime) - Time of the last sleep
└── AgeText (string, computed) - Display age, never stored

VaccineModel (fixed catalog entry, never stored in the database)
├── Id (Guid) - Hardcoded, never regenerated (vaccination records reference it)
├── CvxCode (string) - CDC vaccine code
├── Name (string) - Display name
├── Description (string) - A plain-language description of the vaccine (RSV is an antibody shot)
└── IsRecurrent (bool) - Seasonal vaccine (flu, COVID-19): no total, never complete

VaccinationRecordModel (one baby's progress on one catalog vaccine, stored in the database)
├── Id (Guid) - Unique identifier, created by the app
├── BabyId (Guid) - The baby it belongs to, never changes
├── VaccineId (Guid) - The catalog vaccine it tracks, never changes
├── TotalDoses (int?) - Doses in the series (depends on the brand), empty for a seasonal vaccine
├── CompletedDoses (int) - Doses given so far
├── LastAdministered (DateTime?) - Date of the last dose, empty until the first dose
├── NextDose (DateTime?) - Optional date of the next dose
├── IsCompleted (bool, computed) - All doses given (false without a total), never stored
├── IsCurrent (bool, computed) - Seasonal only: a shot in the last 12 months and no next dose due yet (a date rule, not "up to date"), never stored
├── IsOverdue (bool, computed) - Next dose is before today and the record is not complete, never stored
├── IsDueToday (bool, computed) - Next dose is today and the record is not complete, never stored
└── IsDueSoon (bool, computed) - Next dose is 1 to 14 days away and the record is not complete (due today is not due soon), never stored
```

## Solution Structure
```
BabyBuddyHelper/
├── Core/
│   ├── Models/
│   │   ├── TaskModel.cs - Data model for tasks
│   │   ├── AppointmentModel.cs - Data model for appointments (extends tasks)
│   │   ├── BabyModel.cs - Data model for baby profiles (age computed from date of birth)
│   │   ├── VaccineModel.cs - Data model for one vaccine in the built-in catalog
│   │   ├── VaccinationRecordModel.cs - Data model for one baby's doses and dates for one vaccine
│   │   ├── ChatTurn.cs - One message of the chat with Cub, kept in memory only
│   │   ├── BabyChatContext.cs - What Cub may know about the baby picked in the chat (never the birth date, feeds or sleeps)
│   │   ├── CubPersonality.cs - How Cub talks in the chat: the style and reply length the parent picks
│   │   ├── CubReply.cs - One unchecked reply from the chat provider: its text and whether it was cut off
│   │   └── CubOutcome.cs - What the chat shows for one message: a checked reply, or nothing when both tries failed
│   ├── Services/
│   │   ├── TaskListService.cs - In-memory cache and single source of truth for tasks and appointments
│   │   ├── BabyProfileService.cs - In-memory cache and single source of truth for baby profiles
│   │   ├── BabyFilterService.cs - Baby selection options shared by the checklist, calendar and task entry
│   │   ├── TrackerDataSeeder.cs - Debug-only mock data seeding
│   │   ├── VaccineCatalog.cs - Fixed US CDC vaccine list (birth to 6 years), no database access
│   │   ├── VaccineService.cs - In-memory cache and single source of truth for vaccination records
│   │   ├── CubGuidelines.cs - Cub's fixed chat rules, the style and length lines, the baby block, the grader's prompt, the retry notes, and the caps
│   │   ├── CubReplyChecks.cs - The code checks on a reply: empty, cut off, or markdown
│   │   └── CubReplyService.cs - The sanity layer: asks Cub, checks and grades the reply, and retries once
│   ├── Interfaces/
│   │   ├── ITaskListService.cs - Interface for TaskListService
│   │   ├── IBabyProfileService.cs - Interface for BabyProfileService
│   │   ├── IBabyFilterService.cs - Interface for BabyFilterService
│   │   ├── ITrackerDbService.cs - Persistence boundary, used only by the cache services
│   │   ├── IVaccineCatalog.cs - Interface for VaccineCatalog
│   │   ├── IVaccineService.cs - Interface for VaccineService
│   │   ├── IApiKeyStore.cs - Contract for keeping the user's own AI provider key in the device's secure storage
│   │   ├── ICubChatClient.cs - Contract for Cub's chat replies (the off-device path, ADR-017): Core gives the prompt, cap and turns
│   │   └── ICubPersonalityStore.cs - Contract for keeping Cub's style and reply length on the device
│   ├── Persistence/
│   │   ├── TrackerContext.cs - EF Core SQLite database context
│   │   └── EfTrackerDbService.cs - EF Core + SQLite implementation of ITrackerDbService
│   ├── Collections/
│   │   └── RangeObservableCollection.cs - ObservableCollection that adds many items with one notification
│   ├── Exceptions/
│   │   ├── DbCommunicationException.cs - Raised when the local database can't be read or written
│   │   ├── KeyStorageException.cs - Raised when the device's secure storage can't save or remove the user's key
│   │   └── CubUnavailableException.cs - Raised when Cub can't answer (no key, no network, or the provider refusing)
│   └── Storage/
│       ├── SecureApiKeyStore.cs - Secure-storage implementation of IApiKeyStore (the user's Claude API key)
│       └── PreferencesCubPersonalityStore.cs - Preferences implementation of ICubPersonalityStore (Cub's style and reply length)
├── Remote/
│   └── ClaudeCubChatClient.cs - Cub's chat replies from Claude (Haiku 5.5) on the user's own key
├── UI/
│   ├── Pages/
│   │   ├── MainPage.xaml(.cs) - Due date countdown and home dashboard
│   │   ├── ChecklistPage.xaml(.cs) - Task list with filters and sorting
│   │   ├── AddTaskPage.xaml(.cs) - Task and appointment creation / edit interface
│   │   ├── AddBabyPage.xaml(.cs) - Baby profile creation / edit interface
│   │   ├── BabyProfilePage.xaml(.cs) - Read-only baby profile with the vaccine list, each vaccine's status, and disclaimer; entry point for edit and delete, and for vaccine records
│   │   ├── AddVaccineRecordPage.xaml(.cs) - Vaccination record creation / edit / remove interface
│   │   ├── CalendarPage.xaml(.cs) - Calendar visual of appointments
│   │   └── ChatPage.xaml(.cs) - Chat with Cub: setup state without a key; with one, the baby picker in Cub's card ("is talking about:"), the conversation with Cub's replies, and the message box
│   ├── Controls/
│   │   ├── CompanionView.xaml(.cs) - Companion character card
│   │   ├── CubView.cs - Cub, the animated companion, shared by the card and the toasts
│   │   ├── ToastView.xaml(.cs) - Companion-themed toast feedback
│   │   ├── PaletteSwitchView.xaml(.cs) - Boy / girl palette switch
│   │   ├── CubSettingsPopup.xaml(.cs) - Cub's settings popup: enter, replace or remove the user's API key, and pick Cub's style and reply length
│   │   ├── ChatItemTemplateSelector.cs - Picks the chat's parent bubble, Cub bubble, thinking line or failure line
│   │   └── KeyboardFocusRing.cs - Focus ring for keyboard users
│   ├── Services/
│   │   ├── ToastService.cs - Toast feedback for successful actions
│   │   ├── AlertService.cs - Alerts for failures the user must acknowledge
│   │   └── PaletteService.cs - Applies the boy or girl palette and remembers the choice
│   └── Converters/
│       ├── AssociatedBabyNameConverter.cs - Shows a task's baby name from its stored baby Id
│       ├── IsAppointmentModelConverter.cs - Tells the checklist whether an item is an appointment
│       └── VaccineTagConverter.cs - Shows a baby card's due today / due soon vaccine tag on the dashboard, looked up by baby Id
├── Resources/
│   └── Styles/ - Application-wide styling (Colors.xaml, Styles.xaml, Palettes/BoyPalette.xaml and GirlPalette.xaml)
├── Platforms/ - Platform-specific implementations
├── Properties/ - Launch settings
├── test/
│   └── BabyBuddyHelper.Tests/ - xUnit tests for Core/ and Remote/ (a separate project that links their sources; the network is a fake)
├── App.xaml(.cs) - Application entry and startup data loading
├── AppShell.xaml(.cs) - Tab navigation (Home, Checklist, Calendar)
├── MauiProgram.cs - Application configuration and dependency injection
└── BabyBuddyHelper.csproj - Project file and build configuration
```

## External Resources
- **Database**: SQLite via EF Core (local persistence active); cloud synchronization remains deferred
- **Cloud Services**: None currently; candidate for Azure migration
- **APIs**: the Anthropic Messages API (Claude Haiku 5.5), only when the caregiver writes to Cub, on the caregiver's own key (ADR-017)

## Original Estimate (Phase 1 prototype)
Kept as a historical record. It covers the original prototype only, not later phases.

**Total Estimated Hours**: 14-22 hours
- Core Features: 8-12 hours
- UI/UX Polish: 2-4 hours
- Testing & Refinement: 4-6 hours
