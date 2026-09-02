# AI_CONTEXT.md

# BabyBuddyHelper - AI Development Context

Last Updated: 2026-08-31

---
# Context References:

## Project Vision

Summary:

BabyBuddyHelper is an offline-first parenting companion
that helps caregivers organize routines, appointments,
and baby-care activities in a warm and encouraging environment.

Complete Context:

-   Read the full vision document here: [Project Vision] (docs/ProjectVision.md)

---

## Current Architecture

-   Refer to: [Architecture Design] (docs/ai/context/ARCHITECTURE_DESIGN.md)

---

## Current Project State

-   For a focused look at the current project state, refer to: [Project State] (docs/ai/context/CURRENT_STATE.md)

---

## Project Roadmap

-   For a complete Roadmap reference, refer to: [Roadmap] (docs/ai/context/ROADMAP.md)

---

## Project Decisions

-   For a complete list of project decisions, refer to: [Project Decisions] (docs/ai/context/DECISIONS.md)

---

## UI Guidelines

-   For the visual design and UX guardrails, refer to: [UI Guidelines] (docs/ai/context/UI_GUIDELINES.md)

---

## Current Feature Set

-   For a complete look on current and planned features, refer to: [Feature Set] (docs/FeaturesSet.md)

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
- Guid Identifier

TaskModel uses Guid identifiers.

The project previously used integer IDs.

Guid migration was completed because update operations became unreliable.

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

---

# Data Source

Current source of truth:

TaskListService

TaskListService owns:

ObservableCollection<TaskModel>

### WARNINGS: 

-   All screens consume data from this service.
-   No page should become an alternate source of truth.

---

# Current Data Persistence Strategy

Persistence Status:

Not Implemented

Current storage:

- In-Memory
- Mock Data

Reason:

Domain model is still evolving.

SQLite is intentionally deferred.

Future persistence plan includes:
    SQLite for local storage
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

-   Tap Empty Scheduler Cell
    ->
AddTaskPage (Add New Appointment Constructor with pre-filled data)

This is an intentional design decision.

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

Current CI is considered stable.

---

# Secret Management

Current approach:

.env file

DEVELOPMENT ONLY.

## Configuration:

.env copied to output directory.

Environment variables loaded from:

AppContext.BaseDirectory

Do NOT embed:

.env

as EmbeddedResource.

This approach was removed.

## Preferred pattern:

Development
    ->
.env

CI/CD
    ->
GitHub Secrets

Production
    ->
Environment Variables

### WARNINGS:

-   Do NOT commit secrets to source control, NEVER.

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

For a look on the current project state, refer to: [Current State] (docs/ai/context/CURRENT_STATE.md)
For a complete roadmap, refer to: [Roadmap] (docs/ai/context/ROADMAP.md)

## Planned Design System

Future shared resources:

Resources/
    Colors.xaml
    Typography.xaml
    Styles.xaml
    Themes.xaml

Goal:

Consistent warm and soothing visual language.

---

# Known Technical Debt

Low Priority:

- No MVVM
- No SQLite
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