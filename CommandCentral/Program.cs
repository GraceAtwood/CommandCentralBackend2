using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace CommandCentral
{
    public class Program
    {
        

        public static void Main(string[] args)
        {
            var options = new Options();

            if (args == null || !args.Any())
                Console.WriteLine("no");
            else if (!CommandLine.Parser.Default.ParseArguments(args, options))
                Console.WriteLine("nope");
            else
                Startup.Options = options;

            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .UseApplicationInsights()
                .Build();

            host.Run();
        }
    }
}
