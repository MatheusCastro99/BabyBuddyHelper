namespace BabyBuddyHelper.Core.Exceptions
{
    //The one exception ICubChatClient throws when Cub can't answer: no key, no network, a timeout, or the provider refusing
    //the call (ADR-017). The original exception, when there is one, is kept as InnerException.
    //The key and the parent's messages are never part of the message (ADR-010).
    public sealed class CubUnavailableException : Exception
    {
        public CubUnavailableException(string message) : base(message)
        {
        }

        public CubUnavailableException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
