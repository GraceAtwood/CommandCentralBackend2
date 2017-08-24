using Microsoft.Extensions.Logging;

namespace CommandCentral
{
    public static class Log
    {
        private static ILogger _logger;

        public static ILogger LoggerInstance => _logger;

        public static void Initialize(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger("Command Central");
        }

        public static void Initialize()
        {
            _logger = new LoggerFactory().AddConsole().AddDebug().CreateLogger("Command Central");
        }
    }
}
