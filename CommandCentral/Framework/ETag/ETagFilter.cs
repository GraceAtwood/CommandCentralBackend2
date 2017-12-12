using System;
using System.Linq;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CommandCentral.Framework.ETag
{
    public class ETagFilter : Attribute, IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.HttpContext.Request.Method == "GET")
            {
                if (context.HttpContext.Request.Headers.TryGetValue("If-None-Match", out var eTagCollection))
                {
                    foreach (var eTag in eTagCollection)
                    {
                        if (ETagCache.TryGetCachedEntityDescriptor(eTag, out var cachedEntityDescriptor))
                        {
                            var controllerName = ((ControllerActionDescriptor) context.ActionDescriptor).ControllerName;

                            context.Result = new NotModifiedResult("GET", controllerName,
                                new {id = cachedEntityDescriptor.EntityId}, cachedEntityDescriptor.DateTime,
                                cachedEntityDescriptor.ETag);

                            return;
                        }
                    }
                }
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }
    }
}