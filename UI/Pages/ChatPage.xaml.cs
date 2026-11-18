using BabyBuddyHelper.Core.Exceptions;
using BabyBuddyHelper.Core.Interfaces.Baby;
using BabyBuddyHelper.Core.Interfaces.Cub;
using BabyBuddyHelper.Core.Interfaces.Vaccine;
using BabyBuddyHelper.Core.Interfaces;
using BabyBuddyHelper.Core.Models.Baby;
using BabyBuddyHelper.Core.Models.Cub;
using BabyBuddyHelper.Core.Services.Cub;
using BabyBuddyHelper.UI.Controls;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Extensions;
using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;
using Microsoft.Maui.Controls.Shapes;
using System.Collections.ObjectModel;

namespace BabyBuddyHelper.UI.Pages;

//The chat with Cub: a pushed page opened from the companion card (ADR-016). It only ever asks the key store whether
//a key exists; the key itself is never read here. The conversation lives in this page and goes away with it.
//Replies come through CubReplyService, which checks each one before it is shown (ADR-020), over ICubChatClient:
//the page never references Remote/ or the provider SDK (ADR-017).
public partial class ChatPage : ContentPage
{
    private const double MinimumConversationHeight = 160; //The least room the list keeps before the page scrolls instead
    private const string GeneralAdvice = "General advice";
    private const string ThinkingText = "Cub is thinking…";
    private const string FailureText = "Cub couldn't answer just now. Check your connection and key, then try again.";
    private const string DeclinedText = "Cub couldn't find a good answer to that. Try asking another way."; //Both tries failed the checks (ADR-020)

    private readonly IApiKeyStore _apiKeyStore;
    private readonly CubReplyService _cubReplyService;
    private readonly ICubPersonalityStore _personalityStore;
    private readonly IBabyProfileService _babyProfileService;
    private readonly IVaccineService _vaccineService;
    private readonly IVaccineCatalog _vaccineCatalog;
    private readonly Dictionary<string, Guid?> _babyOptions; //Picker label to baby Id; "General advice" is null
    private readonly ObservableCollection<ChatItem> _items = []; //Never stored: a new ChatPage starts with an empty conversation
    private readonly List<ChatTurn> _history = []; //What Cub is sent: answered messages and their replies only, never a failed one
    private readonly CancellationTokenSource _closing = new(); //Cancelled when the page leaves the stack, so a late reply is dropped
    private bool _isSettingsOpen;
    private bool _isWaitingForCub;

    public ChatPage(IApiKeyStore apiKeyStore, CubReplyService cubReplyService, ICubPersonalityStore personalityStore, IBabyProfileService babyProfileService,
        IBabyFilterService babyFilterService, IVaccineService vaccineService, IVaccineCatalog vaccineCatalog)
    {
        InitializeComponent();

        _apiKeyStore = apiKeyStore;
        _cubReplyService = cubReplyService;
        _personalityStore = personalityStore;
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

        //Before the list shows, the page grid gets the screen's height, so the list never measures against an unbounded one
        if (hasKey && PageScroll.Height > 0 && PageGrid.HeightRequest < 0)
        {
            PageGrid.HeightRequest = PageScroll.Height;
        }

        SetupState.IsVisible = !hasKey;
        ChatState.IsVisible = hasKey;
        TalkingAboutLabel.IsVisible = hasKey;
        BabyPicker.IsVisible = hasKey;
        LayOutCubHeading();
        Dispatcher.Dispatch(SizeConversation);
    }

    //The layout switches run from a dispatch, not inside the size change that asked for them: a change made while the page is being laid out
    //doesn't always reach the rows above it, which then keep their old height. Both read widths only, so running again changes nothing
    private void OnCubCardSizeChanged(object? sender, EventArgs e) => Dispatcher.Dispatch(LayOutCubHeading);

    private void OnBabyPickerSelectedIndexChanged(object? sender, EventArgs e) => LayOutCubHeading(); //A longer name may no longer fit

    private void OnPageSizeChanged(object? sender, EventArgs e) => Dispatcher.Dispatch(SizeConversation);

    //The page grid is given a definite height, and the conversation (a star row of it) takes what the card, the message box and the
    //disclaimer leave. That height is the screen's, so at normal sizes the message box sits at the bottom and nothing scrolls, and it
    //follows a window resize both ways. When the rest of the page plus a least-height conversation is taller than the screen (large
    //text), the grid takes that height instead and the page scrolls, so nothing overlaps or runs off the screen.
    //What the rest of the page needs is the grid's height without the list. Both are read from the same finished layout, and nothing
    //is changed while the grid's height is not yet the one asked for, so a layout that is still catching up can't make it flip
    private void SizeConversation()
    {
        if (!ChatState.IsVisible)
        {
            if (PageGrid.HeightRequest >= 0)
            {
                PageGrid.HeightRequest = -1; //The setup state has nothing to fill
            }

            return;
        }

        double viewport = PageScroll.Height;

        if (viewport <= 0)
        {
            return;
        }

        double height = viewport;

        if (PageGrid.HeightRequest >= 0)
        {
            if (Math.Abs(PageGrid.Height - PageGrid.HeightRequest) >= 1 || ConversationView.Height < 0)
            {
                return;
            }

            double restOfPage = PageGrid.Height - ConversationView.Height;
            height = Math.Max(viewport, Math.Ceiling(restOfPage + MinimumConversationHeight));
        }

        if (Math.Abs(height - PageGrid.HeightRequest) >= 1)
        {
            PageGrid.HeightRequest = height;
        }
    }

