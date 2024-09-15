using System;
using System.IO;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Nexus.Patch.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string workingDir = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            Console.WriteLine("Working directory: " + workingDir);
            Directory.SetCurrentDirectory(workingDir); // ugly hack to fix working dir.
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseUrls(GetServerUrl())
                .UseStartup<Startup>();

        private static string GetServerUrl()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // Get URL from configuration
            return configuration["Url"] ?? "http://localhost:80"; // Default fallback
        }
    }
}
