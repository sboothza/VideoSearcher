using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace VideoSearcherWin32
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var environmentName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: true);

            Config = configBuilder.Build();

            using var host = Host.CreateDefaultBuilder()
                                 .Build();

            ApplicationConfiguration.Initialize();
            Application.Run(new FormMain());
        }
        public static IConfiguration Config;
    }
}