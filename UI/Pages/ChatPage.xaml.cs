using BabyBuddyHelper.Core.Interfaces;

namespace BabyBuddyHelper.UI.Pages;

//The chat with Cub: a pushed page opened from the companion card (ADR-016). It only ever asks the key store whether
//a key exists; the key itself is never read here. The conversation lives in this page and goes away with it.
public partial class ChatPage : ContentPage
{
    private readonly IApiKeyStore _apiKeyStore;

    public ChatPage(IApiKeyStore apiKeyStore)
    {
        InitializeComponent();

        _apiKeyStore = apiKeyStore;
    }

    //Runs when the page opens and again when the settings popup closes, so a key saved or removed there shows here
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await ShowCurrentStateAsync();
    }

    private async Task ShowCurrentStateAsync()
    {
        bool hasKey = await _apiKeyStore.HasKeyAsync();

        SetupState.IsVisible = !hasKey;
        ChatState.IsVisible = hasKey;
    }

    private void OnOpenSettingsClicked(object? sender, EventArgs e)
    {
    }
}
