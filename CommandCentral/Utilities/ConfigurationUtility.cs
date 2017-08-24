using Microsoft.Extensions.Configuration;
using System.IO;
using System.Reflection;

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
