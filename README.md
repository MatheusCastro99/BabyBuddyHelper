# BabyBuddyHelper

[![Build Validation](https://github.com/MatheusCastro99/BabyBuddyHelper/actions/workflows/build.yml/badge.svg)](https://github.com/MatheusCastro99/BabyBuddyHelper/actions/workflows/build.yml)
[![CodeQL](https://github.com/MatheusCastro99/BabyBuddyHelper/actions/workflows/github-code-scanning/codeql/badge.svg)](https://github.com/MatheusCastro99/BabyBuddyHelper/security/code-scanning)
![Dependabot](https://img.shields.io/badge/Dependabot-Enabled-br)

[![Status](https://img.shields.io/badge/Status-Active%20Development-orange)](https://github.com/MatheusCastro99/BabyBuddyHelper)

[![.NET](https://img.shields.io/badge/.NET%20MAUI-Cross--Platform-blue)](https://learn.com/dotnet/maui/)
[![Platforms](https://img.shields.io/badge/Platforms-Windows%20%7C%20Android%20%7C%20iOS%20%7C%20macOS-success)](https://learn.com/dotnet/maui/)

BabyBuddyHelper is a cross-platform parenting and baby-care assistant built with .NET MAUI. The application helps parents and caregivers organize tasks, track appointments, and visualize schedules through an intuitive checklist and calendar experience.

The project is currently under active development with a focus on appointment management, scheduling, reminders, and future baby-care tracking features.

---

## Features

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

### Cross-Platform

- Windows
- Android
- iOS
- macOS

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
Pages/
Models/
Services/
Interfaces/
```

Business logic is centralized through:

```text
ITaskListService
TaskListService
```

which serves as the application's single source of truth for tasks and appointments.

Future architectural refinements may include:

- MVVM
- Feature-based folder structure
- SQLite Persistence
- Cloud Synchronization

---

## Project Structure

```text
BabyBuddyHelper/
│
├── Pages/
│   ├── MainPage.xaml(.cs)
│   ├── ChecklistPage.xaml(.cs)
│   ├── AddTaskPage.xaml(.cs)
│   └── CalendarPage.xaml(.cs)
│
├── Models/
│   ├── TaskModel.cs
│   └── AppointmentModel.cs
│
├── Services/
│   └── TaskListService.cs
│
├── Interfaces/
│   └── ITaskListService.cs
│
├── Platforms/
├── Resources/
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

- Calendar UX improvements
- Recurring appointments
- Vaccine Record Tracking

### Medium Term

- SQLite persistence
- Expanded baby-care tracking

### Long Term

- MVVM architecture
- Cloud synchronization
- Shared family calendars

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
