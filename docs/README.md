# BabyBuddyHelper

[![Build Validation](https://github.com/MatheusCastro99/BabyBuddyHelper/actions/workflows/build.yml/badge.svg)](https://github.com/MatheusCastro99/BabyBuddyHelper/actions/workflows/build.yml)
[![CodeQL](https://github.com/MatheusCastro99/BabyBuddyHelper/actions/workflows/github-code-scanning/codeql/badge.svg)](https://github.com/MatheusCastro99/BabyBuddyHelper/security/code-scanning)
![Dependabot](https://img.shields.io/badge/Dependabot-Enabled-brightgreen)

[![Status](https://img.shields.io/badge/Status-Active%20Development-orange)](https://github.com/MatheusCastro99/BabyBuddyHelper)

[![.NET](https://img.shields.io/badge/.NET%20MAUI-Cross--Platform-blue)](https://learn.com/dotnet/maui/)
[![Platforms](https://img.shields.io/badge/Platforms-Windows%20%7C%20Android%20%7C%20iOS%20%7C%20macOS-success)](https://learn.com/dotnet/maui/)

BabyBuddyHelper is a cross-platform parenting companion built with .NET MAUI. Today, it helps parents and caregivers organize tasks, track appointments, and visualize schedules through a checklist and calendar experience.

The project is currently a functional prototype in active development. Its current focus is building a reliable foundation for tasks, appointments, and scheduling while the domain and user experience continue to take shape.

## Vision and Experience

BabyBuddyHelper is evolving from a task and appointment manager into a warm, friendly, and supportive companion for everyday family life. It is intended to feel low-stress and encouraging, helping caregivers keep track of routines and baby-care activities without becoming another productivity application.

The long-term experience may include a lightweight companion character that surfaces today's appointments, outstanding tasks, positive reinforcement, and gentle reminders. This companion is intended to be welcoming and supportive, not an AI chatbot. Companion behavior, persistence, cloud synchronization, and AI-driven interactions are planned direction rather than shipped functionality.

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

### Cross-Platform

- Windows
- Android
- iOS
- macOS

## Planned Direction

- Visual redesign with a calm, encouraging design system
- Companion character and lightweight supportive interactions
- Toast messages for task and appointment activity
- Local persistence for tasks and appointments
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

The current data source is in-memory and uses mock data. Persistence has not yet been implemented. All screens should continue to consume task and appointment state through `TaskListService`, and Syncfusion Scheduler types should remain in the UI layer.

Future architectural refinements may include:

- Feature-based folder structure
- SQLite Persistence
- Cloud Synchronization

MVVM remains deferred until the application's complexity justifies it.

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

- UI redesign and design-system remap
- Calendar UX improvements
- Companion character direction and toast messages

### Medium Term

- SQLite persistence
- Expanded baby-care tracking
- Database backup and restore

### Long Term

- Cloud synchronization
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
