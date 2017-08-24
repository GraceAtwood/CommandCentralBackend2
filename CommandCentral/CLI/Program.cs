using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.CommandLineUtils;
using CommandCentral.Utilities;

namespace CommandCentral.CLI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args.Any())
            {
                HandleCommandLine(args);
            }
            else
            {
                LaunchService();
            }
        }

        private static void LaunchService()
        {
            var url = ConfigurationUtility.Configuration["Server:BaseAddress"];
            
            if (String.IsNullOrWhiteSpace(url))
                throw new ArgumentException("The base address for the service was not found in the appsettings.json file.  It should be found at 'Server { BaseAddress = http://address:port/ }'.");

            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseUrls(url)
                .UseStartup<Framework.Startup>()
                .Build();

            host.Run();
        }

        private static void HandleCommandLine(string[] args)
        {
            var app = new CommandLineApplication();

            app.Command("build", config =>
            {
                config.Description = "Builds the database and optionally populates it with test data.  Destroys any schema that already exists in the targeted location.";
                config.HelpOption("-? | -h | --help");

                config.Command("testdata", innerConfig =>
                {
                    innerConfig.Description = "Instructs the database build scripts to also populate the database with random test data.";
                    innerConfig.HelpOption("-? | -h | --help");
                    innerConfig.OnExecute(() =>
                    {
                        TestDatabaseBuilder.BuildDatabase(true);

                        return 0;
                    });
                });

                config.OnExecute(() =>
                {
                    TestDatabaseBuilder.BuildDatabase(false);

                    return 0;
                });
            });

            app.Command("launch", config =>
            {
                config.Description = "Launches the api.";
                config.HelpOption("-? | -h | --help");

                config.OnExecute(() =>
                {
                    LaunchService();
                    return 0;
                });
            });

            app.HelpOption("-? | -h | --help");
            var result = app.Execute(args);
            Environment.Exit(result);
        }
    }
}
