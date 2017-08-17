using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using CommandCentral.Enums;
using CommandCentral.Utilities;

namespace CommandCentral.CLI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var app = new Microsoft.Extensions.CommandLineUtils.CommandLineApplication();

            var build = app.Command("build", config =>
            {
                config.Description = "Builds the database and optionally populates it with test data.  Destroys any schema that already exists in the targeted location.";
                config.HelpOption("-? | -h | --help");

                var testData = config.Command("testdata", innerConfig =>
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

            var launch = app.Command("launch", config =>
            {
                config.Description = "Launches the api.";
                config.HelpOption("-? | -h | --help");

                config.OnExecute(() =>
                {
                    var host = new WebHostBuilder()
                                .UseKestrel()
                                .UseContentRoot(Directory.GetCurrentDirectory())
                                .UseUrls("http://*:1113")
                                .UseStartup<Framework.Startup>()
                                .Build();

                    host.Run();
                    return 0;
                });
            });

            app.HelpOption("-? | -h | --help");
            var result = app.Execute(args);
            Environment.Exit(result);
        }
    }
}
