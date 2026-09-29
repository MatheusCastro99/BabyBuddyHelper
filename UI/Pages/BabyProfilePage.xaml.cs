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
        ShowVaccineCatalog();
    }

    //Closing the editor brings this page back, so a saved edit shows here
    protected override void OnAppearing()
    {
        base.OnAppearing();

        ShowCurrentProfile();
    }

    //Found by Id every time (ADR-007): saving an edit replaces the cached instance, so a held reference would go stale
    private void ShowCurrentProfile()
    {
        BindingContext = _babyProfileService.BabyProfiles.FirstOrDefault(x => x.Id == _babyId);
    }

    //The catalog never changes, so the Vaccines section is filled once. The wording follows the disclaimer approved in #41.
    private void ShowVaccineCatalog()
    {
        BindableLayout.SetItemsSource(VaccineList, _vaccineCatalog.Vaccines);

        VaccineDisclaimerLabel.Text = $"This list follows the {_vaccineCatalog.SourceName} ({_vaccineCatalog.SourceVersion}). " +
            "It's here to help you keep track, not to give medical advice. Please confirm timing and doses with your pediatrician.";
        VaccineLastReviewedLabel.Text = $"Last reviewed: {_vaccineCatalog.LastReviewed:MMMM yyyy}";
    }

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

    //Opens the record editor for the tapped vaccine: a new record if this baby has none for it yet, otherwise the existing one.
    //The record is looked up by baby and vaccine Id at tap time (ADR-007), so a record saved a moment ago is found.
    private async void OnVaccineTapped(object? sender, TappedEventArgs e)
    {
        if (_isOpeningRecord || Navigation.ModalStack.Count > 0)
        {
            return;
        }

        if (sender is not BindableObject { BindingContext: VaccineModel vaccine } || BindingContext is not BabyModel babyProfile)
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
