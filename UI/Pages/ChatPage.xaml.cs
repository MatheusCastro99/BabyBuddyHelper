using BabyBuddyHelper.Core.Interfaces;
using BabyBuddyHelper.UI.Controls;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Extensions;
using Microsoft.Maui.Controls.Shapes;
using System.Collections.ObjectModel;

namespace BabyBuddyHelper.UI.Pages;

//The chat with Cub: a pushed page opened from the companion card (ADR-016). It only ever asks the key store whether
//a key exists; the key itself is never read here. The conversation lives in this page and goes away with it.
public partial class ChatPage : ContentPage
{
    private readonly IApiKeyStore _apiKeyStore;
    private readonly ObservableCollection<ChatMessage> _messages = []; //Never stored: a new ChatPage starts with an empty conversation
    private bool _hasKey;
    private bool _isSettingsOpen;

    public ChatPage(IApiKeyStore apiKeyStore)
    {
        InitializeComponent();

        _apiKeyStore = apiKeyStore;
        ConversationView.ItemsSource = _messages;
    }

    //One message in the conversation. Only the user's side exists until Cub can answer (#96)
    public sealed record ChatMessage(string Text);

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

    private void OnMessageTextChanged(object? sender, TextChangedEventArgs e) =>
        SendButton.IsEnabled = !string.IsNullOrWhiteSpace(e.NewTextValue);

    //Adds the message to the conversation and keeps the box ready for the next one. Nothing is sent anywhere yet
    private void OnSendClicked(object? sender, EventArgs e)
    {
        string text = MessageEditor.Text?.Trim() ?? string.Empty;

        if (text.Length == 0)
        {
            return;
        }

        _messages.Add(new ChatMessage(text));

        MessageEditor.Text = string.Empty;

        ConversationView.ScrollTo(_messages.Count - 1, position: ScrollToPosition.End, animate: true);

        //Clearing the box disables Send, and on Windows a focused button that gets disabled hands focus to the next
        //control (the Back arrow), where the next key press would leave the chat. Focus returns to the box once that settles.
        //MAUI still believes the box is focused there, so Focus() would do nothing: the native control is focused directly.
        //On the other platforms tapping Send never takes focus from the box.
#if WINDOWS
        Dispatcher.Dispatch(() => (MessageEditor.Handler?.PlatformView as Microsoft.UI.Xaml.Controls.Control)?.Focus(Microsoft.UI.Xaml.FocusState.Programmatic));
#endif
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
