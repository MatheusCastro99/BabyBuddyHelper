namespace BabyBuddyHelper.Core.Exceptions
{
    //Thrown by IApiKeyStore when the device's secure storage can't save or remove the user's key.
    //The original platform exception is kept as InnerException. The key itself is never part of the message.
    public sealed class KeyStorageException : Exception
    {
        public KeyStorageException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