    //"Cub – is talking about: [picker]" stays on one line while it fits. On a narrow card (a phone) the picker moves onto its
    //own line under it, instead of being squeezed out of sight, and the tagline moves down one row. The label then also
    //spans the free column, so on the narrowest phones (or with larger text) it wraps instead of running under the gear.
    //With larger text the picker alone can be wider than the column beside Cub and the gear: the whole heading then moves under them,
    //across the card, where the picker has the card's full width. The choices come from the card's width and the natural widths,
    //never from the current layout, so they can't flip back and forth
    private void LayOutCubHeading()
    {
        //Without a key there is only "Cub" and the tagline, so everything goes back to its one-row place (a key removed in the settings
        //popup can leave the heading under Cub and the gear from an earlier layout)
        if (!BabyPicker.IsVisible)
        {
            PlaceCubHeading(headingBelow: false, fits: true);
            return;
        }

        if (CubCardGrid.Width <= 0)
        {
            return;
        }

        //The picker is measured at its natural width, so a cap from an earlier layout can't make a longer name look like it fits
        BabyPicker.MaximumWidthRequest = double.PositiveInfinity;

        double columnWidth = CubCardGrid.Width - CubIconFrame.WidthRequest - SettingsButton.WidthRequest - 2 * CubCardGrid.ColumnSpacing;
        double nameWidth = CubName.Measure(double.PositiveInfinity, double.PositiveInfinity).Width;
        double labelWidth = TalkingAboutLabel.Measure(double.PositiveInfinity, double.PositiveInfinity).Width;
        double pickerWidth = BabyPicker.Measure(double.PositiveInfinity, double.PositiveInfinity).Width;
        bool headingBelow = pickerWidth > columnWidth;
        double headingWidth = headingBelow ? CubCardGrid.Width : columnWidth;
        double lineWidth = nameWidth + labelWidth + pickerWidth + 2 * CubHeading.ColumnSpacing;
        bool fits = lineWidth <= headingWidth;

        //The picker never gets more than the room it has: the whole heading width on its own line, what is left beside the label
        //inline. Only a name longer than the whole card is cut short inside the picker, instead of running past the card
        BabyPicker.MaximumWidthRequest = fits
            ? Math.Max(0, headingWidth - nameWidth - labelWidth - 2 * CubHeading.ColumnSpacing)
            : headingWidth;

        PlaceCubHeading(headingBelow, fits);
    }

    //headingBelow: the whole heading sits in a second row across the card. fits: the picker is on the heading's first line
    private void PlaceCubHeading(bool headingBelow, bool fits)
    {
        Grid.SetRow(CubHeading, headingBelow ? 1 : 0);
        Grid.SetColumn(CubHeading, headingBelow ? 0 : 1);
        Grid.SetColumnSpan(CubHeading, headingBelow ? 3 : 1);
        CubCardGrid.RowSpacing = headingBelow ? 10 : 0;

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

    //Shows the message, then "Cub is thinking…" until the checked reply, or a failure line, takes its place
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

        CubOutcome? outcome;

        try
        {
            //The style and length are read at send time, like the baby picker, so a change in the popup applies from the next message
            outcome = await _cubReplyService.AskAsync([.. _history, message], GetPickedBabyContext(), _personalityStore.Load(), _closing.Token);
        }
        catch (OperationCanceledException) //The page was closed: there is no one left to show the reply to
        {
            return;
        }
        catch (CubUnavailableException) //No network, no key, Claude refused, or the grader couldn't answer. Nothing local was touched
        {
            outcome = null;
        }

        if (_closing.IsCancellationRequested)
        {
            return;
        }

        _items.Remove(thinking);

        //Only a checked reply joins the history; a failed or declined message is never sent again
        if (outcome is CubOutcome.Reply reply)
        {
            _history.Add(message);
            _history.Add(new ChatTurn(ChatSpeaker.Cub, reply.Text));
            _items.Add(new ChatItem(ChatItemKind.Cub, reply.Text));
        }
        else
        {
            string line = outcome is CubOutcome.Declined ? DeclinedText : FailureText;
            _items.Add(new ChatItem(ChatItemKind.Failure, line));
            SemanticScreenReader.Announce(line);
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

            await this.ShowPopupAsync(new CubSettingsPopup(_apiKeyStore, _personalityStore, this, hasKey), new PopupOptions
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
