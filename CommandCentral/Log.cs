using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommandCentral
{
    public static class Log
    {
        private static ILogger _logger;

        public static ILogger LoggerInstance
        {
            get
            {
                return _logger;
            }
        }

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
