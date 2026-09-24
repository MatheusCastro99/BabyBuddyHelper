# Current State:

Status:

- Functional Prototype
- Active Development

---

# Current Development Focus:

## Phase 3.5:

> Folder Restructure:
- Layered Structure: 
	- Core/:
		- Models/ 
			- TaskModel, AppointmentModel, BabyModel
		- Services/ 
			- TaskListService, BabyProfileService, BabyFilterService, TrackerDataService
		- Interfaces/ 
			- ITrackerDbService, IBabyProfileService, ITaskListService, IBabyFilterService
		- PersistanceBoundary/ 
			- TrackerContext, EfTrackerDbService
		- Collections/ 
			- RangeObservableCollection
		- Exceptions/ 
			- DbCommunicationException
	- UI/:
		- Pages/
			- MainPage, AddTaskPage, AddAppointmentPage, AddBabyPage, BabyProfilePage
		- Controls/
			- CompanionView, ToastView
		- Services/ 
			- ToastService, AlertService
- Rest remain unchanged (App.xaml, AppShell.xaml, Resources/, Assets/, etc.)

---
