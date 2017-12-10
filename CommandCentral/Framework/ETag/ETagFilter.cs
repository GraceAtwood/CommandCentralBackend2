using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CommandCentral.Framework.ETag
{
    public class ETagFilter : Attribute, IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.HttpContext.Request.Method == "GET")
            {
                if (context.HttpContext.Request.Headers.TryGetValue("If-None-Match", out var value))
                {
                    if (value.Any(eTag => ETagCache.TryGetCachedEntityDescriptor(eTag, out var cachedEntityDescriptor)))
                    {
                        
                    }
                }
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            throw new NotImplementedException();
        }

        private NotModifiedResult CreateNotModifiedResult(ActionContext context,
            CachedEntityDescriptor cachedEntityDescriptor)
        {
            return new NotModifiedResult("GET",
                ((Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor) context.ActionDescriptor)
                .ControllerName, new {id = cachedEntityDescriptor.EntityId},
                cachedEntityDescriptor.DateTime, cachedEntityDescriptor.ETag);
        }
    }
}