# BabyBuddyHelper

[![Build Validation](https://github.com/MatheusCastro99/BabyBuddyHelper/actions/workflows/build.yml/badge.svg)](https://github.com/MatheusCastro99/BabyBuddyHelper/actions/workflows/build.yml)
[![CodeQL](https://github.com/MatheusCastro99/BabyBuddyHelper/actions/workflows/github-code-scanning/codeql/badge.svg)](https://github.com/MatheusCastro99/BabyBuddyHelper/security/code-scanning)
![Dependabot](https://img.shields.io/badge/Dependabot-Enabled-brightgreen)

[![Status](https://img.shields.io/badge/Status-Active%20Development-orange)](https://github.com/MatheusCastro99/BabyBuddyHelper)

[![.NET](https://img.shields.io/badge/.NET%20MAUI-Cross--Platform-blue)](https://learn.com/dotnet/maui/)
[![Platforms](https://img.shields.io/badge/Platforms-Windows%20%7C%20Android%20%7C%20iOS%20%7C%20macOS-success)](https://learn.com/dotnet/maui/)

BabyBuddyHelper is a cross-platform parenting companion built with .NET MAUI. Today, it helps parents and caregivers organize tasks, track appointments, manage baby profiles, and visualize schedules through checklist and calendar experiences.

The project is currently a functional prototype in active development. Its current focus is baby profile pages and vaccination record tracking, while preserving the shipped task, appointment, baby-profile, filtering, and companion experience.

## Vision and Experience

BabyBuddyHelper is evolving from a task and appointment manager into a warm, friendly, and supportive companion for everyday family life. It is intended to feel low-stress and encouraging, helping caregivers keep track of routines and baby-care activities without becoming another productivity application.

The current experience already includes a lightweight companion character and supportive toast feedback. The companion is intended to be welcoming and supportive, not an AI chatbot. Advanced companion behavior, cloud synchronization, and AI-driven interactions remain planned direction.

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
- Week and Month calendar views
- Visual appointment scheduling
- Double-click appointment editing
- Create appointments directly from empty calendar cells
- Real-time calendar refresh when tasks change

### Baby Profiles & Task Association

- Create, edit, and delete baby profiles
- Link tasks and appointments to specific baby profiles
- Automatically project profile names into checklist and calendar-related workflows

### Companion & Feedback

- Companion character on MainPage
- Companion-themed toast feedback for key user actions

### Filtering & Ordering

- Completion-first ordering (pending first)
- Upcoming-date ordering for appointments
- Baby-specific checklist filtering

### Cross-Platform

- Windows
- Android
- iOS
- macOS

## Planned Direction

- Backup and restore
- Cloud synchronization and shared family experiences
- Later, carefully scoped AI-supported interactions

These capabilities are planned and are not included in the current prototype.

---

## Technology Stack

### Frontend

- .NET MAUI
- XAML
- C#

### UI Components

- Syncfusion Scheduler

### Architecture

- Service-Oriented Design
- Dependency Injection
- ObservableCollection-based State Management

### Development Tooling

- GitHub Actions Build Validation
- Dependabot Dependency Monitoring
- CodeQL Security Analysis

---

## Current Architecture

The project intentionally favors simplicity over premature optimization.

Current structure:

```text
Core/   Models/, Services/, Interfaces/, Persistence/, Collections/, Exceptions/
UI/     Pages/, Controls/, Services/, Converters/
```

Business logic is centralized through services and interfaces, including:

```text
ITaskListService / TaskListService
IBabyProfileService / BabyProfileService
IBabyFilterService / BabyFilterService
ITrackerDbService / EfTrackerDbService
```

`TaskListService` remains the application's single source of truth for tasks and appointments.

The app uses in-memory cache services (`TaskListService`, `BabyProfileService`) backed by `ITrackerDbService`. `EfTrackerDbService` is the active EF Core + SQLite implementation. All screens should continue to consume task and appointment state through `TaskListService`, and Syncfusion Scheduler types should remain in the UI layer.

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
│   │   ├── TaskModel.cs
│   │   ├── AppointmentModel.cs
│   │   └── BabyModel.cs
│   │
│   ├── Services/
│   │   ├── TaskListService.cs
│   │   ├── BabyProfileService.cs
│   │   ├── BabyFilterService.cs
│   │   └── TrackerDataSeeder.cs
│   │
│   ├── Interfaces/
│   │   ├── ITaskListService.cs
│   │   ├── IBabyProfileService.cs
│   │   ├── IBabyFilterService.cs
│   │   └── ITrackerDbService.cs
│   │
│   ├── Persistence/
│   │   ├── TrackerContext.cs
│   │   └── EfTrackerDbService.cs
│   │
│   ├── Collections/
│   │   └── RangeObservableCollection.cs
│   │
│   └── Exceptions/
│       └── DbCommunicationException.cs
│
├── UI/
│   ├── Pages/
│   │   ├── MainPage.xaml(.cs)
│   │   ├── ChecklistPage.xaml(.cs)
│   │   ├── AddTaskPage.xaml(.cs)
│   │   ├── AddBabyPage.xaml(.cs)
│   │   └── CalendarPage.xaml(.cs)
│   │
│   ├── Controls/
│   │   ├── CompanionView.xaml(.cs)
│   │   └── ToastView.xaml(.cs)
│   │
│   ├── Services/
│   │   ├── ToastService.cs
│   │   └── AlertService.cs
│   │
│   └── Converters/
│       ├── AssociatedBabyNameConverter.cs
│       └── IsAppointmentModelConverter.cs
│
├── Platforms/
├── Resources/
│   └── Styles/
│       ├── Colors.xaml
│       └── Styles.xaml
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

---

## Development Practices

This project follows modern software engineering practices:

- Automated Build Validation
- Continuous Integration with GitHub Actions
- Security Scanning with CodeQL
- Automated Dependency Updates via Dependabot
- Dependency Injection
- Interface-Based Service Design

---

## Roadmap

### Near Term

- Expanded baby-care tracking with vaccination records tracking

### Medium Term

- Dockerization for easier deployment
- Cloud synchronization
- Data backup and restore

### Long Term

- Shared family calendars
- Interactive companion behavior
- Carefully scoped AI-supported interactions

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
