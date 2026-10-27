using BabyBuddyHelper.Core.Exceptions;
using BabyBuddyHelper.Core.Interfaces.Cub;
using BabyBuddyHelper.Core.Interfaces;
using BabyBuddyHelper.Core.Models;
using BabyBuddyHelper.UI.Services;
using CommunityToolkit.Maui.Views;

namespace BabyBuddyHelper.UI.Controls
{
    /// <summary>
    /// Cub's settings, opened from the gear on the chat. The user enters, replaces or removes their own
    /// Claude API key here. A saved key is never read back: the popup only knows whether one exists.
    /// The parent also picks how Cub talks in the chat (#97); each choice is saved the moment it is picked.
    /// </summary>
    public partial class CubSettingsPopup : Popup
    {
        static readonly Uri HowToGetKeyUrl = new("https://platform.claude.com/settings/keys");

        //Neutral and in the app's voice, not Cub's (ADR-013). It lists exactly what BabyChatContext and the chat send (#96)
        const string ConsentTitle = "Before Cub can chat";
        const string ConsentMessage =
            "When you write to Cub, this goes to Anthropic, the company behind Claude, using your key:\n"
            + "• your messages in this chat\n"
            + "• if you pick a baby: their name, age, height, weight and vaccines\n"
            + "Feeds and sleeps are never sent. Anthropic bills your key for each message (Cub's reply and a quick check of it). This app doesn't keep the chat once you close it.";

        readonly IApiKeyStore _apiKeyStore;
        readonly ICubPersonalityStore _personalityStore;
        readonly Page _hostPage; //Alerts and confirmations are raised from the page the popup sits on
        readonly bool _hasKey;
        bool _isBusy;
        CubPersonality _personality;

        public CubSettingsPopup(IApiKeyStore apiKeyStore, ICubPersonalityStore personalityStore, Page hostPage, bool hasKey)
        {
            InitializeComponent();

            _apiKeyStore = apiKeyStore;
            _personalityStore = personalityStore;
            _hostPage = hostPage;
            _hasKey = hasKey;

            ShowState(isEntering: !hasKey);

            //Set before the options are checked, so showing the saved choice doesn't save it again
            _personality = personalityStore.Load();
            CheckOption(StyleOptions, _personality.Style.ToString());
            CheckOption(LengthOptions, _personality.Verbosity.ToString());
        }

        static void CheckOption(Layout options, string value)
        {
            foreach (RadioButton option in options.Children.OfType<RadioButton>())
            {
                option.IsChecked = Equals(option.Value, value);
            }
        }

        //Saved at once: the parent's next message uses it, and closing the popup any way keeps it
        void OnStyleChanged(object? sender, CheckedChangedEventArgs e)
        {
            if (e.Value && sender is RadioButton { Value: string name } && Enum.TryParse(name, out CubStyle style) && style != _personality.Style)
            {
                SavePersonality(_personality with { Style = style });
            }
        }

        void OnLengthChanged(object? sender, CheckedChangedEventArgs e)
        {
            if (e.Value && sender is RadioButton { Value: string name } && Enum.TryParse(name, out CubVerbosity verbosity) && verbosity != _personality.Verbosity)
            {
                SavePersonality(_personality with { Verbosity = verbosity });
            }
        }

        void SavePersonality(CubPersonality personality)
        {
            _personality = personality;
            _personalityStore.Save(personality);
        }

        //Cancel only exists while replacing: it goes back to the key that is still saved
        void ShowState(bool isEntering)
        {
            EntryState.IsVisible = isEntering;
            StoredState.IsVisible = !isEntering;
            CancelReplaceButton.IsVisible = isEntering && _hasKey;
        }

        void OnKeyTextChanged(object? sender, TextChangedEventArgs e)
        {
            SaveButton.IsEnabled = !string.IsNullOrWhiteSpace(e.NewTextValue);
            FormatMessage.IsVisible = false; //The hint goes away as soon as the user starts fixing the key
        }

