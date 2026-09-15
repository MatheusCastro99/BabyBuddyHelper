# BabyBuddyHelper Copilot Instructions

This repository contains a custom AI framework.

Before making recommendations, reviewing code, or implementing features, review the following documents when relevant:

## Core Context

- docs/ai/AI_CONTEXT.md
- docs/ai/context/CURRENT_STATE.md
- docs/ai/context/DECISIONS.md
- docs/ai/context/ROADMAP.md
- docs/ProjectVision.md

## Agent System

The repository uses a custom agent architecture available as subagents for specialized tasks.

Coordinator Agent:
- Primary entry point
- Classifies requests
- Routes work
- Orchestrates workflows

Specialized Agents:

- TechLead
- ProductOwner
- UXDesigner
- ProjectHistorian
- RedTeam

## Project Philosophy

Prioritize:

- Stability
- Simplicity
- Extensibility
- Feature Velocity

Avoid recommending:

- CQRS
- MediatR
- Repository Pattern
- Full Clean Architecture
- MVVM

unless project complexity clearly justifies them.

See:
docs/ai/AI_CONTEXT.md