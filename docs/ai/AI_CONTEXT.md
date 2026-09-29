# AI_CONTEXT.md

# BabyBuddyHelper - AI Development Context

Last Updated: 2026-09-28

---
# Context References:

## Project Vision

Summary:

BabyBuddyHelper is an offline-first parenting companion
that helps caregivers organize routines, appointments,
and baby-care activities in a warm and encouraging environment.

Complete Context:

-   Read the full vision document here: [Project Vision](../ProjectVision.md)

---

## Current Architecture

-   Refer to: [Architecture Design](context/ARCHITECTURE_DESIGN.md)

---

## Current Project State

-   For a focused look at the current project state, refer to: [Project State](context/CURRENT_STATE.md)

---

## Project Roadmap

-   For a complete Roadmap reference, refer to: [Roadmap](context/ROADMAP.md)

---

## Project Decisions

-   For a complete list of project decisions, refer to: [Project Decisions](context/DECISIONS.md)

---

## UI Guidelines

-   For the visual design and UX guardrails, refer to: [UI Guidelines](context/UI_GUIDELINES.md)

---

## Current Feature Set

-   For a complete look on current and planned features, refer to: [Feature Set](../FeaturesSet.md)

---

-------------------------------------- END OF INDEXED PAGES ----------------------------------------------------

---

# Technical Philosophy

The project intentionally prioritizes:

- Shipping features
- Domain discovery
- Simplicity

over:

- Premature optimization
- Enterprise architecture patterns
- Overengineering

Preferred approach:

1. Build functionality.
2. Discover real requirements.
3. Refactor when patterns emerge.

Avoid introducing:

- CQRS
- MediatR
- Full Clean Architecture
- Repository Pattern
- MVVM

until the project complexity justifies them.

---

# Core Entities

## TaskModel

Represents a standard task.

Current responsibilities:

- Name
- Description
- Priority
- Completion State
- Associated Baby Id
- Guid Identifier

TaskModel uses Guid identifiers.

The project previously used integer IDs.

Guid migration was completed because update operations became unreliable.

See ADR-007 in DECISIONS.md for the mandatory Id-based lookup/update/remove rule.

Checklist completion must remain one-way bound in XAML and must route through `SetCompletionAsync`; switching it back to two-way would skip persistence.
Never set a bound property from code (for example, `checkBox.IsChecked = ...`); the binding must own the state. Use `ClearValue` when a UI reset is required.

Do not revert.

## AppointmentModel

Inherits from TaskModel.

Additional data:

- Appointment Date
- Appointment Start Time
- Appointment End Time
- Location

Includes scheduler helper properties:

- SchedulerStartTime
- SchedulerEndTime

These computed properties exist specifically for Syncfusion Scheduler integration.

## BabyModel

Represents one baby profile.

- Name
- DateOfBirth (a future date is an expected baby)
- Weight and height
- Last feed and last sleep
- Guid Identifier

`AgeText` is computed for display and never stored, so the age can't go stale.

## VaccineModel

One entry of the fixed vaccine catalog (US CDC schedule, birth to 6 years).

- Guid Identifier (hardcoded and never regenerated, because vaccination records reference it)
- CvxCode
- Name
- Description

Served read-only by `IVaccineCatalog`. Never stored in the database (ADR-012).

## VaccinationRecordModel

One baby's progress on one catalog vaccine. At most one record per baby per vaccine.

- Guid Identifier
- BabyId and VaccineId (required, never change; VaccineId has no database foreign key because the catalog is not in the database)
- TotalDoses and CompletedDoses
- LastAdministered and NextDose (both optional)

`IsCompleted` and `IsOverdue` are computed for display and never stored. A dose due today is not overdue. Deleting a baby deletes its records (database cascade).

---

# Data Source

Current cache services:

- TaskListService for tasks and appointments
- BabyProfileService for baby profiles
- VaccineService for vaccination records

Persistence boundary:

- ITrackerDbService
- `EfTrackerDbService` is the current implementation

TaskListService owns:

ObservableCollection<TaskModel>

BabyProfileService owns:

ObservableCollection<BabyModel>

VaccineService owns:

ObservableCollection<VaccinationRecordModel>

Reference data (not a cache service, no database):

- IVaccineCatalog serves the fixed vaccine catalog (ADR-012)

### WARNINGS: 

-   All screens consume user data through these cache services. Fixed reference data (the vaccine catalog) is read directly from `IVaccineCatalog`.
-   No page should become an alternate source of truth.
-   No page should talk directly to `ITrackerDbService`.

---

# Current Data Persistence Strategy

Persistence Status:

Implemented (local SQLite via EF Core)

Current storage:

- In-memory cache services
- `EfTrackerDbService` over EF Core + SQLite
- SQLite file: `babybuddy.db3` under `FileSystem.AppDataDirectory`
- Database path is written to Debug output on first use
- Debug-only seeded development data

Reason:

