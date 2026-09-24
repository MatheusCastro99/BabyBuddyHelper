using BabyBuddyHelper.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BabyBuddyHelper.Core.Persistence
{
    //EF Core SQLite context. Only EfTrackerDbService uses it, through IDbContextFactory (one short-lived context per operation).
    public class TrackerContext : DbContext
    {
        public DbSet<TaskModel> Tasks => Set<TaskModel>();
        public DbSet<BabyModel> BabyProfiles => Set<BabyModel>();

        public TrackerContext(DbContextOptions<TrackerContext> options)
            : base(options)
        {
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            //SQLite doesn't store DateTimeKind, so every date would come back Unspecified. The app works in local time throughout.
            configurationBuilder.Properties<DateTime>().HaveConversion<LocalDateTimeConverter>();
            configurationBuilder.Properties<DateTime?>().HaveConversion<LocalDateTimeConverter>();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Tasks and appointments share one table (table-per-hierarchy); TaskType records which one a row is
            modelBuilder.Entity<TaskModel>(task =>
            {
                task.HasKey(x => x.Id);
                task.Property(x => x.Id).ValueGeneratedNever(); //Ids are created by the app, never by the database

                task.HasDiscriminator<string>("TaskType")
                    .HasValue<TaskModel>("Task")
                    .HasValue<AppointmentModel>("Appointment");

                //Deleting a baby clears it from its tasks in the database itself. No navigation properties: tasks only carry the Id.
                task.HasOne<BabyModel>()
                    .WithMany()
                    .HasForeignKey(x => x.AssociatedBabyId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<BabyModel>(baby =>
            {
                baby.HasKey(x => x.Id);
                baby.Property(x => x.Id).ValueGeneratedNever();
            });
        }

        private sealed class LocalDateTimeConverter : ValueConverter<DateTime, DateTime>
        {
            public LocalDateTimeConverter()
                : base(toStore => toStore, fromStore => DateTime.SpecifyKind(fromStore, DateTimeKind.Local))
            {
            }
        }
    }
}
