using DotNetEnv;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Core.Hosting;
using Syncfusion.Licensing;
using System.Reflection;
using System.Diagnostics;

namespace BabyBuddyHelper
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            //Get the current assembly to access the embedded .env file
            var assembly = Assembly.GetExecutingAssembly();

            //Since it is dealing with secrets, using statement is optimal to ensure the stream is properly disposed of after use
            using var stream = assembly.GetManifestResourceStream("BabyBuddyHelper..env"); //Format: {DefaultNamespace}.{Folder(if not root)}.{FileName}

            if (stream != null) //attempts to load the .env file and register keys
            {
                Env.Load(stream);
                Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(Environment.GetEnvironmentVariable("SYNCFUSION_LICENSE"));
            }

            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                })
                .ConfigureSyncfusionCore();

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            Debug.WriteLine(Environment.GetEnvironmentVariable("SYNCFUSION_LICENSE"));


            return builder.Build();
        }
    }
}
