using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Nexus.Patch.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string workingDir = Path.GetDirectoryName(
      System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase).Substring(6);
            Directory.SetCurrentDirectory(workingDir); // ugly hack to fix working dir.
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseUrls("http://localhost:80")
                .UseStartup<Startup>();
    }
}
