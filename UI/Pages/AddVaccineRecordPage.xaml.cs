using BabyBuddyHelper.Core.Exceptions;
using BabyBuddyHelper.Core.Interfaces;
using BabyBuddyHelper.Core.Models;
using BabyBuddyHelper.UI.Services;

namespace BabyBuddyHelper.UI.Pages;

//The single editor for one baby's record of one vaccine (ADR-004 pattern). Opened from a vaccine in BabyProfilePage's Vaccines section:
//it creates the record when there is none yet, or edits and removes the existing one.
public partial class AddVaccineRecordPage : ContentPage
{
    private const int MaxTotalDoses = 10;

    private readonly IVaccineService _vaccineService;
    private readonly VaccineModel _vaccine;
    private readonly Guid _babyId;
    private readonly string _babyName;
    private readonly VaccinationRecordModel? _recordOnEdit;

    //What the record held when the page opened, so the dose dates can follow the dose count (see OnCompletedDosesIncreaseClicked)
    private readonly int _storedCompletedDoses;
    private readonly DateTime? _storedLastAdministered;
    private readonly DateTime? _storedNextDose;

    private int _totalDoses = 1;
    private int _completedDoses;

    public AddVaccineRecordPage(IVaccineService vaccineService, VaccineModel vaccine, Guid babyId, string babyName, VaccinationRecordModel? recordOnEdit)
    {
        InitializeComponent();

        _vaccineService = vaccineService;
        _vaccine = vaccine;
        _babyId = babyId;
        _babyName = babyName;
        _recordOnEdit = recordOnEdit;

        VaccineNameLabel.Text = vaccine.Name;
        LastDosePicker.MaximumDate = DateTime.Today;
        LastDosePicker.Date = DateTime.Today;
        NextDosePicker.Date = DateTime.Today;

        if (recordOnEdit is not null)
        {
            _storedCompletedDoses = recordOnEdit.CompletedDoses;
            _storedLastAdministered = recordOnEdit.LastAdministered?.Date;
            _storedNextDose = recordOnEdit.NextDose?.Date;

            _totalDoses = recordOnEdit.TotalDoses ?? 1; //Seasonal records get their own editor behavior in P2 (#82)
            _completedDoses = recordOnEdit.CompletedDoses;
            LastDosePicker.Date = _storedLastAdministered ?? DateTime.Today;
            NextDoseCheckBox.IsChecked = _storedNextDose.HasValue;
            NextDosePicker.Date = _storedNextDose ?? DateTime.Today;

            SaveButton.Text = "Update";
            RemoveButton.IsVisible = true;
        }

        RefreshDoseState();
    }

    //Shows or hides the date sections to match the counts: no last date before the first dose, no next date once every dose is given
    private void RefreshDoseState()
    {
        bool isCompleted = _completedDoses >= _totalDoses;

        TotalDosesLabel.Text = _totalDoses.ToString();
        CompletedDosesLabel.Text = _completedDoses.ToString();

        //Total can't drop below the doses already given; lower "Doses given" first
        TotalDosesDecreaseButton.IsEnabled = _totalDoses > Math.Max(1, _completedDoses);
        TotalDosesIncreaseButton.IsEnabled = _totalDoses < MaxTotalDoses;
        CompletedDosesDecreaseButton.IsEnabled = _completedDoses > 0;
        CompletedDosesIncreaseButton.IsEnabled = _completedDoses < _totalDoses;

        LastDoseSection.IsVisible = _completedDoses > 0;
        NextDoseSection.IsVisible = !isCompleted;
        NextDosePickerBorder.IsVisible = NextDoseCheckBox.IsChecked;
        AllDosesGivenLabel.IsVisible = isCompleted;
    }

    private void OnTotalDosesDecreaseClicked(object? sender, EventArgs e)
    {
        if (_totalDoses > Math.Max(1, _completedDoses))
        {
            _totalDoses--;
            RefreshDoseState();
        }
    }

    private void OnTotalDosesIncreaseClicked(object? sender, EventArgs e)
    {
        if (_totalDoses < MaxTotalDoses)
        {
            _totalDoses++;
            RefreshDoseState();
        }
    }

    //Going one past the stored count means a new dose was given, so the last-dose date moves to today (the usual case: logging
    //right after the visit) and the stored next date is cleared: it was for the dose just logged, and keeping it would later read
    //as overdue. Coming back down to the stored count restores both stored dates. Any date picked in between is kept.
    private void OnCompletedDosesIncreaseClicked(object? sender, EventArgs e)
    {
        if (_completedDoses >= _totalDoses)
        {
            return;
        }

        _completedDoses++;

        if (_completedDoses == _storedCompletedDoses + 1)
        {
            LastDosePicker.Date = DateTime.Today;
            NextDoseCheckBox.IsChecked = false;
        }

        RefreshDoseState();
    }

