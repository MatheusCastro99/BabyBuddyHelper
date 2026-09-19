using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Core.Hosting;
using System.Diagnostics;

namespace BabyBuddyHelper
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            RegisterSyncfusionLicense();

            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                })
                .ConfigureSyncfusionCore()
                .Services.AddSingleton<Interfaces.ITaskListService, Services.TaskListService>()
                .AddSingleton<Interfaces.IBabyProfileService, Services.BabyProfileService>()
                .AddSingleton<Interfaces.IBabyFilterService, Services.BabyFilterService>()
                .AddSingleton<Interfaces.ITrackerDbService, Services.InMemoryTrackerDbService>(); //Swapped for the EF Core SQLite implementation in #25

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }

        //The license is compiled into BuildSecrets by the GenerateBuildSecrets target in the .csproj, so it travels
        //with the app on every platform. Nothing is read from disk at runtime, and the key is never logged.
        private static void RegisterSyncfusionLicense()
        {
            try //tryCatch prevents app from crashing if the license is missing or invalid
            {
                if (string.IsNullOrWhiteSpace(BuildSecrets.SyncfusionLicense))
                {
                    Debug.WriteLine("Syncfusion license missing at build time. Running unlicensed.");
                    return;
                }

                Syncfusion.Licensing.SyncfusionLicenseProvider
                .RegisterLicense(BuildSecrets.SyncfusionLicense);

                Debug.WriteLine("Syncfusion license registered.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error registering Syncfusion license: {ex.Message}");
            }
        }
    }
}
