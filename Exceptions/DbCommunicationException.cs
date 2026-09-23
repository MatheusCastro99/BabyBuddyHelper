namespace BabyBuddyHelper.Exceptions
{
    //Thrown by ITrackerDbService when the local database can't be reached or a command against it fails.
    //The original EF Core / SQLite exception is kept as InnerException. Programming errors are never wrapped in this type.
    public sealed class DbCommunicationException : Exception
    {
        public DbCommunicationException(string message) : base(message)
        {
        }

        public DbCommunicationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