    private void OnCompletedDosesDecreaseClicked(object? sender, EventArgs e)
    {
        if (_completedDoses <= 0)
        {
            return;
        }

        _completedDoses--;

        if (_completedDoses == _storedCompletedDoses)
        {
            if (_storedLastAdministered.HasValue)
            {
                LastDosePicker.Date = _storedLastAdministered.Value;
            }

            NextDoseCheckBox.IsChecked = _storedNextDose.HasValue;
            NextDosePicker.Date = _storedNextDose ?? DateTime.Today;
        }

        RefreshDoseState();
    }

    private void OnNextDoseCheckedChanged(object? sender, CheckedChangedEventArgs e)
    {
        RefreshDoseState();
    }

    //The label is part of the checkbox's tap target
    private void OnNextDoseLabelTapped(object? sender, TappedEventArgs e)
    {
        NextDoseCheckBox.IsChecked = !NextDoseCheckBox.IsChecked;
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
            await SaveRecord();
        }
        catch (DbCommunicationException) //Nothing was saved and the modal stays open, so the user can retry without re-entering
        {
            await AlertService.ShowWriteFailedAsync(this, $"Couldn't save the {_vaccine.Name} record", "Everything you entered is still here. Please try again in a moment.");
        }
        finally
        {
            SaveButton.IsEnabled = true;
        }
    }

    private async Task SaveRecord()
    {
        var validatedRecord = await ValidateForm();

        if (validatedRecord is null)
        {
            return;
        }

        if (_recordOnEdit is null)
        {
            await _vaccineService.AddAsync(validatedRecord);
            await Navigation.PopModalAsync();
            ToastService.Show(ToastKind.VaccineRecordAdded);
        }
        else
        {
            await _vaccineService.UpdateAsync(validatedRecord);
            await Navigation.PopModalAsync();
            ToastService.Show(ToastKind.VaccineRecordUpdated);
        }
    }

    //The counters already keep the counts in range; the checks stay so a bad record can never be saved
    private async Task<VaccinationRecordModel?> ValidateForm()
    {
        if (_totalDoses < 1 || _completedDoses < 0 || _completedDoses > _totalDoses)
        {
            await DisplayAlertAsync("Check the doses", "Doses given should be between 0 and the total number of doses.", "OK");
            return null;
        }

        DateTime? lastAdministered = null;

        if (_completedDoses > 0)
        {
            if (LastDosePicker.Date is not { } lastDoseDate)
            {
                await DisplayAlertAsync("Last dose date missing", "Please pick the day the last dose was given.", "OK");
                return null;
            }

            if (lastDoseDate.Date > DateTime.Today)
            {
                await DisplayAlertAsync("Check the date", "The last dose date can't be in the future. Please pick today or an earlier day.", "OK");
                return null;
            }

            lastAdministered = DateTime.SpecifyKind(lastDoseDate.Date, DateTimeKind.Local);
        }

        //A complete record needs no next date, so a date picked before the last dose was counted is dropped
        DateTime? nextDose = null;
        bool isCompleted = _completedDoses >= _totalDoses;

        if (!isCompleted && NextDoseCheckBox.IsChecked && NextDosePicker.Date is { } nextDoseDate)
        {
            nextDose = DateTime.SpecifyKind(nextDoseDate.Date, DateTimeKind.Local);
        }

        //Editing keeps the record's Id (ADR-007); the service swaps the cached instance for this one
        return new VaccinationRecordModel
        {
            Id = _recordOnEdit?.Id ?? Guid.NewGuid(),
            BabyId = _babyId,
            VaccineId = _vaccine.Id,
            TotalDoses = _totalDoses,
            CompletedDoses = _completedDoses,
            LastAdministered = lastAdministered,
            NextDose = nextDose
        };
    }

    private async void OnRemoveClicked(object? sender, EventArgs e)
    {
        if (_recordOnEdit is null)
        {
            return;
        }

        bool shouldRemove = await DisplayAlertAsync(
            "Remove this record?",
            $"The {_vaccine.Name} doses and dates for {_babyName} will be cleared. You can add them again anytime.",
            "Remove",
            "Keep");

        if (!shouldRemove)
        {
            return;
        }

        RemoveButton.IsEnabled = false;

        try
        {
            await _vaccineService.RemoveAsync(_recordOnEdit.Id);
        }
        catch (DbCommunicationException) //Nothing was removed, so the editor stays open
        {
            await AlertService.ShowWriteFailedAsync(this, $"Couldn't remove the {_vaccine.Name} record", "The record hasn't changed. Please try again in a moment.");
            return;
        }
        finally
        {
            RemoveButton.IsEnabled = true;
        }

        await Navigation.PopModalAsync();
        ToastService.Show(ToastKind.VaccineRecordRemoved);
    }
}
