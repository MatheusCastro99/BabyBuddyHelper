# Current Architecture Design:

## Structure:

- Core/
  - Models/
  - Services/
  - Interfaces/
    - Baby/
    - TaskList/
    - Vaccine/
    - Cub/
  - Persistence/
  - Collections/
  - Exceptions/
  - Storage/
- Remote/
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
- ChatTurn.cs
- BabyChatContext.cs
- CubPersonality.cs
- CubReply.cs
- CubOutcome.cs

Core/Services/

- TaskListService.cs
- BabyProfileService.cs
- BabyFilterService.cs
- TrackerDataSeeder.cs
- VaccineCatalog.cs
- VaccineService.cs
- CubGuidelines.cs
- CubReplyChecks.cs
- CubReplyService.cs

Core/Interfaces/

- Baby/
  - IBabyProfileService.cs
  - IBabyFilterService.cs
- TaskList/
  - ITaskListService.cs
- Vaccine/
  - IVaccineCatalog.cs
  - IVaccineService.cs
- Cub/
  - ICubChatClient.cs
  - ICubPersonalityStore.cs
- ITrackerDbService.cs
- IApiKeyStore.cs

Core/Persistence/

- TrackerContext.cs
- EfTrackerDbService.cs

Core/Collections/

- RangeObservableCollection.cs

Core/Exceptions/

- DbCommunicationException.cs
- KeyStorageException.cs
- CubUnavailableException.cs

Core/Storage/

- SecureApiKeyStore.cs
- PreferencesCubPersonalityStore.cs

Remote/ (off-device implementations only; only `MauiProgram` references it, ADR-015 and ADR-017)

- ClaudeCubChatClient.cs

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
- ChatItemTemplateSelector.cs
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

test/BabyBuddyHelper.Tests/ (a separate plain `net10.0` xUnit project; it links the `Core/` and `Remote/` sources instead of referencing the app, leaving out `SecureApiKeyStore.cs` and `PreferencesCubPersonalityStore.cs`, which need MAUI; ADR-018)

- Infrastructure/: TestDatabase.cs, TestServices.cs, DatabaseTest.cs, TestData.cs, TestHostTests.cs, FakeAnthropicHandler.cs, FakeApiKeyStore.cs, FakeCubChatClient.cs
- Services/: TaskCrudTests.cs, TaskConversionTests.cs, TaskSortingTests.cs, TaskFilteringTests.cs, BabyProfileCrudTests.cs, BabyRemovalTests.cs, BabyMeasurementDateTests.cs, BabyFilterServiceTests.cs, VaccinationRecordCrudTests.cs, CollidingDataTests.cs, CubGuidelinesTests.cs, ClaudeCubChatClientTests.cs, CubReplyChecksTests.cs, CubReplyServiceTests.cs
- Models/: TaskModelTests.cs, VaccinationRecordFlagTests.cs, BabyChatContextTests.cs, CubPersonalityTests.cs

See the data-flow diagram: [DataFlow.excalidraw](../../docs/DataFlow.excalidraw)

## Future Architecture Direction

The layered Core/UI layout is current (ADR-015). When further redesign becomes necessary, Service-based, Feature-based, and Modular organization are candidates (ADR-005); none is scheduled on the roadmap.

The root-level `Remote/` folder holds off-device implementations (clients for external services, their DTOs and mapping). Contracts stay in `Core/Interfaces`, and only `MauiProgram` references `Remote/`. The rule is recorded in ADR-015 and ADR-017. Its first file is the Claude client for Cub's chat (`ClaudeCubChatClient`, #96).

## Platform notes

Details that the code doesn't explain on its own. Moved from the retired `AI_CONTEXT.md` (#121).

### Secrets at build time

