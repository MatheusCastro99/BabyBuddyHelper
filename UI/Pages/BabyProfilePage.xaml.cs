using BabyBuddyHelper.Core.Exceptions;
using BabyBuddyHelper.Core.Interfaces;
using BabyBuddyHelper.Core.Models;
using BabyBuddyHelper.UI.Services;

namespace BabyBuddyHelper.UI.Pages;

//Read-only view of one baby, and the entry point for editing and deleting it. AddBabyPage stays the only editor (ADR-004).
public partial class BabyProfilePage : ContentPage
{
    private readonly IBabyProfileService _babyProfileService;
    private readonly Guid _babyId;

    public BabyProfilePage(IBabyProfileService babyProfileService, Guid babyId)
    {
        InitializeComponent();

        _babyProfileService = babyProfileService;
        _babyId = babyId;

        ShowCurrentProfile();
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
