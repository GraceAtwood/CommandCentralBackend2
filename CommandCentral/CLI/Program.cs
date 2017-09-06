using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.CommandLineUtils;
using CommandCentral.Utilities;

namespace CommandCentral.CLI
{
    /// <summary>
    /// Application entry point.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Application entry point.
        /// </summary>
        /// <param name="args"></param>
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
                throw new ArgumentException(
                    "The base address for the service was not found in the appsettings.json file.  " +
                    "It should be found at 'Server { BaseAddress = http://address:port/ }'.");

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

            app.Command("build", buildCommand =>
            {
                buildCommand.Description =
                    "Builds the database and optionally populates it with test data.  " +
                    "Destroys any schema that already exists in the targeted location.";
                buildCommand.HelpOption("-? | -h | --help");

                buildCommand.Command("testdata", testDataCommand =>
                {
                    testDataCommand.Description =
                        "Instructs the database build scripts to also populate the database with random test data.";
                    testDataCommand.HelpOption("-? | -h | --help");

                    var commandsOption = testDataCommand.Option("--com <commands>",
                        "Number of commands to create.  Must be greater than zero. Default = 4.",
                        CommandOptionType.SingleValue);

                    var departmentsOption = testDataCommand.Option("--dep <departments>",
                        "Number of departments per command to create.  Must be greater than zero. Default = 4.",
                        CommandOptionType.SingleValue);

                    var divisionsOption = testDataCommand.Option("--div <divisions>",
                        "Number of divisions per department to create.  Must be greater than zero. Default = 4.",
                        CommandOptionType.SingleValue);

                    var personsPerDivisionOption = testDataCommand.Option("--per <persons>",
                        "Number of persons to create per division.  Must be greater than zero. Default = 30.",
                        CommandOptionType.SingleValue);


                    testDataCommand.OnExecute(() =>
                    {
                        var commands = 4;
                        if (commandsOption.HasValue())
                            Int32.TryParse(commandsOption.Value(), out commands);
                        if (commands <= 0)
                            throw new CommandParsingException(app, "--com must be greater than zero.");

                        var departments = 4;
                        if (departmentsOption.HasValue())
                            Int32.TryParse(departmentsOption.Value(), out departments);
                        if (departments <= 0)
                            throw new CommandParsingException(app, "--dep must be greater than zero.");
                        
                        var divisions = 4;
                        if (divisionsOption.HasValue())
                            Int32.TryParse(divisionsOption.Value(), out divisions);
                        if (divisions <= 0)
                            throw new CommandParsingException(app, "--div must be greater than zero.");
                        
                        var persons = 30;
                        if (personsPerDivisionOption.HasValue())
                            Int32.TryParse(personsPerDivisionOption.Value(), out persons);
                        if (persons <= 0)
                            throw new CommandParsingException(app, "--per must be greater than zero.");

                        TestDatabaseBuilder.BuildDatabase();
                        TestDatabaseBuilder.InsertTestData(commands, departments, divisions, persons);

                        return 0;
                    });
                });

                buildCommand.OnExecute(() =>
                {
                    TestDatabaseBuilder.BuildDatabase();

                    return 0;
                });
            });

            app.Command("launch", launchCommand =>
            {
                launchCommand.Description = "Launches the api.";
                launchCommand.HelpOption("-? | -h | --help");

                launchCommand.OnExecute(() =>
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