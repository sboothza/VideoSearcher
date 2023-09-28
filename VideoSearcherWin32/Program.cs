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
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            var configBuilder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                                              .AddJsonFile("appsettings.json");

            Config = configBuilder.Build();

            using var host = Host.CreateDefaultBuilder()                                                            
                                 .Build();

            ApplicationConfiguration.Initialize();
            Application.Run(new Form1());
        }
        public static IConfiguration Config;
    }
}