using BabyBuddyHelper.Core.Interfaces;

namespace BabyBuddyHelper.UI.Pages;

//Read-only view of one baby. AddBabyPage stays the only editor (ADR-004).
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

    //Found by Id every time (ADR-007): saving an edit replaces the cached instance, so a held reference would go stale
    private void ShowCurrentProfile()
    {
        BindingContext = _babyProfileService.BabyProfiles.FirstOrDefault(x => x.Id == _babyId);
    }
}
