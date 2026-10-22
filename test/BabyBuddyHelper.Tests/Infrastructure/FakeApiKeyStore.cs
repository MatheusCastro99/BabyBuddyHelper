using BabyBuddyHelper.Core.Interfaces;

namespace BabyBuddyHelper.Tests.Infrastructure
{
    //An in-memory key store holding a dummy key. The real one needs MAUI's SecureStorage, so it isn't linked here.
    public sealed class FakeApiKeyStore(string? key) : IApiKeyStore
    {
        public const string DummyKey = "sk-ant-test-0000-not-a-real-key";

        private string? _key = key;

        public bool IsValidFormat(string? key) => !string.IsNullOrWhiteSpace(key);
        public Task<bool> HasKeyAsync() => Task.FromResult(!string.IsNullOrEmpty(_key));
        public Task<string?> GetKeyAsync() => Task.FromResult(_key);

        public Task SaveKeyAsync(string key)
        {
            _key = key;
            return Task.CompletedTask;
        }

        public Task RemoveKeyAsync()
        {
            _key = null;
            return Task.CompletedTask;
        }
    }
}
