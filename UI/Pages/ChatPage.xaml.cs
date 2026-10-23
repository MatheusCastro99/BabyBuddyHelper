using BabyBuddyHelper.Core.Exceptions;
using BabyBuddyHelper.Core.Interfaces;
using BabyBuddyHelper.Core.Models;
using BabyBuddyHelper.Core.Services;
using BabyBuddyHelper.UI.Controls;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Extensions;
using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;
using Microsoft.Maui.Controls.Shapes;
using System.Collections.ObjectModel;

namespace BabyBuddyHelper.UI.Pages;

//The chat with Cub: a pushed page opened from the companion card (ADR-016). It only ever asks the key store whether
//a key exists; the key itself is never read here. The conversation lives in this page and goes away with it.
//Replies come through ICubChatClient only: the page never references Remote/ or the provider SDK (ADR-017).
public partial class ChatPage : ContentPage
{
    private const string GeneralAdvice = "General advice";
    private const string ThinkingText = "Cub is thinking…";
    private const string FailureText = "Cub couldn't answer just now. Check your connection and key, then try again.";

    private readonly IApiKeyStore _apiKeyStore;
    private readonly ICubChatClient _cubChatClient;
    private readonly IBabyProfileService _babyProfileService;
    private readonly IVaccineService _vaccineService;
    private readonly IVaccineCatalog _vaccineCatalog;
    private readonly Dictionary<string, Guid?> _babyOptions; //Picker label to baby Id; "General advice" is null
    private readonly ObservableCollection<ChatItem> _items = []; //Never stored: a new ChatPage starts with an empty conversation
    private readonly List<ChatTurn> _history = []; //What Cub is sent: answered messages and their replies only, never a failed one
    private readonly CancellationTokenSource _closing = new(); //Cancelled when the page leaves the stack, so a late reply is dropped
    private bool _isSettingsOpen;
    private bool _isWaitingForCub;

    public ChatPage(IApiKeyStore apiKeyStore, ICubChatClient cubChatClient, IBabyProfileService babyProfileService,
        IBabyFilterService babyFilterService, IVaccineService vaccineService, IVaccineCatalog vaccineCatalog)
    {
        InitializeComponent();

        _apiKeyStore = apiKeyStore;
        _cubChatClient = cubChatClient;
        _babyProfileService = babyProfileService;
        _vaccineService = vaccineService;
        _vaccineCatalog = vaccineCatalog;
        ConversationView.ItemsSource = _items;

        //The same option list as the other baby selectors, so two babies with one name stay apart
        _babyOptions = babyFilterService.BuildOptions(GeneralAdvice);
        BabyPicker.ItemsSource = _babyOptions.Keys.ToList();
        BabyPicker.SelectedIndex = 0;
    }

