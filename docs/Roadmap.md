# Future features and Roadmap.

## Phase 1: 

```
	UI Redesign:
		- Complete redesign for all pages
		- Calm and Encouraging soft colors palette and theme
		- Card-based, rounded corners layouts for tasks and appointments
		- Companion character theme and idealization
			-> Current candidate: "Buddy Bear", "Cuddly Lion Cub", "Fox", "Robot"

	Design System:
		- Design system remap for consistent UI components and layouts
		- Reusable styles, templates, and controls
```

## Phase 2:

```
	Companion Character:
		- Introduce a companion character to guide and accompany users through the app
		- Character will provide tips, encouragement, and feedback
		- Static SVG or Lottie animations for character interactions
		- Simple and short animations to avoid overwhelming users
		- Reside on MainPage only (at least initially)

	Toast Messages:
		- Implement toast messages for Task Completion, added, deleted, and appointment scheduled
		- Toasts will be designed to be non-intrusive and visually appealing
		- Mascot interactions will be included in toast messages for added engagement
```

## Phase 3: 

```
	Data Persistence:
		- Implement local data persistence for tasks and appointments
		- Use SQLite or similar local database for offline-first functionality
		- Ensure data is saved and retrieved efficiently
```

## Phase 4:

```
	Architecture Enhancements:
		- Refactor codebase to adopt MVVM architecture
		- Implement feature-based folder structure for better expansibility and modularity
		- Introduce dependency injection for services and view models
		- Improve state management using ObservableCollection or similar patterns
```

## Phase 5:

```
	Cloud Synchronization:
		- Implement cloud synchronization for tasks and appointments
		- Allow users to sync data across multiple devices
		- Ensure data security and privacy during synchronization
		- Azure is the preferred cloud provider for this feature

	Database Backup and Restore:
		- Implement backup and restore functionality for local data
		- Allow users to create backups of their data and restore them when needed
		- Ensure backup files are secure and easily accessible
```

## Phase 6: 

```
	Companion comes to life:
		- Implement interactive animations for the companion character (e.g., waving, clapping, dancing)
		- Allow users to customize the companion character's appearance and behavior (basic customization options)
		- Introduce an AI agent for the companion character to provide personalized tips and answer basic questions.
		- Allow user to minimaly adjust companion personality and behavior (e.g., more encouraging, more playful, more serious)
			-> Enforce well rounded and structured guidelines for AI agent, this is not a doctor or therapist
```

## Phase 7:

```
	Advanced AI Features:
		- Personalized advice based on their baby's age and development stage
		- Introduce machine learning algorithms to provide insights and recommendations based on user data
		- Ensure AI features are safe, reliable, and adhere to privacy regulations
```