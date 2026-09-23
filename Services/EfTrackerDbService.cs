using BabyBuddyHelper.Data;
using BabyBuddyHelper.Exceptions;
using BabyBuddyHelper.Interfaces;
using BabyBuddyHelper.Models;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace BabyBuddyHelper.Services
{
    //EF Core SQLite implementation of the persistence boundary. Every call uses its own short-lived context.
    //Every public method runs through ExecuteReadAsync or ExecuteWriteAsync, which turn database failures into DbCommunicationException.
    public class EfTrackerDbService : ITrackerDbService
    {
#if DEBUG
        private enum SimulatedDbFailure { None, Load, Save }

        //Set to true for one run after a schema change: EnsureCreated never alters an existing database. Set it back afterwards,
        //or every launch starts from an empty database. static readonly rather than const, so the compiler doesn't flag dead code.
        private static readonly bool ResetDatabaseOnStartup = false;

        //Set for a run to exercise the failure handling. Load fails every read (the startup load), Save fails every write.
        //The call throws before it reaches the database, so no stored data is touched. Set it back to None afterwards.
        //Use breakpoint + Watch window to change it at runtime, so the same run can exercise both failure types.
        private static SimulatedDbFailure SimulateDbFailure = SimulatedDbFailure.None;
#endif

        private readonly IDbContextFactory<TrackerContext> _contextFactory;
        private Task? _databaseReadyTask;

        public EfTrackerDbService(IDbContextFactory<TrackerContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public Task<IReadOnlyList<TaskModel>> GetTasksAsync() =>
            ExecuteReadAsync<IReadOnlyList<TaskModel>>(async () =>
            {
                await using TrackerContext db = await CreateContextAsync();
                return await db.Tasks.AsNoTracking().ToListAsync();
            });

        public Task AddTaskAsync(TaskModel task) =>
            ExecuteWriteAsync(async () =>
            {
                await using TrackerContext db = await CreateContextAsync();
                db.Tasks.Add(task);
                await db.SaveChangesAsync();
            });

        //A task <-> appointment conversion changes the row's type, which EF can't do in place. The row is deleted and
        //re-inserted under the same Id, in one transaction so a failure can't lose the record.
        public Task UpdateTaskAsync(TaskModel task) =>
            ExecuteWriteAsync(async () =>
            {
                await using TrackerContext db = await CreateContextAsync();
                TaskModel? storedTask = await db.Tasks.AsNoTracking().FirstOrDefaultAsync(x => x.Id == task.Id);

                //A missing row goes through Update too: SaveChanges then affects 0 rows and throws DbUpdateConcurrencyException,
                //so the caller gets DbCommunicationException instead of a silent success, and the cache is never updated for a
                //row that wasn't saved. Same behavior as UpdateBabyProfileAsync.
                if (storedTask is null || storedTask.GetType() == task.GetType())
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
            });

        public Task RemoveTaskAsync(Guid taskId) =>
            ExecuteWriteAsync(async () =>
            {
                await using TrackerContext db = await CreateContextAsync();
                await db.Tasks.Where(x => x.Id == taskId).ExecuteDeleteAsync();
            });

        public Task<IReadOnlyList<BabyModel>> GetBabyProfilesAsync() =>
            ExecuteReadAsync<IReadOnlyList<BabyModel>>(async () =>
            {
                await using TrackerContext db = await CreateContextAsync();
                return await db.BabyProfiles.AsNoTracking().ToListAsync();
            });

        public Task AddBabyProfileAsync(BabyModel babyProfile) =>
            ExecuteWriteAsync(async () =>
            {
                await using TrackerContext db = await CreateContextAsync();
                db.BabyProfiles.Add(babyProfile);
                await db.SaveChangesAsync();
            });

        public Task UpdateBabyProfileAsync(BabyModel babyProfile) =>
            ExecuteWriteAsync(async () =>
            {
                await using TrackerContext db = await CreateContextAsync();
                db.BabyProfiles.Update(babyProfile);
                await db.SaveChangesAsync();
            });

        //The foreign key's ON DELETE SET NULL clears this baby from its tasks in the same statement
        public Task RemoveBabyProfileAsync(Guid babyId) =>
            ExecuteWriteAsync(async () =>
            {
                await using TrackerContext db = await CreateContextAsync();
                await db.BabyProfiles.Where(x => x.Id == babyId).ExecuteDeleteAsync();
            });

        //Context creation runs inside the wrapped operation, so a database that can't be created or opened surfaces the same way.
        private static async Task<T> ExecuteReadAsync<T>(Func<Task<T>> operation, [CallerMemberName] string operationName = "")
        {
#if DEBUG
            ThrowIfSimulated(SimulatedDbFailure.Load, operationName);
#endif
            try
            {
                return await operation();
            }
            catch (Exception ex) when (IsDatabaseFailure(ex))
            {
                throw Wrap(ex, operationName);
            }
        }

        private static async Task ExecuteWriteAsync(Func<Task> operation, [CallerMemberName] string operationName = "")
        {
#if DEBUG
            ThrowIfSimulated(SimulatedDbFailure.Save, operationName);
#endif
            try
            {
                await operation();
            }
            catch (Exception ex) when (IsDatabaseFailure(ex))
            {
                throw Wrap(ex, operationName);
            }
        }

        //SqliteException derives from DbException. Anything else is a programming error and is left to surface as-is.
        private static bool IsDatabaseFailure(Exception ex) => ex is DbUpdateException or DbException;

        //A model-vs-schema bug (e.g. a NULL written into a NOT NULL column) is also a DbUpdateException, so the user sees a failed
        //save. The full inner exception goes to Debug output so the real cause stays visible while developing.
        private static DbCommunicationException Wrap(Exception ex, string operationName)
        {
            Debug.WriteLine($"Database failure in {operationName}: {ex}");
            return new DbCommunicationException($"Database communication failed during {operationName}.", ex);
        }

#if DEBUG
        private static void ThrowIfSimulated(SimulatedDbFailure failure, string operationName)
        {
            //failure is the SimulatedDbFailure state passed as argument when method is called.
            //SimulateDbFailure is the current state of the static field, which the method uses to test against failure.
            //For forced failures only. SimulateDbFailure controls the behavior of this method
            if (SimulateDbFailure != failure)
                return;

            Debug.WriteLine($"Simulated {failure} failure in {operationName}. Set SimulateDbFailure back to None afterwards.");
            throw new DbCommunicationException($"Simulated {failure} failure during {operationName}.");
        }
#endif

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
