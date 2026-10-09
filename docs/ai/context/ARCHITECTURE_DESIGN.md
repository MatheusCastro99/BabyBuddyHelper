# Current Architecture Design:

## Structure:

- Core/
  - Models/
  - Services/
  - Interfaces/
  - Persistence/
  - Collections/
  - Exceptions/
  - Storage/
- UI/
  - Pages/
  - Controls/
  - Services/
  - Converters/
- test/BabyBuddyHelper.Tests/
  - Infrastructure/
  - Services/
  - Models/

## Current key files:

Core/Models/

- TaskModel.cs
- AppointmentModel.cs
- BabyModel.cs
- VaccineModel.cs
- VaccinationRecordModel.cs

Core/Services/

- TaskListService.cs
- BabyProfileService.cs
- BabyFilterService.cs
- TrackerDataSeeder.cs
- VaccineCatalog.cs
- VaccineService.cs

Core/Interfaces/

- ITaskListService.cs
- IBabyProfileService.cs
- IBabyFilterService.cs
- ITrackerDbService.cs
- IVaccineCatalog.cs
- IVaccineService.cs
- IApiKeyStore.cs

Core/Persistence/

- TrackerContext.cs
- EfTrackerDbService.cs

Core/Collections/

- RangeObservableCollection.cs

Core/Exceptions/

- DbCommunicationException.cs
- KeyStorageException.cs

Core/Storage/

- SecureApiKeyStore.cs

UI/Pages/

- MainPage
- ChecklistPage
- AddTaskPage
- AddBabyPage
- BabyProfilePage
- AddVaccineRecordPage
- CalendarPage
- ChatPage

UI/Controls/

- CompanionView
- CubSettingsPopup
- ToastView
- PaletteSwitchView
- CubView.cs
- KeyboardFocusRing.cs

UI/Services/

- ToastService.cs
- AlertService.cs
- PaletteService.cs

Resources/Styles/

- Colors.xaml (shared neutrals)
- Palettes/BoyPalette.xaml
- Palettes/GirlPalette.xaml
- Styles.xaml

Resources/Raw/

- cub.json (Cub's Lottie animations)

tools/cub/ (run by hand, not part of the build)

- generate_cub.py
- preview.html

Platforms/Windows/

- ShellTabBarFooter.cs

UI/Converters/

- AssociatedBabyNameConverter.cs
- IsAppointmentModelConverter.cs
- VaccineTagConverter.cs

test/BabyBuddyHelper.Tests/ (a separate plain `net10.0` xUnit project; it links the `Core/` sources instead of referencing the app, leaving out `SecureApiKeyStore.cs`; ADR-018)

- Infrastructure/: TestDatabase.cs, TestServices.cs, DatabaseTest.cs, TestData.cs, TestHostTests.cs
- Services/: TaskCrudTests.cs, TaskConversionTests.cs, TaskSortingTests.cs, TaskFilteringTests.cs, BabyProfileCrudTests.cs, BabyRemovalTests.cs, BabyMeasurementDateTests.cs, BabyFilterServiceTests.cs, VaccinationRecordCrudTests.cs, CollidingDataTests.cs
- Models/: TaskModelTests.cs, VaccinationRecordFlagTests.cs

See the data-flow diagram: [DataFlow.excalidraw](../../DataFlow.excalidraw)

## Future Architecture Direction

The layered Core/UI layout is current (ADR-015). When further redesign becomes necessary, Service-based, Feature-based, and Modular organization are candidates (ADR-005); none is scheduled on the roadmap.

Planned in Phase 5: a root-level `Remote/` folder for off-device implementations (clients for external services, their DTOs and mapping). Contracts stay in `Core/Interfaces`, and only `MauiProgram` references `Remote/`. The rule is recorded in ADR-015 and ADR-017. The folder does not exist yet; its first file arrives with the Claude client in #96.
