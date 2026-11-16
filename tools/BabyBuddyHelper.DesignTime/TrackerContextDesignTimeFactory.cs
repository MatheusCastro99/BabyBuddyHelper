using BabyBuddyHelper.Core.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BabyBuddyHelper.DesignTime
{
    //Only `dotnet ef` uses this (migrations add, dbcontext info). The app builds its context in MauiProgram and never reaches here.
    //The connection string only has to be valid: no command run through this factory touches the file.
    public class TrackerContextDesignTimeFactory : IDesignTimeDbContextFactory<TrackerContext>
    {
        public TrackerContext CreateDbContext(string[] args)
        {
            DbContextOptions<TrackerContext> options = new DbContextOptionsBuilder<TrackerContext>()
                .UseSqlite("Data Source=design-time.db3")
                .Options;

            return new TrackerContext(options);
        }
    }
}