    //Runs when the page opens and again when the settings popup closes, so a key saved or removed there shows here
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        SetSoftInputMode(WindowSoftInputModeAdjust.Resize);
        await ShowCurrentStateAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        SetSoftInputMode(WindowSoftInputModeAdjust.Pan);
    }

    //Leaving the chat ends it: a reply still on its way is cancelled and never shown. Only a navigation that takes the page
    //off the stack counts: the settings popup is a modal push, and a tab switch (Replace) keeps the chat for when it returns
    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        base.OnNavigatedFrom(args);

        if (args.NavigationType is NavigationType.Pop or NavigationType.PopToRoot or NavigationType.Remove)
        {
            _closing.Cancel();
        }
    }

    //Android only (the call does nothing elsewhere). The app's default slides the whole window up for the keyboard, which
    //cuts off the chat's header and hides the line under the message box. While the chat is showing, the page resizes
    //above the keyboard instead; the other pages get the default back when it goes away.
    private static void SetSoftInputMode(WindowSoftInputModeAdjust mode) =>
        Microsoft.Maui.Controls.Application.Current?.On<Microsoft.Maui.Controls.PlatformConfiguration.Android>().UseWindowSoftInputModeAdjust(mode);

    private async Task ShowCurrentStateAsync()
    {
        bool hasKey = await _apiKeyStore.HasKeyAsync();

        SetupState.IsVisible = !hasKey;
        ChatState.IsVisible = hasKey;
        TalkingAboutLabel.IsVisible = hasKey;
        BabyPicker.IsVisible = hasKey;
        LayOutCubHeading();
    }

    private void OnCubHeadingSizeChanged(object? sender, EventArgs e) => LayOutCubHeading();

    private void OnBabyPickerSelectedIndexChanged(object? sender, EventArgs e) => LayOutCubHeading(); //A longer name may no longer fit

    //"Cub – is talking about: [picker]" stays on one line while it fits. On a narrow card (a phone) the picker moves onto its
    //own line under it, instead of being squeezed out of sight, and the tagline moves down one row. The label then also
    //spans the free column, so on the narrowest phones (or with larger text) it wraps instead of running under the gear
    private void LayOutCubHeading()
    {
        if (!BabyPicker.IsVisible || CubHeading.Width <= 0)
        {
            return;
        }

        double lineWidth = CubName.Measure(double.PositiveInfinity, double.PositiveInfinity).Width
            + TalkingAboutLabel.Measure(double.PositiveInfinity, double.PositiveInfinity).Width
            + BabyPicker.Measure(double.PositiveInfinity, double.PositiveInfinity).Width
            + 2 * CubHeading.ColumnSpacing;
        bool fits = lineWidth <= CubHeading.Width;

        Grid.SetRow(BabyPicker, fits ? 0 : 1);
        Grid.SetColumn(BabyPicker, fits ? 2 : 0);
        Grid.SetColumnSpan(BabyPicker, fits ? 1 : 3);
        Grid.SetColumnSpan(TalkingAboutLabel, fits ? 1 : 2);
        Grid.SetRow(CubTagline, fits ? 1 : 2);
    }

    private void OnMessageTextChanged(object? sender, TextChangedEventArgs e) => UpdateSendButton();

    //One message at a time: Send waits for Cub's reply, while the box stays free for the next message
    private void UpdateSendButton() =>
        SendButton.IsEnabled = !_isWaitingForCub && !string.IsNullOrWhiteSpace(MessageEditor.Text);

    //Shows the message, then "Cub is thinking…" until the reply, or the failure line, takes its place
    private async void OnSendClicked(object? sender, EventArgs e)
    {
        string text = MessageEditor.Text?.Trim() ?? string.Empty;

        if (text.Length == 0 || _isWaitingForCub)
        {
            return;
        }

        var message = new ChatTurn(ChatSpeaker.Parent, text);
        var thinking = new ChatItem(ChatItemKind.Pending, ThinkingText);

        _items.Add(new ChatItem(ChatItemKind.Parent, text));
        _items.Add(thinking);
        _isWaitingForCub = true;

        MessageEditor.Text = string.Empty; //Also disables Send, through UpdateSendButton

        ScrollToEnd();
        SemanticScreenReader.Announce(ThinkingText);

        //Clearing the box disables Send, and on Windows a focused button that gets disabled hands focus to the next
        //control (the Back arrow), where the next key press would leave the chat. Focus returns to the box once that settles.
        //MAUI still believes the box is focused there, so Focus() would do nothing: the native control is focused directly.
        //On the other platforms tapping Send never takes focus from the box.
#if WINDOWS
        Dispatcher.Dispatch(() => (MessageEditor.Handler?.PlatformView as Microsoft.UI.Xaml.Controls.Control)?.Focus(Microsoft.UI.Xaml.FocusState.Programmatic));
#endif

        string? reply;

        try
        {
            CubReply answer = await _cubChatClient.GetReplyAsync(CubGuidelines.TrimHistory([.. _history, message]),
                CubGuidelines.BuildSystemPrompt(GetPickedBabyContext(), CubPersonality.Default),
                CubGuidelines.MaxReplyTokensFor(CubPersonality.Default.Verbosity), _closing.Token);

            reply = answer.Text.Length == 0 ? null : answer.Text; //An empty reply shows the failure line, as before
        }
        catch (OperationCanceledException) //The page was closed: there is no one left to show the reply to
        {
            return;
        }
        catch (CubUnavailableException) //No network, no key, or Claude refused. Nothing local was touched
        {
            reply = null;
        }

        if (_closing.IsCancellationRequested)
        {
            return;
        }

        _items.Remove(thinking);

        if (reply is null)
        {
            _items.Add(new ChatItem(ChatItemKind.Failure, FailureText));
            SemanticScreenReader.Announce(FailureText);
        }
        else
        {
            _history.Add(message);
            _history.Add(new ChatTurn(ChatSpeaker.Cub, reply));
            _items.Add(new ChatItem(ChatItemKind.Cub, reply));
        }

        _isWaitingForCub = false;
        UpdateSendButton();
        ScrollToEnd();
    }

    private void ScrollToEnd() => ConversationView.ScrollTo(_items.Count - 1, position: ScrollToPosition.End, animate: true);

    //Read at send time, so a change to the picker applies from the next message. "General advice" shares nothing
    private BabyChatContext? GetPickedBabyContext()
    {
        if (BabyPicker.SelectedItem is not string label || _babyOptions.GetValueOrDefault(label) is not Guid babyId)
        {
            return null;
        }

        BabyModel? baby = _babyProfileService.BabyProfiles.FirstOrDefault(profile => profile.Id == babyId);

        return baby is null ? null : BabyChatContext.From(baby, _vaccineService.GetRecordsForBaby(babyId), _vaccineCatalog);
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
            //Asked here, not remembered from the page: the popup must open on what is stored right now
            bool hasKey = await _apiKeyStore.HasKeyAsync();

            await this.ShowPopupAsync(new CubSettingsPopup(_apiKeyStore, this, hasKey), new PopupOptions
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