        async void OnSaveClicked(object? sender, EventArgs e)
        {
            string key = KeyEntry.Text ?? string.Empty;

            if (_isBusy || string.IsNullOrWhiteSpace(key))
            {
                return;
            }

            if (!_apiKeyStore.IsValidFormat(key))
            {
                FormatMessage.IsVisible = true;
                SemanticScreenReader.Announce(FormatMessage.Text);
                return;
            }

            _isBusy = true;

            try
            {
                //Every save asks first, a replace included: the user is told what is sent before anything can be (ADR-017).
                //"Not now" stores nothing and leaves the popup as it was
                if (!await _hostPage.DisplayAlertAsync(ConsentTitle, ConsentMessage, "Save key", "Not now"))
                {
                    return;
                }

                await _apiKeyStore.SaveKeyAsync(key);
            }
            catch (KeyStorageException) //Nothing was stored, so a key saved earlier is still the one in place
            {
                await AlertService.ShowKeyStorageFailedAsync(_hostPage, "Couldn't save your key");
                return;
            }
            finally
            {
                _isBusy = false;
            }

            KeyEntry.Text = string.Empty; //The key leaves the screen the moment it is stored
            await CloseQuietlyAsync();
            ToastService.Show(ToastKind.ApiKeySaved);
        }

        //The saved key stays in place until a new one is saved, so backing out of a replace loses nothing
        async void OnReplaceClicked(object? sender, EventArgs e)
        {
            if (!await ConfirmKeyChangeAsync("Replace your key?", "Your saved key stays in place until you save a new one.", "Replace"))
            {
                return;
            }

            ShowState(isEntering: true);
            KeyEntry.Focus();
        }

        void OnCancelReplaceClicked(object? sender, EventArgs e)
        {
            KeyEntry.Text = string.Empty;
            ShowState(isEntering: false);
        }

        async void OnRemoveClicked(object? sender, EventArgs e)
        {
            if (!await ConfirmKeyChangeAsync("Remove your key?", "Cub won't be able to chat until you add a key again. The key is only removed from this device.", "Remove"))
            {
                return;
            }

            _isBusy = true;

            try
            {
                await _apiKeyStore.RemoveKeyAsync();
            }
            catch (KeyStorageException) //Nothing was removed, so the key is still saved
            {
                await AlertService.ShowKeyStorageFailedAsync(_hostPage, "Couldn't remove your key");
                return;
            }
            finally
            {
                _isBusy = false;
            }

            await CloseQuietlyAsync();
            ToastService.Show(ToastKind.ApiKeyRemoved);
        }

        //One confirmation for both changes to a saved key. The flag keeps a second tap from stacking a second dialog
        async Task<bool> ConfirmKeyChangeAsync(string title, string message, string accept)
        {
            if (_isBusy)
            {
                return false;
            }

            _isBusy = true;

            try
            {
                return await _hostPage.DisplayAlertAsync(title, message, accept, "Keep");
            }
            finally
            {
                _isBusy = false;
            }
        }

        async void OnHowToGetKeyClicked(object? sender, EventArgs e)
        {
            bool opened;

            try
            {
                opened = await Browser.Default.OpenAsync(HowToGetKeyUrl, BrowserLaunchMode.SystemPreferred);
            }
            catch (Exception) //Platforms throw different exceptions when no browser can take the link; all of them mean it didn't open
            {
                opened = false;
            }

            if (!opened)
            {
                await AlertService.ShowLinkFailedAsync(_hostPage, HowToGetKeyUrl);
            }
        }

        async void OnCloseClicked(object? sender, EventArgs e) => await CloseQuietlyAsync();

        //Tapping outside or the Android back button can close the popup while a save or remove is still running.
        //Closing it a second time throws, and from an event handler that would take the app down
        async Task CloseQuietlyAsync()
        {
            try
            {
                await CloseAsync();
            }
            catch (InvalidPopupOperationException)
            {
                //Already closed: there is nothing left to do
            }
        }
    }
}
