using BabyBuddyHelper.Data;
using BabyBuddyHelper.Interfaces;
using BabyBuddyHelper.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace BabyBuddyHelper.Services
{
    //EF Core SQLite implementation of the persistence boundary. Every call uses its own short-lived context.
    public class EfTrackerDbService : ITrackerDbService
    {
#if DEBUG
        //Set to true for one run after a schema change: EnsureCreated never alters an existing database. Set it back afterwards,
        //or every launch starts from an empty database. static readonly rather than const, so the compiler doesn't flag dead code.
        private static readonly bool ResetDatabaseOnStartup = false;
#endif

        private readonly IDbContextFactory<TrackerContext> _contextFactory;
        private Task? _databaseReadyTask;

        public EfTrackerDbService(IDbContextFactory<TrackerContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<IReadOnlyList<TaskModel>> GetTasksAsync()
        {
            await using TrackerContext db = await CreateContextAsync();
            return await db.Tasks.AsNoTracking().ToListAsync();
        }

        public async Task AddTaskAsync(TaskModel task)
        {
            await using TrackerContext db = await CreateContextAsync();
            db.Tasks.Add(task);
            await db.SaveChangesAsync();
        }

        //A task <-> appointment conversion changes the row's type, which EF can't do in place. The row is deleted and
        //re-inserted under the same Id, in one transaction so a failure can't lose the record.
        public async Task UpdateTaskAsync(TaskModel task)
        {
            await using TrackerContext db = await CreateContextAsync();
            TaskModel? storedTask = await db.Tasks.AsNoTracking().FirstOrDefaultAsync(x => x.Id == task.Id);

            if (storedTask is null)
                return;

            if (storedTask.GetType() == task.GetType())
            {
                db.Tasks.Update(task);
                await db.SaveChangesAsync();
                return;
            }

            await using var transaction = await db.Database.BeginTransactionAsync();
            await db.Tasks.Where(x => x.Id == task.Id).ExecuteDeleteAsync();
            db.Tasks.Add(task);
            await db.SaveChangesAsync();
            await transaction.CommitAsync();
        }

        public async Task RemoveTaskAsync(Guid taskId)
        {
            await using TrackerContext db = await CreateContextAsync();
            await db.Tasks.Where(x => x.Id == taskId).ExecuteDeleteAsync();
        }

        public async Task<IReadOnlyList<BabyModel>> GetBabyProfilesAsync()
        {
            await using TrackerContext db = await CreateContextAsync();
            return await db.BabyProfiles.AsNoTracking().ToListAsync();
        }

        public async Task AddBabyProfileAsync(BabyModel babyProfile)
        {
            await using TrackerContext db = await CreateContextAsync();
            db.BabyProfiles.Add(babyProfile);
            await db.SaveChangesAsync();
        }

        public async Task UpdateBabyProfileAsync(BabyModel babyProfile)
        {
            await using TrackerContext db = await CreateContextAsync();
            db.BabyProfiles.Update(babyProfile);
            await db.SaveChangesAsync();
        }

        //The foreign key's ON DELETE SET NULL clears this baby from its tasks in the same statement
        public async Task RemoveBabyProfileAsync(Guid babyId)
        {
            await using TrackerContext db = await CreateContextAsync();
            await db.BabyProfiles.Where(x => x.Id == babyId).ExecuteDeleteAsync();
        }

        private async Task<TrackerContext> CreateContextAsync()
        {
            await EnsureDatabaseReadyAsync();
            return await _contextFactory.CreateDbContextAsync();
        }

        //Creates the database on first use. Concurrent callers share the same run; a failed run is retried on the next call.
        private Task EnsureDatabaseReadyAsync()
        {
            if (_databaseReadyTask is null || _databaseReadyTask.IsFaulted || _databaseReadyTask.IsCanceled)
            {
                _databaseReadyTask = CreateDatabaseAsync();
            }

            return _databaseReadyTask;
        }

        private async Task CreateDatabaseAsync()
        {
            await using TrackerContext db = await _contextFactory.CreateDbContextAsync();

#if DEBUG
            if (ResetDatabaseOnStartup)
            {
                await db.Database.EnsureDeletedAsync();
                Debug.WriteLine("Database reset: ResetDatabaseOnStartup is on. Turn it off after this run.");
            }
#endif

            bool created = await db.Database.EnsureCreatedAsync();
            Debug.WriteLine($"Database {(created ? "created" : "opened")}: {db.Database.GetDbConnection().DataSource}");
        }
    }
}
