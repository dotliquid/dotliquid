using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace DotLiquid.Website
{
    /// <summary>
    /// The website main program
    /// </summary>
    static public class Program
    {
        /// <summary>
        /// Website main method to start up the host
        /// </summary>
        /// <param name="args">Default program input arguments</param>
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}