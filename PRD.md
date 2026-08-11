# Product Requirements Document: BabyBuddyHelper

## Purpose
BabyBuddyHelper is a cross-platform application designed to help parents manage and track essential baby care activities and preparation tasks. The app provides a centralized dashboard for organizing tasks by priority and completion status, with a countdown timer to the expected due date.

## Technical Specifications
- **Language**: C#
- **Framework**: .NET MAUI
- **Version**: .NET 10
- **App Type**: Cross-platform mobile and desktop application (iOS, Android, Windows, macOS)

## Custom Data Types
```
TaskModel
├── Id (int)
├── TaskName (string) - Name of the task
├── TaskDescription (string) - Detailed description
├── TaskPriority (int) - Priority level for sorting
└── IsCompleted (bool) - Completion status

AppointmentModel
├── Extends (inherits from) TaskModel
├── AppointmentDate (DateTime) - Time of appointment
└── AppointmentLocation (string) - Location of appointment
```

## Solution Structure
```
BabyBuddyHelper/
├── Pages/
│   ├── MainPage.xaml(.cs) - Due date countdown and home dashboard
│   ├── ChecklistPage.xaml(.cs) - Task list with filters and sorting
│   ├── AddTaskPage.xaml(.cs) - Task creation interface
│   └── CalendarPage.xaml(.cs) - Calendar Visual of Appointments
├── Models/
│   ├── TaskModel.cs - Data model for tasks
│   └── AppointmentModel.cs - Data model for appointment (extends tasks)
├── Resources/
│   └── Styles/ - Application-wide styling
├── Platforms/ - Platform-specific implementations
└── MauiProgram.cs - Application configuration
```

## External Resources
- **Database**: None (in-memory storage currently; scalable to SQLite, MSSQL, or Azure)
- **Cloud Services**: None currently; candidate for Azure migration
- **APIs**: None currently

## Planned Development Time
**Total Estimated Hours**: 14-22 hours
- Core Features: 8-12 hours
- UI/UX Polish: 2-4 hours
- Testing & Refinement: 4-6 hours
