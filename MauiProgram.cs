using DotNetEnv;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Core.Hosting;
using System.Diagnostics;
using System.Reflection;

namespace BabyBuddyHelper
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            LoadEnvironmentConfiguration();

            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                })
                .ConfigureSyncfusionCore()
                .Services.AddSingleton<Interfaces.ITaskListService, Services.TaskListService>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            Debug.WriteLine(Environment.GetEnvironmentVariable("SYNCFUSION_LICENSE"));


            return builder.Build();
        }

        private static void LoadEnvironmentConfiguration ()
        {
            try //tryCatch prevents app from crashing if .env file is missing or license is not set
            {
                var envPath = Path.Combine(AppContext.BaseDirectory, ".env");

                if (File.Exists(envPath)) //try to load .env file if it exists
                {
                    Env.Load(envPath);
                    Debug.WriteLine(".env Loaded");
                }

                var licenseStream = Environment.GetEnvironmentVariable("SYNCFUSION_LICENSE");

                if (!string.IsNullOrWhiteSpace(licenseStream)) //try to register Syncfusion license if it is set in environment variables
                {
                    Syncfusion.Licensing.SyncfusionLicenseProvider
                    .RegisterLicense(licenseStream);

                    Debug.WriteLine("Environment variables initialized and registered.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading .env file or registering Syncfusion license: {ex.Message}");
            }
        }
    }
}
