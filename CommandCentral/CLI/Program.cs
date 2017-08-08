using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using CommandCentral.CLI.Options;
using Microsoft.Extensions.DependencyInjection;
using CommandCentral.Enums;

namespace CommandCentral.CLI
{
    public class Program
    {
        

        public static void Main(string[] args)
        {
            try
            {
                if (Environment.UserInteractive)
                {
                    var options = new MainOptions();

                    CommandLine.Parser.Default.ParseArguments(args, options, (verb, subOptions) =>
                    {
                        if (subOptions == null)
                            throw new Exception("Invalid parameters!");

                        if (subOptions is LaunchOptions launch)
                        {
                            var host = new WebHostBuilder()
                                .UseKestrel()
                                .UseContentRoot(Directory.GetCurrentDirectory())
                                .UseUrls("http://*:1113")
                                .UseStartup<Framework.Startup>()
                                .Build();

                            host.Run();
                        }
                        else if (subOptions is BuildOptions build)
                        {
                            Utilities.TestDatabaseBuilder.BuildDatabase();

                        }
                        else
                        {
                            throw new NotImplementedException();
                        }
                    });
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.ReadLine();
            }
        }
    }
}
