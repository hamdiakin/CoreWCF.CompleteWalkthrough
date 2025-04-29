using CoreWCF.Configuration;
using System.Diagnostics;

namespace NetCoreServer
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Host terminated unexpectedly: {ex.Message}");
                if (Debugger.IsAttached)
                {
                    Console.WriteLine(ex.ToString());
                    Console.ReadKey();
                }
            }
        }

        // Listen on 8088 for http, 8443 for https, and 8089 for NetTcp
        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .UseUrls()
                        .UseKestrel(options =>
                        {
                            options.ListenAnyIP(Startup.HTTP_PORT);
                            options.ListenAnyIP(Startup.HTTPS_PORT, listenOptions =>
                            {
                                listenOptions.UseHttps();
                                if (Debugger.IsAttached)
                                {
                                    listenOptions.UseConnectionLogging();
                                }
                            });
                        })
                        .UseNetTcp(Startup.NETTCP_PORT)
                        .ConfigureLogging(logging =>
                        {
                            logging.ClearProviders();
                            logging.AddConsole();
                            logging.AddDebug();

                            // Set default minimum level
                            logging.SetMinimumLevel(LogLevel.Information);

                            // Configure specific categories to filter more strictly
                            logging.AddFilter("CoreWCF.Channels", LogLevel.Warning);
                            logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.Information);
                        })
                        .UseStartup<Startup>();
                });
        }
    }
}