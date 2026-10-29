# BabyBuddyHelper

[![Build Validation](https://github.com/MatheusCastro99/BabyBuddyHelper/actions/workflows/build.yml/badge.svg)](https://github.com/MatheusCastro99/BabyBuddyHelper/actions/workflows/build.yml)
[![xUnit Test Validation](https://github.com/MatheusCastro99/BabyBuddyHelper/actions/workflows/xunit-tests.yml/badge.svg)](https://github.com/MatheusCastro99/BabyBuddyHelper/actions/workflows/xunit-tests.yml)
[![CodeQL](https://github.com/MatheusCastro99/BabyBuddyHelper/actions/workflows/github-code-scanning/codeql/badge.svg)](https://github.com/MatheusCastro99/BabyBuddyHelper/security/code-scanning)
![Dependabot](https://img.shields.io/badge/Dependabot-Enabled-brightgreen)

[![Status](https://img.shields.io/badge/Status-Active%20Development-orange)](https://github.com/MatheusCastro99/BabyBuddyHelper)

[![.NET](https://img.shields.io/badge/.NET%20MAUI-Cross--Platform-blue)](https://learn.microsoft.com/dotnet/maui/)
[![Platforms](https://img.shields.io/badge/Platforms-Windows%20%7C%20Android%20%7C%20iOS%20%7C%20macOS-success)](https://learn.microsoft.com/dotnet/maui/)

BabyBuddyHelper is a cross-platform parenting companion built with .NET MAUI. Today, it helps parents and caregivers organize tasks, track appointments, manage baby profiles, and visualize schedules through checklist and calendar experiences.

The project is currently a functional prototype in active development. Phase 4 (vaccine tracker) and its follow-ups are concluded. Phase 5 is concluded too: it added a dual UI mode (a boy / girl palette in place of a light / dark mode) and brought the companion to life with interactive animations, the chat page and the settings for the caregiver's own API key, a carefully scoped AI helper (Cub answers in the chat through Claude, on the caregiver's own key), and a light personality adjustment, with a check on every reply. Phase 6 then prepares the first release: mobile targets in CI, a mobile UI/UX polish pass, EF Core migrations, then deployment and distribution. The shipped task, appointment, baby-profile, vaccine, and filtering experience stays as it is.

## Vision and Experience

BabyBuddyHelper is evolving from a task and appointment manager into a warm, friendly, and supportive companion for everyday family life. It is intended to feel low-stress and encouraging, helping caregivers keep track of routines and baby-care activities without becoming another productivity application.

The current experience already includes an animated companion character (Cub waves, claps, dances and nods), supportive toast feedback, and an optional chat where Cub gives tips and answers basic questions (not a doctor or therapist) in a style and reply length the family can adjust. The companion is intended to be welcoming and supportive, not a general-purpose AI chatbot. Planned direction starts with a first release, then user accounts and cloud synchronization.

---

## Implemented Features

### Task Management

- Create and manage tasks
- Edit existing tasks
- Delete tasks
- Mark tasks as completed
- Priority-based organization
- Completion-based organization

### Appointment Management

- Create appointments with date, time, and location
- Edit appointments from both the checklist and calendar views
- Convert tasks into appointments
- Convert appointments into tasks
- Automatic scheduler synchronization

### Calendar & Scheduling

- Integrated Syncfusion Scheduler
- Week calendar view
- Visual appointment scheduling
- Double-click appointment editing
- Create appointments directly from empty calendar cells
- Real-time calendar refresh when tasks change

### Baby Profiles & Task Association

- Create, edit, and delete baby profiles
- Read-only profile page per baby, with edit and delete from there
- "Last updated" date under a baby's weight and height on the profile
- Built-in US CDC vaccine list (birth to 6 years) on each profile, with a disclaimer
- Per-baby vaccination records (doses given, last and next dose dates), added and edited from the profile
- Vaccine status per vaccine (doses given, next dose, gentle overdue reminder)
- Due soon / due today tag on the dashboard baby cards and the profile's vaccine rows
- Seasonal vaccines (flu, COVID-19) tracked on the profile
- Link tasks and appointments to specific baby profiles
- Automatically project profile names into checklist and calendar-related workflows

### Companion & Feedback

- Companion character on MainPage
- Cub is animated (Lottie): waves on MainPage and when tapped, dances on "Tell me something"
- Companion-themed toast feedback for key user actions, with Cub clapping for a completion, nodding for an add or update, and staying still for a removal
- "Talk to Cub" chat page, opened from the companion card: Cub answers everyday baby-care questions through Claude, on the caregiver's own key. A picker in Cub's card ("Cub is talking about: …") keeps it to general advice (the default) or shares one baby's name, age, height, weight and vaccines; feeds, sleeps and the birth date are never sent, and the chat is gone when the page closes
- Cub's settings: the caregiver's own Claude API key, kept in the device's secure storage, and how Cub talks in the chat: a conversation style (Supportive, Neutral or Direct) and a reply length (Short, Balanced or Detailed)
- Every chat reply is checked before it is shown (code checks, then a second Claude call that grades it); a reply that fails is retried once and otherwise never shown

### Filtering & Ordering

- Completion-first ordering (pending first)
- Upcoming-date ordering for appointments
- Baby-specific checklist filtering

### Dual UI Mode

- Boy and girl color palettes in place of a light / dark mode
- Palette switch on the tab row (Windows; mobile placement is planned in the Phase 6 polish pass)
- The choice is saved on the device and applied at startup

### Data & Reliability

- Local, offline-first storage on the device (EF Core + SQLite)
- Friendly recovery when saving or loading fails: a failed save changes nothing and says so, and a failed startup load offers to try again

### Cross-Platform

- Windows
- Android
- iOS
- macOS

## Planned Direction

- A first release: mobile polish, release-ready data migrations, and distribution (app stores or direct downloads)
- User accounts, cloud synchronization, and backup and restore; later, sharing with family
- Later, personalized advice based on the baby's age and development stage

These capabilities are planned and are not included in the current prototype.

---

## Technology Stack

### Frontend

- .NET MAUI
- XAML
- C#

### UI Components

- Syncfusion Scheduler
- SkiaSharp.Extended.UI.Maui (Lottie animation for Cub)
- CommunityToolkit.Maui
- Anthropic C# SDK (Cub's chat replies, in `Remote/`)

### Data

- EF Core
- SQLite (local, on the device)

### Architecture

- Service-Oriented Design
- Dependency Injection
- ObservableCollection-based State Management

### Development Tooling

- GitHub Actions Build Validation
- GitHub Actions xUnit Test Validation
- xUnit (v3) unit tests for `Core/`, on a SQLite in-memory database
- Dependabot Dependency Monitoring
- CodeQL Security Analysis
- Claude Code pull request review and @claude mentions (GitHub Actions)

---

## Current Architecture

The project intentionally favors simplicity over premature optimization.

Current structure:

```text
Core/   Models/, Services/, Interfaces/ (each with Baby/, TaskList/, Vaccine/ and Cub/ subfolders), Persistence/, Collections/, Exceptions/, Storage/
UI/     Pages/, Controls/, Services/, Converters/
```

Business logic is centralized through services and interfaces, including:

```text
ITaskListService / TaskListService
IBabyProfileService / BabyProfileService
IBabyFilterService / BabyFilterService
ITrackerDbService / EfTrackerDbService
IVaccineCatalog / VaccineCatalog
IVaccineService / VaccineService
IApiKeyStore / SecureApiKeyStore
ICubPersonalityStore / PreferencesCubPersonalityStore
```

`TaskListService` remains the application's single source of truth for tasks and appointments.

The app uses in-memory cache services (`TaskListService`, `BabyProfileService`, `VaccineService`) backed by `ITrackerDbService`. `EfTrackerDbService` is the active EF Core + SQLite implementation. All screens should continue to consume task and appointment state through `TaskListService`, and Syncfusion Scheduler types should remain in the UI layer.

Future architectural refinements may include:

- Feature-based folder structure
- Cloud Synchronization

MVVM remains deferred until the application's complexity justifies it.

---

## Project Structure

```text
BabyBuddyHelper/
│
├── Core/
│   ├── Models/
│   │   ├── Baby/
│   │   │   └── BabyModel.cs
│   │   ├── TaskList/
│   │   │   ├── TaskModel.cs
│   │   │   └── AppointmentModel.cs
│   │   ├── Vaccine/
│   │   │   ├── VaccineModel.cs
│   │   │   └── VaccinationRecordModel.cs
│   │   └── Cub/
│   │       ├── ChatTurn.cs
│   │       ├── BabyChatContext.cs
│   │       ├── CubPersonality.cs
│   │       ├── CubReply.cs
│   │       ├── CubThinking.cs
│   │       └── CubOutcome.cs
│   │
│   ├── Services/
│   │   ├── Baby/
│   │   │   ├── BabyProfileService.cs
│   │   │   └── BabyFilterService.cs
│   │   ├── TaskList/
│   │   │   └── TaskListService.cs
│   │   ├── Vaccine/
│   │   │   ├── VaccineCatalog.cs
│   │   │   └── VaccineService.cs
│   │   ├── Cub/
│   │   │   ├── CubGuidelines.cs
│   │   │   ├── CubReplyChecks.cs
│   │   │   └── CubReplyService.cs
│   │   └── TrackerDataSeeder.cs
│   │
│   ├── Interfaces/
│   │   ├── Baby/
│   │   │   ├── IBabyProfileService.cs
│   │   │   └── IBabyFilterService.cs
│   │   ├── TaskList/
│   │   │   └── ITaskListService.cs
│   │   ├── Vaccine/
│   │   │   ├── IVaccineCatalog.cs
│   │   │   └── IVaccineService.cs
│   │   ├── Cub/
│   │   │   ├── ICubChatClient.cs
│   │   │   └── ICubPersonalityStore.cs
│   │   ├── ITrackerDbService.cs
│   │   └── IApiKeyStore.cs
│   │
│   ├── Persistence/
│   │   ├── TrackerContext.cs
│   │   └── EfTrackerDbService.cs
│   │
│   ├── Collections/
│   │   └── RangeObservableCollection.cs
│   │
│   ├── Exceptions/
│   │   ├── DbCommunicationException.cs
│   │   ├── KeyStorageException.cs
│   │   └── CubUnavailableException.cs
│   │
│   └── Storage/
│       ├── SecureApiKeyStore.cs
│       └── PreferencesCubPersonalityStore.cs
│
├── Remote/
│   └── ClaudeCubChatClient.cs
│
├── UI/
│   ├── Pages/
│   │   ├── MainPage.xaml(.cs)
│   │   ├── ChecklistPage.xaml(.cs)
│   │   ├── AddTaskPage.xaml(.cs)
│   │   ├── AddBabyPage.xaml(.cs)
│   │   ├── BabyProfilePage.xaml(.cs)
│   │   ├── AddVaccineRecordPage.xaml(.cs)
│   │   ├── CalendarPage.xaml(.cs)
│   │   └── ChatPage.xaml(.cs)
│   │
│   ├── Controls/
│   │   ├── CompanionView.xaml(.cs)
│   │   ├── CubSettingsPopup.xaml(.cs)
│   │   ├── ChatItemTemplateSelector.cs
│   │   ├── ToastView.xaml(.cs)
│   │   ├── PaletteSwitchView.xaml(.cs)
│   │   ├── CubView.cs
│   │   └── KeyboardFocusRing.cs
│   │
│   ├── Services/
│   │   ├── ToastService.cs
│   │   ├── AlertService.cs
│   │   └── PaletteService.cs
│   │
│   └── Converters/
│       ├── AssociatedBabyNameConverter.cs
│       ├── IsAppointmentModelConverter.cs
│       └── VaccineTagConverter.cs
│
├── Platforms/
│   └── Windows/
│       └── ShellTabBarFooter.cs
├── Resources/
│   ├── Raw/
│   │   └── cub.json
│   └── Styles/
│       ├── Colors.xaml
│       ├── Palettes/
│       │   ├── BoyPalette.xaml(.cs)
│       │   └── GirlPalette.xaml(.cs)
│       └── Styles.xaml
│
├── test/
│   └── BabyBuddyHelper.Tests/
│       ├── Infrastructure/
│       ├── Services/
│       ├── Models/
│       └── Remote/
│
├── tools/
│   └── cub/
│       ├── generate_cub.py
│       └── preview.html
│
├── MauiProgram.cs
├── App.xaml
└── BabyBuddyHelper.csproj
```

---

## Installation

### Prerequisites

- .NET 10 SDK
- Visual Studio 2026 Community (or later)
- .NET MAUI Workload

Platform-specific requirements:

- Android SDK (Android)
- Xcode (iOS/macOS)
- Windows 10/11 SDK (Windows)

### Clone Repository

```bash
git clone https://github.com/MatheusCastro99/BabyBuddyHelper.git
cd BabyBuddyHelper
```

### Restore Packages

```bash
dotnet restore
```

### Build

```bash
dotnet build
```

The build compiles the Syncfusion license from `SYNCFUSION_LICENSE` in `.env` (or the environment variable in CI). A key is valid for one Syncfusion major version only, so a major package upgrade needs a new key.

### Test

```bash
dotnet test --project test/BabyBuddyHelper.Tests/BabyBuddyHelper.Tests.csproj
```

The tests cover `Core/` and `Remote/`, and run on a SQLite in-memory database and a fake network, never on the app's own database or a real provider. New tests go in `test/BabyBuddyHelper.Tests/`: `Services/` for tests through the cache services, `Models/` for model logic, `Remote/` for off-device clients on a fake network, `Infrastructure/` for the test host. Inside `Services/` and `Models/`, tests sit in the same feature subfolder (`Baby`, `TaskList`, `Vaccine`, `Cub`) as the code they test; cross-feature tests stay at the root. A change to `Core/` behaviour comes with tests.

---

## Development Practices

This project follows modern software engineering practices:

- Automated Build Validation
- Automated Unit Tests (xUnit) for `Core/`
- Continuous Integration with GitHub Actions
- Security Scanning with CodeQL
- Automated Dependency Updates via Dependabot
- Dependency Injection
- Interface-Based Service Design

---

## Roadmap

### Near Term (Phase 5)

- Interactive companion (animations shipped) and AI groundwork (shipped)
- Chat page and settings for the caregiver's own API key (shipped)
- A carefully scoped AI helper: an optional chat with Cub for tips and basic questions, using the caregiver's own API key (not a doctor or therapist) (shipped)
- Light personality adjustment for the companion (conversational style and verbosity), with a check on every reply (shipped)

### Medium Term (Phase 6)

- Mobile targets in CI
- Mobile UI/UX polish
- EF Core migrations
- Deployment and distribution (app stores or direct downloads, Dockerization)

### Long Term (Phases 7 and 8)

- User authentication and accounts
- Cloud synchronization (Azure)
- Data backup and restore
- Later, shared family experiences such as shared calendars
- Advanced AI features: personalized, age-based advice

The roadmap is directional; planned items should not be read as currently available features.

---

## Organization

Developed as part of:

- Microsoft Software and Systems Academy (MSSA)
- Cloud Application Development (PCAD21)

---

## Support

Issues, feature requests, and feedback are welcome through GitHub Issues:

https://github.com/MatheusCastro99/BabyBuddyHelper/issues

---

## Author

**Matheus Castro**

Repository:
https://github.com/MatheusCastro99/BabyBuddyHelper
