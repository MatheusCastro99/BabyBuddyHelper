using BabyBuddyHelper.Core.Exceptions;
using BabyBuddyHelper.Core.Interfaces;

namespace BabyBuddyHelper.Core.Storage
{
    //The user's Claude API key, kept in the device's secure storage (ADR-010): the Android Keystore-backed preferences,
    //the iOS Keychain, or a DPAPI-encrypted file on Windows. It never goes to Preferences, the database or a log.
    public sealed class SecureApiKeyStore : IApiKeyStore
    {
        private const string StorageName = "claude_api_key";
        private const string ClaudeKeyPrefix = "sk-ant-";

        //Catches a half-copied key or a key from another provider. Whether Claude accepts it is only known once it is used (#96)
        public bool IsValidFormat(string? key)
        {
            string trimmed = key?.Trim() ?? string.Empty;

            return trimmed.Length > ClaudeKeyPrefix.Length
                && trimmed.StartsWith(ClaudeKeyPrefix, StringComparison.Ordinal)
                && !trimmed.Any(char.IsWhiteSpace);
        }

        public async Task<bool> HasKeyAsync() => !string.IsNullOrEmpty(await GetKeyAsync());

        //A stored value the device can't read right now is treated as "no key", so the user is asked for the key again
        //instead of seeing an error. It is deliberately not removed: a read can fail for a passing reason, and deleting
        //a good key would cost the user a new one. A value that really is unreadable is overwritten by the next save.
        public async Task<string?> GetKeyAsync()
        {
            try
            {
                return await SecureStorage.Default.GetAsync(StorageName);
            }
            catch (Exception) //Platforms throw different exceptions for an unreadable value; all of them mean there is no usable key
            {
                return null;
            }
        }

        public async Task SaveKeyAsync(string key)
        {
            string trimmed = key.Trim();
            string? stored;

            try
            {
                await SecureStorage.Default.SetAsync(StorageName, trimmed);
                stored = await SecureStorage.Default.GetAsync(StorageName);
            }
            catch (Exception ex)
            {
                throw new KeyStorageException("The device's secure storage couldn't save the key.", ex);
            }

            //On Android a write is skipped without an error when the encrypted storage can't be opened,
            //so the key is read back: the user is only told it was saved when it really is there
            if (stored != trimmed)
            {
                throw new KeyStorageException("The device's secure storage didn't keep the key.");
            }
        }

        public Task RemoveKeyAsync()
        {
            try
            {
                SecureStorage.Default.Remove(StorageName);
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw new KeyStorageException("The device's secure storage couldn't remove the key.", ex);
            }
        }
    }
}