- `.env` is for development only and is a build input only: the `GenerateBuildSecrets` target compiles its values into `BuildSecrets`. In CI the values come from GitHub Secrets.
- Do not embed `.env` as an `EmbeddedResource`, and do not copy it to the output directory. Both approaches were removed due to security concerns.
- Server-side credentials never go into the client app.
- At startup, `MauiProgram` validates the registered Syncfusion key and writes the result to the debug output (Syncfusion's message when it is rejected, never the key).

### The user's API key (ADR-010)

- The user-supplied key is the one secret that is not compiled in. `IApiKeyStore` (`SecureApiKeyStore` in `Core/Storage`) keeps it in secure storage only, checks its format only, never reads it back into the UI and never logs it.
- A key the device cannot read is treated as "no key" and never deletes the stored value.
- Android keeps the secure storage file out of backups (`auto_backup_rules.xml` and `data_extraction_rules.xml`).

### Cub (Lottie)

- Cub is one generated Lottie file, `Resources/Raw/cub.json`, played through the shared `UI/Controls/CubView.cs`. `tools/cub/generate_cub.py` writes the file (run by hand, never by the build).
- The frame table in `CubView` mirrors `SEGMENTS` in the script, so the two must change together.
- `SkiaSharp.Extended.UI.Maui` targets .NET 9, so the csproj pins `Microsoft.Maui.Controls.Compatibility` 10.0.110 explicitly; without it the Windows build breaks. Drop the pin once the Lottie package ships a .NET 10 build.

### Chat page

- `ChatPage` only asks the key store whether a key exists and never reads the key. The conversation is never stored.
- Replies come through `CubReplyService` (ADR-020), which calls `ICubChatClient` (ADR-017). `MainPage` passes the reply service, the personality store and the baby, filter, vaccine and catalog services to the page; the page never references `Remote/`.
- `CubReplyService` trims the history, asks for a reply, runs the code checks (`CubReplyChecks`: empty, cut off, markdown) and then the grader, and retries once with a fixed note. It returns a `CubOutcome`: `Reply` or `Declined`. `CubUnavailableException`, a grader that can't answer included, and cancellation pass through.
- The page shows a `Reply` and adds it to the history; `Declined` and a `CubUnavailableException` each show their own neutral line on the failure template, and the message never joins the history.
- The style and reply length (`CubPersonality`) are a device setting in Preferences (`ICubPersonalityStore`, like the palette), never in the database. The page reads them when a message is sent, so a change in the popup applies from the next message. `CubPersonality.FromSaved` falls back to Supportive and Balanced, each on its own, for a missing or unknown name.
- `ICubChatClient` takes the system prompt, the output-token cap and the turns, all built in Core (`CubGuidelines`), and returns the reply unchecked as a `CubReply`: its trimmed text, which may be empty, and whether it was cut off at the cap. `Remote/` only fixes the model and maps to the SDK.
- The baby picker in Cub's card ("is talking about:") is read when a message is sent, so a change applies from the next message. "General advice" sends no baby data; a picked baby is turned into a `BabyChatContext` at that moment.
- The page keeps two lists: the items it shows (parent, Cub, thinking, failure) and the turns it sends (answered messages and their replies only). A message that failed is never sent again.
- Leaving the page (a pop, a pop to the tab's root, or a removal) cancels a reply still on its way, and a late reply is dropped. Opening the settings popup is a modal push and switching tabs is a replace, so neither does: the chat is still there when the parent comes back.
- The settings popup (`CubSettingsPopup`) is shown as a modal page, so `ChatPage.OnAppearing` runs again when it closes. Keep that: it is how the page picks up a key saved or removed there.
- `CubSettingsPopup` saves the style and length the moment a radio button is picked; there is no Save for them. The `RadioButton` style in `Styles.xaml` draws the circle with palette colors, so it follows the palette instead of the system accent; the radio semantics (selection, keyboard focus) stay.
- `CubSettingsPopup` shows the consent confirm before every `SaveKeyAsync`, a replace included. "Not now" returns before anything is stored. Keys saved before #96 never saw it.
- On Android the page switches the window to resize above the keyboard while it shows, and back to the default when it goes away.
- On Windows, focus returns to the message box after Send through the native control, because MAUI's `IsFocused` is stale there and `Focus()` does nothing.

### Theme

- The app is pinned to the light theme (`App.xaml.cs`, through `UserAppTheme`); the boy and girl palettes take the place of a dark mode (UI_GUIDELINES.md).
- `PaletteService` applies the palette and saves the choice in Preferences (a device UI setting, not database data).
- The Windows title bar uses the nav bar colors through a MAUI `TitleBar`.
