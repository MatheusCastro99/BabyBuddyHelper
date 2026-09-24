# Product Requirements Document: BabyBuddyHelper

## Purpose
BabyBuddyHelper is an offline-first parenting companion that helps parents and caregivers manage and track essential baby care activities and preparation tasks. The app organizes tasks by priority and completion status, schedules appointments on an integrated calendar, keeps a profile for each baby (tasks and appointments can be linked to one), and offers a home dashboard with a countdown timer to the expected due date. A companion character and gentle toast feedback keep the experience warm and encouraging. Data is stored locally on the device (SQLite).

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
```

## Solution Structure
```
BabyBuddyHelper/
├── Core/
│   ├── Models/
│   │   ├── TaskModel.cs - Data model for tasks
│   │   ├── AppointmentModel.cs - Data model for appointments (extends tasks)
│   │   └── BabyModel.cs - Data model for baby profiles (age computed from date of birth)
│   ├── Services/
│   │   ├── TaskListService.cs - In-memory cache and single source of truth for tasks and appointments
│   │   ├── BabyProfileService.cs - In-memory cache and single source of truth for baby profiles
│   │   ├── BabyFilterService.cs - Baby selection options shared by the checklist, calendar and task entry
│   │   └── TrackerDataSeeder.cs - Debug-only mock data seeding
│   ├── Interfaces/
│   │   ├── ITaskListService.cs - Interface for TaskListService
│   │   ├── IBabyProfileService.cs - Interface for BabyProfileService
│   │   ├── IBabyFilterService.cs - Interface for BabyFilterService
│   │   └── ITrackerDbService.cs - Persistence boundary, used only by the cache services
│   ├── Persistence/
│   │   ├── TrackerContext.cs - EF Core SQLite database context
│   │   └── EfTrackerDbService.cs - EF Core + SQLite implementation of ITrackerDbService
│   ├── Collections/
│   │   └── RangeObservableCollection.cs - ObservableCollection that adds many items with one notification
│   └── Exceptions/
│       └── DbCommunicationException.cs - Raised when the local database can't be read or written
├── UI/
│   ├── Pages/
│   │   ├── MainPage.xaml(.cs) - Due date countdown and home dashboard
│   │   ├── ChecklistPage.xaml(.cs) - Task list with filters and sorting
│   │   ├── AddTaskPage.xaml(.cs) - Task and appointment creation / edit interface
│   │   ├── AddBabyPage.xaml(.cs) - Baby profile creation / edit interface
│   │   └── CalendarPage.xaml(.cs) - Calendar visual of appointments
│   ├── Controls/
│   │   ├── CompanionView.xaml(.cs) - Companion character
│   │   └── ToastView.xaml(.cs) - Companion-themed toast feedback
│   ├── Services/
│   │   ├── ToastService.cs - Toast feedback for successful actions
│   │   └── AlertService.cs - Alerts for failures the user must acknowledge
│   └── Converters/
│       ├── AssociatedBabyNameConverter.cs - Shows a task's baby name from its stored baby Id
│       └── IsAppointmentModelConverter.cs - Tells the checklist whether an item is an appointment
├── Resources/
│   └── Styles/ - Application-wide styling (Colors.xaml, Styles.xaml)
├── Platforms/ - Platform-specific implementations
├── Properties/ - Launch settings
├── App.xaml(.cs) - Application entry and startup data loading
├── AppShell.xaml(.cs) - Tab navigation (Home, Checklist, Calendar)
├── MauiProgram.cs - Application configuration and dependency injection
└── BabyBuddyHelper.csproj - Project file and build configuration
```

## External Resources
- **Database**: SQLite via EF Core (local persistence active); cloud synchronization remains deferred
- **Cloud Services**: None currently; candidate for Azure migration
- **APIs**: None currently

## Original Estimate (Phase 1 prototype)
Kept as a historical record. It covers the original prototype only, not later phases.

**Total Estimated Hours**: 14-22 hours
- Core Features: 8-12 hours
- UI/UX Polish: 2-4 hours
- Testing & Refinement: 4-6 hours
