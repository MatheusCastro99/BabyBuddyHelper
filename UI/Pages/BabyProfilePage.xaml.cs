using BabyBuddyHelper.Core.Exceptions;
using BabyBuddyHelper.Core.Interfaces;
using BabyBuddyHelper.Core.Models;
using BabyBuddyHelper.UI.Services;

namespace BabyBuddyHelper.UI.Pages;

//Read-only view of one baby, and the entry point for editing and deleting it. AddBabyPage stays the only editor (ADR-004).
public partial class BabyProfilePage : ContentPage
{
    private readonly IBabyProfileService _babyProfileService;
    private readonly IVaccineCatalog _vaccineCatalog;
    private readonly IVaccineService _vaccineService;
    private readonly Guid _babyId;
    private bool _isOpeningRecord;

    public BabyProfilePage(IBabyProfileService babyProfileService, IVaccineCatalog vaccineCatalog, IVaccineService vaccineService, Guid babyId)
    {
        InitializeComponent();

        _babyProfileService = babyProfileService;
        _vaccineCatalog = vaccineCatalog;
        _vaccineService = vaccineService;
        _babyId = babyId;

        ShowCurrentProfile();
        ShowVaccineDisclaimer();
        ShowVaccineStatus();
    }

    //One vaccine row in the Vaccines section: the catalog entry plus this baby's status for it, ready to display
    public sealed record VaccineRow(VaccineModel Vaccine, string PillText, string? StatusLine, bool IsCompleted)
    {
        public string Name => Vaccine.Name;
        public string Description => Vaccine.Description;
        public bool HasStatusLine => StatusLine is not null;
    }

    //Closing an editor brings this page back, so a saved profile or vaccine record shows here
    protected override void OnAppearing()
    {
        base.OnAppearing();

        ShowCurrentProfile();
        ShowVaccineStatus();
    }

    //Found by Id every time (ADR-007): saving an edit replaces the cached instance, so a held reference would go stale
    private void ShowCurrentProfile()
    {
        BindingContext = _babyProfileService.BabyProfiles.FirstOrDefault(x => x.Id == _babyId);
    }

    //The catalog never changes, so its disclaimer is set once. The wording follows the disclaimer approved in #41.
    private void ShowVaccineDisclaimer()
    {
        VaccineDisclaimerLabel.Text = $"This list follows the {_vaccineCatalog.SourceName} ({_vaccineCatalog.SourceVersion}). " +
            "It's here to help you keep track, not to give medical advice. Please confirm timing and doses with your pediatrician.";
        VaccineLastReviewedLabel.Text = $"Last reviewed: {_vaccineCatalog.LastReviewed:MMMM yyyy}";
    }

    //Rebuilds every row in catalog order from this baby's records, so a record saved or removed in the editor shows on return
    private void ShowVaccineStatus()
    {
        Dictionary<Guid, VaccinationRecordModel> recordsByVaccineId = _vaccineService.GetRecordsForBaby(_babyId).ToDictionary(x => x.VaccineId);

        List<VaccineRow> rows = _vaccineCatalog.Vaccines
            .Select(vaccine => CreateVaccineRow(vaccine, recordsByVaccineId.GetValueOrDefault(vaccine.Id)))
            .ToList();

        BindableLayout.SetItemsSource(VaccineList, rows);
    }

    //Status wording approved in #47. A record with no dose given yet still reads "Not started", and overdue is a gentle reminder, not an alarm.
    private static VaccineRow CreateVaccineRow(VaccineModel vaccine, VaccinationRecordModel? record)
    {
        if (record is null)
        {
            return new VaccineRow(vaccine, "Not started", null, false);
        }

        if (record.IsCompleted)
        {
            return new VaccineRow(vaccine, "Complete", null, true);
        }

        string pillText = record.CompletedDoses == 0 ? "Not started" : $"{record.CompletedDoses} of {record.TotalDoses}";

        string? statusLine = record.NextDose?.Date switch
        {
            null => null,
            DateTime nextDose when record.IsOverdue => $"Due since {FormatDoseDate(nextDose)} · check with your pediatrician",
            DateTime nextDose when nextDose == DateTime.Today => "Due today",
            DateTime nextDose => $"Next dose: {FormatDoseDate(nextDose)}"
        };

        return new VaccineRow(vaccine, pillText, statusLine, false);
    }

    //The year only shows when it isn't this year
    private static string FormatDoseDate(DateTime date) =>
        date.Year == DateTime.Today.Year ? date.ToString("MMM d") : date.ToString("MMM d, yyyy");

    private async void OnViewScheduleClicked(object? sender, EventArgs e)
    {
        bool opened;

        try
        {
            opened = await Browser.Default.OpenAsync(_vaccineCatalog.SourceUrl, BrowserLaunchMode.SystemPreferred);
        }
        catch (Exception) //Platforms throw different exceptions when no browser can take the link; all of them mean it didn't open
        {
            opened = false;
        }

        if (!opened)
        {
            await AlertService.ShowLinkFailedAsync(this, _vaccineCatalog.SourceUrl);
        }
    }

    private async void OnVaccineTapped(object? sender, TappedEventArgs e) => await OpenVaccineRecordAsync(sender);

    private async void OnOpenRecordClicked(object? sender, EventArgs e) => await OpenVaccineRecordAsync(sender);

    //Opens the record editor for the vaccine's row: a new record if this baby has none for it yet, otherwise the existing one.
    //The record is looked up by baby and vaccine Id at tap time (ADR-007), so a record saved a moment ago is found.
    private async Task OpenVaccineRecordAsync(object? sender)
    {
        if (_isOpeningRecord || Navigation.ModalStack.Count > 0)
        {
            return;
        }

        if (sender is not BindableObject { BindingContext: VaccineRow { Vaccine: var vaccine } } || BindingContext is not BabyModel babyProfile)
        {
            return;
        }

        VaccinationRecordModel? record = _vaccineService.GetRecordsForBaby(_babyId).FirstOrDefault(x => x.VaccineId == vaccine.Id);

        _isOpeningRecord = true;

        try
        {
            await Navigation.PushModalAsync(new AddVaccineRecordPage(_vaccineService, vaccine, _babyId, babyProfile.Name, record));
        }
        finally
        {
            _isOpeningRecord = false;
        }
    }

    private async void OnEditClicked(object? sender, EventArgs e)
    {
        if (BindingContext is not BabyModel babyProfile)
        {
            return;
        }

        await Navigation.PushModalAsync(new AddBabyPage(_babyProfileService, babyProfile));
    }

    private async void OnDeleteClicked(object? sender, EventArgs e)
    {
        if (BindingContext is not BabyModel babyProfile)
        {
            return;
        }

        bool shouldDelete = await DisplayAlertAsync(
            "Delete profile?",
            $"Remove {babyProfile.Name}'s profile? You can always add it back later.",
            "Delete",
            "Keep");

        if (!shouldDelete)
        {
            return;
        }

        try
        {
            await _babyProfileService.RemoveAsync(babyProfile.Id);
        }
        catch (DbCommunicationException) //Nothing was removed, including the links from its tasks, so the profile stays open
        {
            await AlertService.ShowWriteFailedAsync(this, $"Couldn't remove {babyProfile.Name}'s profile", "The profile and its linked tasks haven't changed. Please try again in a moment.");
            return;
        }

        //The baby no longer exists, so the user goes back to MainPage instead of staying on its profile
        await Navigation.PopAsync();
        ToastService.Show(ToastKind.BabyProfileDeleted);
    }
}
