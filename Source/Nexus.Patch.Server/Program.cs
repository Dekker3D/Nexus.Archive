using System;
using System.IO;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

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
                .UseUrls("http://localhost:80")
                .UseStartup<Startup>();
    }
}
