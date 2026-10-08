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

        //A stored value the device can no longer read (for example after a restore onto another device) is cleared
        //and treated as "no key", so the user is asked for the key again instead of seeing an error.
        public async Task<string?> GetKeyAsync()
        {
            try
            {
                return await SecureStorage.Default.GetAsync(StorageName);
            }
            catch (Exception) //Platforms throw different exceptions for an unreadable value; all of them mean there is no usable key
            {
                try
                {
                    SecureStorage.Default.Remove(StorageName);
                }
                catch (Exception)
                {
                    //Nothing more to do here: the key stays unusable and the next save reports the problem
                }

                return null;
            }
        }

        public async Task SaveKeyAsync(string key)
        {
            try
            {
                await SecureStorage.Default.SetAsync(StorageName, key.Trim());
            }
            catch (Exception ex)
            {
                throw new KeyStorageException("The device's secure storage couldn't save the key.", ex);
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
