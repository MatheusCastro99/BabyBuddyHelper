using BabyBuddyHelper.Core.Interfaces;
using BabyBuddyHelper.UI.Controls;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Extensions;
using Microsoft.Maui.Controls.Shapes;

namespace BabyBuddyHelper.UI.Pages;

//The chat with Cub: a pushed page opened from the companion card (ADR-016). It only ever asks the key store whether
//a key exists; the key itself is never read here. The conversation lives in this page and goes away with it.
public partial class ChatPage : ContentPage
{
    private readonly IApiKeyStore _apiKeyStore;
    private bool _hasKey;
    private bool _isSettingsOpen;

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
        _hasKey = await _apiKeyStore.HasKeyAsync();

        SetupState.IsVisible = !_hasKey;
        ChatState.IsVisible = _hasKey;
    }

    //The gear and "Add my key" both land here. The popup is shown as a modal page, so closing it brings OnAppearing back.
    //The flag keeps a double-click from stacking a second popup while the first is opening or open.
    private async void OnOpenSettingsClicked(object? sender, EventArgs e)
    {
        if (_isSettingsOpen)
        {
            return;
        }

        _isSettingsOpen = true;

        try
        {
            await this.ShowPopupAsync(new CubSettingsPopup(_apiKeyStore, this, _hasKey), new PopupOptions
            {
                Shape = new RoundRectangle { CornerRadius = 24, StrokeThickness = 0 },
                Shadow = new Shadow { Brush = Colors.Black, Opacity = 0.18f, Radius = 24, Offset = new Point(0, 6) }, //Softer than the toolkit's default
            });
        }
        finally
        {
            _isSettingsOpen = false;
        }
    }
}
