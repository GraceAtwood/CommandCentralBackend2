using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace CommandCentral.Utilities
{
    public static class ConfigurationUtility
    {
        public static string XmlDocumentationPath  = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "commandcentral.xml");

        public static IConfigurationRoot Configuration { get; private set; }

        static ConfigurationUtility()
        {
            Configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false, true)
                .Build();
        }
    }
}
