# BabyBuddyHelper

A cross-platform mobile and desktop application designed to help parents manage and track daily baby care activities. Built with .NET MAUI, BabyBuddyHelper provides a simple yet effective way to organize feeding times, diaper changes, sleep schedules, and other essential tasks.

## Features

- **Task Management**: Create, update, and track baby care tasks with ease
- **Priority Levels**: Assign priorities to tasks for better organization
- **Progress Tracking**: Mark tasks as completed and maintain a history
- **Cross-Platform**: Available on iOS, Android, macOS, and Windows
- **Responsive UI**: Optimized interface for mobile phones and desktop devices

## Technology Stack

- **.NET MAUI**: Cross-platform app framework
- **.NET 10**: Latest .NET runtime
- **C#**: Primary programming language
- **XAML**: UI markup language

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Visual Studio 2026 Community](https://visualstudio.microsoft.com/vs/community/) or later (with MAUI workload)
- Platform-specific requirements:
  - **iOS**: Xcode and macOS development environment
  - **Android**: Android SDK and emulator/device
  - **Windows**: Windows 10 or later
  - **macOS**: macOS Catalina or later

## Installation

1. Clone the repository:
   ```bash
   git clone https://github.com/MatheusCastro99/BabyBuddyHelper.git
   cd BabyBuddyHelper
   ```

2. Restore NuGet packages:
   ```bash
   dotnet restore
   ```

3. Build the project:
   ```bash
   dotnet build
   ```

## Usage

1. Launch the application on your target platform
2. Create a new task by clicking "Add Task"
3. Fill in task details including name, description, and priority level
	3a. For appointments, set date, time, and location
4. View your tasks on the main dashboard
5. Mark tasks as completed when finished

## Project Structure

```
BabyBuddyHelper/
├── Pages/                      # UI pages for different functionalities
│   ├── MainPage.xaml(.cs)         - Due date countdown and home dashboard
│   ├── ChecklistPage.xaml(.cs)    - Task list with filters and sorting
│   ├── AddTaskPage.xaml(.cs)      - Task creation / edit interface
│   └── CalendarPage.xaml(.cs)     - Calendar Visual of Appointments
├── Models/                     # Custom data types
│   ├── TaskModel.cs               - Data model for tasks
│   └── AppointmentModel.cs        - Data model for appointment (extends tasks)
├── Services/                   # Business logic and data services
│	└── TaskListService.cs         - Service for managing task grouping, sorting, manipulating, and filtering
├──	Interfaces/                 #  Interfaces for services
│	└── ITaskListService.cs        - Interface for TaskListService
├── Platforms/           # Platform-specific implementations
├── Resources/           # Images, fonts, and other assets
├── MauiProgram.cs       # Application configuration
└── App.xaml             # Main application definition
```

## Contributing

For now, due to the academic nature of this project, contributions are not being accepted.
That might change in the near future.

## Organization

- **Microsoft Software and Systems Academy**, 
- **Cloud Application Development - PCAD21**

## Support

For issues, questions, or suggestions, please open an issue on the [GitHub Issues](https://github.com/MatheusCastro99/BabyBuddyHelper/issues) page.

---

**Author:** Matheus Castro  
**Repository:** [MatheusCastro99/BabyBuddyHelper](https://github.com/MatheusCastro99/BabyBuddyHelper)