The persistence boundary and SQLite backend are active. Writes are database-first, failed writes throw `DbCommunicationException`, and startup load failures surface alerts so the user can retry or continue.
Schema evolution currently uses `EnsureCreated` with debug-only reset during development (migrations are tracked in #76).

Debug-only flags:
- `ResetDatabaseOnStartup` for one-run schema resets after a schema change
- `SimulateDbFailure` for exercising write-failure and startup-retry paths

Future persistence plan includes:
    Azure for cloud synchronization and redundancy.

---

# Calendar Architecture

## Calendar uses:
Syncfusion Scheduler

The scheduler currently consumes:
    ObservableCollection<SchedulerAppointment> (SINGLE SOURCE OF TRUTH)

Appointments are projected from:

- AppointmentModel -> SchedulerAppointment

inside CalendarPage.

This projection exists because appointment mapping caused integration issues.

## Current flow:

AppointmentModel
    ->
SchedulerAppointment
    ->
SfScheduler

Scheduler-specific types should remain in the UI layer.

Avoid introducing Syncfusion dependencies into services.

---

# Editing Workflow

Single Point of Editing:
    AddTaskPage

All task and appointment creation/editing should eventually route through this page.

Consumers:

-   Checklist Page
-   Calendar Page
-   Future Dashboard

Examples:

-   Tap Existing Task in Checklist
    ->
AddTaskPage (Editing Task Constructor)

-   DoubleClick Scheduler Appointment
    ->
AddTaskPage (Editing Appointment Constructor)

-   DoubleClick Empty Scheduler Cell
    ->
AddTaskPage (Add New Appointment Constructor with pre-filled data)

This is an intentional design decision.

Baby profiles follow the same rule with AddBabyPage (ADR-004):

-   Tap Baby Card on MainPage
    ->
BabyProfilePage (pushed, read-only; ADR-016)
    ->
Edit profile
    ->
AddBabyPage (modal)

Vaccination records follow the same rule with AddVaccineRecordPage:

-   Tap a vaccine row on BabyProfilePage
    ->
AddVaccineRecordPage (modal)

---

# Scheduler Behavior

Current behavior:

- Displays appointments only (Not Tasks)
- Double click on filled slots edits appointments
- Double click on empty slots creates appointment
- Refreshes when TaskListService changes

Refresh currently occurs through:

-   TaskListService.Tasks.CollectionChanged

Calendar refreshing from collection events is acceptable.

### WARNINGS:

-   Sorting from collection events is NOT acceptable.

-   This previously caused ObservableCollection re-entrancy exceptions.

-   Avoid reintroducing collection-triggered sorting.

---

# Development Infrastructure

Implemented:

✅ GitHub Actions

✅ Build Validation

✅ Dependabot

✅ CodeQL

✅ GitHub Advanced Security

✅ Claude Code pull request review (claude-code-review.yml) and @claude mentions (claude.yml)

Current CI is considered stable.

---

# Secret Management

Current approach:

.env file

DEVELOPMENT ONLY.

## Configuration:

.env is compiled in at build time, file is a build input only.

Values are compiled into BuildSecrets by the GenerateBuildSecrets target.

Do NOT embed .env as EmbeddedResource.
Do NOT copy .env to output directory.

These approaches were removed due to security concerns.

## Preferred pattern:

Development
    ->
.env

CI/CD
    ->
GitHub Secrets

### WARNINGS:

-   Do NOT commit secrets to source control, NEVER.
-   Server-side credentials never go into the client app

---

# UI Guidelines

UI Modernization

### Goals:

- Modern appearance
- Soft, calming color palette
- Improved spacing
- Better visual hierarchy
- Improved data-entry experience

### Target feeling:

Parent Companion

NOT

Business Productivity Tool

### Avoid:

- Harsh colors
- Overly dense layouts
- Corporate visual language

For a look on the current project state, refer to: [Current State](context/CURRENT_STATE.md)
For a complete roadmap, refer to: [Roadmap](context/ROADMAP.md)

## Design System Status

Current shared resources include:

Resources/Styles/
    Colors.xaml
    Styles.xaml

Future expansion candidates:

Resources/Styles/
    Typography.xaml
    Themes.xaml

Theme:

The app is light-theme only (pinned in App.xaml.cs via UserAppTheme). The Windows title bar uses the nav bar colors through a MAUI TitleBar.

Goal:

Consistent warm and soothing visual language.

---

# Known Technical Debt

Low Priority:

- No MVVM
- No EF migration workflow yet (current schema strategy is `EnsureCreated` + debug reset; tracked in #76)
- No INotifyPropertyChanged

Accepted trade-offs.

Medium Priority:

- Improve CodeQL quality metrics
- Reduce scheduler projection complexity

High Priority:

None currently.

Focus should remain on product functionality, stability, and UX.

---

# AI Guidance

When assisting with this project:

Prioritize:

1. Product usability
2. Maintainability
3. Simple architecture
4. Feature velocity

Avoid recommending enterprise patterns unless justified by:

- Multiple persistence layers
- Complex business workflows
- Significant scaling requirements

Default recommendation:

-   Simple solution first.
-   Refactor later.

This philosophy aligns with the project's current development style.