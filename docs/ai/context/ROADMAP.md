# Future features and Roadmap.

## Phase 1: CONCLUDED

UI Redesign: CONCLUDED

- Complete redesign for all pages
- Calm and Encouraging soft colors palette and theme
- Card-based, rounded corners layouts for tasks and appointments
- Companion character theme and idealization
	> Current candidates: "Buddy Bear", "Cuddly Lion Cub", "Fox", "Robot"

Design System: CONCLUDED

- Design system remap for consistent UI components and layouts
- Reusable styles, templates, and controls

Main Page: CONCLUDED
- Turn Main Page into an informative day dashboard.
- Dashboard will include Today's appointment, Baby(ies) age, Baby(ies) weight

## Phase 2: CONCLUDED

BabyModel: CONCLUDED
- New model that will serve as different profiles for different babies / children
- Initial properties:
	- Name (string), Age (int), WeightInLbs (double), HeightInFt (double), LastFeed (DateTime), LastSleep (DateTime)

Baby Profiles: CONCLUDED

- Ability to add, edit, and delete babies profiles
- Refactor Tasks and Appointments to hold an optional BabyName property, indicating which baby the task is related to 

Companion Character: CONCLUDED

- Introduce a companion character to guide and accompany users through the app
- Character will provide tips, encouragement, and feedback
- Static SVG or Lottie animations for character interactions
- Simple and short animations to avoid overwhelming users
- Reside on MainPage only (at least initially)

Toast Messages: CONCLUDED

- Implement toast messages for Task Completion, added, deleted, and appointment scheduled
- Toasts will be designed to be non-intrusive and visually appealing
- Mascot interactions will be included in toast messages for added engagement

Toast Messages for Baby Profile Management: CONCLUDED

Filters Update: CONCLUDED

---

## => Phase 3: 

CodeBase Refactor:

- When Refactoring, ALWAYS consult AI_CONTEXT.md, DECISIONS.md, and ARCHITECTURE_DESIGN.md as guidelines for code production
- Scan codebase for files that
	-> Have too much responsibility
	-> Are way too large (e.g AddTaskPage)
	-> Could be broken down into services (Filtering processes)
- Scan codebase for secrets
- Prepare codebase for DataBase interaction

Data Persistence:

- Reflect on which strategy to use: Local DB with cloud Sync later OR Online DB
- Implement data persistence for tasks, appointments, and Baby Profiles
- Use SQLite or similar local database for offline-first functionality OR PostgresDB for online DB
- Ensure data is saved, read, and retrieved efficiently
- Turn Current TaskListService into a cache service (Will hold in-memory copies of tasks and appointments)

---

## Phase 4:

Architecture Enhancements:

- Refactor codebase to adopt MVVM architecture
- Implement feature-based folder structure for better expansibility and modularity
- Introduce dependency injection for services and view models
- Improve state management using ObservableCollection or similar patterns

---

## Phase 5:

Cloud Synchronization:

- Implement cloud synchronization for tasks and appointments
- Allow users to sync data across multiple devices
- Ensure data security and privacy during synchronization
- Azure is the preferred cloud provider for this feature

Database Backup and Restore:

- Implement backup and restore functionality for local data
- Allow users to create backups of their data and restore them when needed
- Ensure backup files are secure and easily accessible

---

## Phase 6: 

Companion comes to life:

- Implement interactive animations for the companion character (e.g., waving, clapping, dancing)
- Allow users to customize the companion character's appearance and behavior (basic customization options)
- Introduce an AI agent for the companion character to provide personalized tips and answer basic questions.
- Allow user to minimaly adjust companion personality and behavior (e.g., more encouraging, more playful, more serious)
	> Enforce well rounded and structured guidelines for AI agent, this is not a doctor or therapist

---

## Phase 7:

Advanced AI Features:

- Personalized advice based on their baby's age and development stage
- Introduce machine learning algorithms to provide insights and recommendations based on user data
- Ensure AI features are safe, reliable, and adhere to privacy regulations
