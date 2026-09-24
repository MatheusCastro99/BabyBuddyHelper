using BabyBuddyHelper.Core.Exceptions;
using BabyBuddyHelper.Core.Interfaces;
using BabyBuddyHelper.Core.Models;
using BabyBuddyHelper.UI.Services;
using System.Globalization;

namespace BabyBuddyHelper.UI.Pages;

public partial class AddBabyPage : ContentPage
{
    private readonly IBabyProfileService _babyProfileService;
    private readonly BabyModel? _babyProfileOnEdit;

    public AddBabyPage(IBabyProfileService babyProfileService)
    {
        InitializeComponent();

        _babyProfileService = babyProfileService;
        ApplyDefaultValues();
    }

    public AddBabyPage(IBabyProfileService babyProfileService, BabyModel babyProfileOnEdit)
    {
        InitializeComponent();

        _babyProfileService = babyProfileService;
        _babyProfileOnEdit = babyProfileOnEdit;

        HeaderTitleLabel.Text = "Edit baby profile";
        SaveButton.Text = "Update";

        NameEntry.Text = babyProfileOnEdit.Name;
        DateOfBirthPicker.Date = babyProfileOnEdit.DateOfBirth.Date;
        WeightEntry.Text = babyProfileOnEdit.WeightInLbs.ToString("0.##", CultureInfo.InvariantCulture);
        HeightEntry.Text = babyProfileOnEdit.HeightInFt.ToString("0.##", CultureInfo.InvariantCulture);
        LastFeedDatePicker.Date = babyProfileOnEdit.LastFeed.Date;
        LastFeedTimePicker.Time = babyProfileOnEdit.LastFeed.TimeOfDay;
        LastSleepDatePicker.Date = babyProfileOnEdit.LastSleep.Date;
        LastSleepTimePicker.Time = babyProfileOnEdit.LastSleep.TimeOfDay;
    }

    private void ApplyDefaultValues()
    {
        var now = DateTime.Now;
        var recentSleep = now.AddHours(-2);

        DateOfBirthPicker.Date = now.Date;
        LastFeedDatePicker.Date = now.Date;
        LastFeedTimePicker.Time = now.TimeOfDay;
        LastSleepDatePicker.Date = recentSleep.Date;
        LastSleepTimePicker.Time = recentSleep.TimeOfDay;
    }

    private async void OnCancelClicked(object? sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        SaveButton.IsEnabled = false; //Blocks a second tap from saving twice while the save is in flight

        try
        {
            await SaveProfile();
        }
        catch (DbCommunicationException) //Nothing was saved and the modal stays open, so the user can retry without retyping
        {
            //Only reachable after validation, so the name is never empty here
            await AlertService.ShowWriteFailedAsync(this, $"Couldn't save {NameEntry.Text.Trim()}'s profile", "Everything you entered is still here. Please try again in a moment.");
        }
        finally
        {
            SaveButton.IsEnabled = true;
        }
    }

    private async Task SaveProfile()
    {
        var validatedProfile = await ValidateForm();

        if (validatedProfile is null)
        {
            return;
        }

        if (_babyProfileOnEdit is null)
        {
            await _babyProfileService.AddAsync(validatedProfile);
            await Navigation.PopModalAsync();
            ToastService.Show(ToastKind.BabyProfileCreated);
        }
        else
        {
            var updatedProfile = new BabyModel
            {
                Id = _babyProfileOnEdit.Id,
                Name = validatedProfile.Name,
                DateOfBirth = validatedProfile.DateOfBirth,
                WeightInLbs = validatedProfile.WeightInLbs,
                HeightInFt = validatedProfile.HeightInFt,
                LastFeed = validatedProfile.LastFeed,
                LastSleep = validatedProfile.LastSleep
            };
            await _babyProfileService.UpdateAsync(updatedProfile);
            await Navigation.PopModalAsync();
            ToastService.Show(ToastKind.BabyProfileUpdated);
        }
    }

    private async Task<BabyModel?> ValidateForm()
    {
        if (string.IsNullOrWhiteSpace(NameEntry.Text))
        {
            await DisplayAlertAsync("Required Field Missing", "Please add your baby's name.", "OK");
            return null;
        }

        if (!TryParsePositiveDouble(WeightEntry.Text, out var weightInLbs))
        {
            await DisplayAlertAsync("Invalid Weight", "Please enter a valid weight in pounds.", "OK");
            return null;
        }

        if (!TryParsePositiveDouble(HeightEntry.Text, out var heightInFt))
        {
            await DisplayAlertAsync("Invalid Height", "Please enter a valid height in feet.", "OK");
            return null;
        }

        var lastFeed = CombineDateAndTime(LastFeedDatePicker.Date, LastFeedTimePicker.Time);
        var lastSleep = CombineDateAndTime(LastSleepDatePicker.Date, LastSleepTimePicker.Time);

        return new BabyModel
        {
            Name = NameEntry.Text.Trim(),
            DateOfBirth = CombineDateAndTime(DateOfBirthPicker.Date, TimeSpan.Zero),
            WeightInLbs = weightInLbs,
            HeightInFt = heightInFt,
            LastFeed = lastFeed,
            LastSleep = lastSleep
        };
    }

    private static bool TryParsePositiveDouble(string? value, out double parsedValue)
    {
        var styles = NumberStyles.Float | NumberStyles.AllowThousands;

        var didParse = double.TryParse(value, styles, CultureInfo.CurrentCulture, out parsedValue)
            || double.TryParse(value, styles, CultureInfo.InvariantCulture, out parsedValue);

        return didParse && parsedValue >= 0;
    }

    private static DateTime CombineDateAndTime(DateTime? date, TimeSpan? time)
    {
        var resolvedDate = date ?? DateTime.Now.Date;
        var resolvedTime = time ?? TimeSpan.Zero;
        return DateTime.SpecifyKind(resolvedDate.Date + resolvedTime, DateTimeKind.Local);
    }
}
