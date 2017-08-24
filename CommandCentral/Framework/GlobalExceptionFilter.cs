using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;

namespace CommandCentral.Framework
{
    public class GlobalExceptionFilter : IExceptionFilter
    {
        private readonly ILogger _logger;

        public GlobalExceptionFilter(ILoggerFactory logger)
        {
            _logger = logger?.CreateLogger("Command Central Exception Handler") 
                           ?? throw new ArgumentNullException(nameof(logger));
        }

        public void OnException(ExceptionContext context)
        {
            context.Result = new StatusCodeResult(500);
            _logger.LogError("GlobalExceptionFilter", context.Exception);
        }
    }
}
