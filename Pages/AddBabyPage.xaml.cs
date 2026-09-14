using BabyBuddyHelper.Interfaces;
using BabyBuddyHelper.Models;
using System.Globalization;

namespace BabyBuddyHelper.Pages;

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
        AgeEntry.Text = babyProfileOnEdit.Age.ToString(CultureInfo.InvariantCulture);
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
        var validatedProfile = await ValidateForm();

        if (validatedProfile is null)
        {
            return;
        }

        if (_babyProfileOnEdit is null)
        {
            _babyProfileService.Add(validatedProfile);
        }
        else
        {
            var updatedProfile = new BabyModel
            {
                Id = _babyProfileOnEdit.Id,
                Name = validatedProfile.Name,
                Age = validatedProfile.Age,
                WeightInLbs = validatedProfile.WeightInLbs,
                HeightInFt = validatedProfile.HeightInFt,
                LastFeed = validatedProfile.LastFeed,
                LastSleep = validatedProfile.LastSleep
            };
            _babyProfileService.Update(updatedProfile);
        }

        await Navigation.PopModalAsync();
    }

    private async Task<BabyModel?> ValidateForm()
    {
        if (string.IsNullOrWhiteSpace(NameEntry.Text))
        {
            await DisplayAlertAsync("Required Field Missing", "Please add your baby's name.", "OK");
            return null;
        }

        if (!int.TryParse(AgeEntry.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var age) || age < 0)
        {
            await DisplayAlertAsync("Invalid Age", "Please enter a whole number for age.", "OK");
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

        var lastFeed = CombineDateAndTime(LastFeedDatePicker.Date.GetValueOrDefault(), LastFeedTimePicker.Time.GetValueOrDefault());
        var lastSleep = CombineDateAndTime(LastSleepDatePicker.Date.GetValueOrDefault(), LastSleepTimePicker.Time.GetValueOrDefault());

        return new BabyModel
        {
            Name = NameEntry.Text.Trim(),
            Age = age,
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

    private static DateTime CombineDateAndTime(DateTime date, TimeSpan time)
    {
        return DateTime.SpecifyKind(date.Date + time, DateTimeKind.Local);
    }
}
