namespace BabyBuddyHelper.Core.Interfaces
{
    public interface IApiKeyStore //Keeps the user's own AI provider key (Claude only, for now) in the device's secure storage and nowhere else (ADR-010)
    {
        bool IsValidFormat(string? key); //A format check only, with no network call. It can't tell whether the provider accepts the key
        Task<bool> HasKeyAsync();
        Task<string?> GetKeyAsync(); //For the provider client only. Pages never read the key back, so it is never shown again
        Task SaveKeyAsync(string key); //Replaces a key that is already saved. Throws KeyStorageException when the device can't store it
        Task RemoveKeyAsync(); //Throws KeyStorageException when the device can't remove it
    }
}
