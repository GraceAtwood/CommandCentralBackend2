﻿using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Controllers;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Abstractions;
using FluentValidation.Attributes;
using FluentValidation;
using FluentValidation.Results;
using CommandCentral.Entities;
using NHibernate;
using CommandCentral.Authentication;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Net;
using Microsoft.AspNetCore.Routing;

namespace CommandCentral.Framework
{
    public class CommandCentralController : Controller
    {

        public new Person User
        {
            get
            {
                return (Person)HttpContext.Items["User"];
            }
        }

        public DateTime CallTime
        {
            get
            {
                return (DateTime)HttpContext.Items["CallTime"];
            }
        }

        public ISession DBSession
        {
            get
            {
                return Data.SessionManager.CurrentSession;
            }
        }

        private ILogger Logger
        {
            get
            {
                return Log.LoggerInstance;
            }
        }

        #region Logging

        [NonAction]
        public void LogException(Exception e)
        {
            Logger.LogError(new EventId(), e, e.ToString());
        }

        [NonAction]
        public void LogInformation(string message)
        {
            Logger.LogInformation(message);
        }

        [NonAction]
        public void LogDebug(string message)
        {
            Logger.LogDebug(message);
        }

        #endregion

        #region Return Actions

        /// <summary>
        /// Returns an unauthorized (401) result with a value.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [NonAction]
        public IActionResult Unauthorized(object value = null)
        {
            return StatusCode((int)HttpStatusCode.Unauthorized, value);
        }

        [NonAction]
        public IActionResult InternalServerError(object value = null)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, value);
        }
        
        [NonAction]
        public IActionResult Forbid(object value)
        {
            return StatusCode((int)HttpStatusCode.Forbidden, value);
        }

        [NonAction]
        public IActionResult Conflict(object value = null)
        {
            return StatusCode((int)HttpStatusCode.Conflict, value);
        }

        #endregion

        #region On Actions

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            HttpContext.Items["CallTime"] = DateTime.UtcNow;

            //Pull out the api key too.
            if (!Request.Headers.TryGetValue("apikey", out Microsoft.Extensions.Primitives.StringValues apiKeyHeader)
                || !Guid.TryParse(apiKeyHeader.FirstOrDefault(), out Guid apiKey)
                || DBSession.Get<APIKey>(apiKey) == null)
            {
                context.Result = Unauthorized("Your api key was not valid.");
                return;
            }

            //Handle Authentication.  Do we require authentication?
            if (((ControllerActionDescriptor)context.ActionDescriptor).MethodInfo.GetCustomAttribute<RequireAuthenticationAttribute>() != null)
            {
                if (!Request.Headers.TryGetValue("sessionid", out Microsoft.Extensions.Primitives.StringValues sessionIdHeader)
                    || !Guid.TryParse(sessionIdHeader.FirstOrDefault(), out Guid sessionId))
                {
                    context.Result = Unauthorized();
                    return;
                }

                var authSession = DBSession.Get<AuthenticationSession>(sessionId);

                if (authSession == null || !authSession.IsValid())
                {
                    context.Result = Unauthorized();
                    return;
                }

                HttpContext.Items["User"] = authSession.Person;
                authSession.LastUsedTime = this.CallTime;

                DBSession.Update(authSession);
            }

            base.OnActionExecuting(context);
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            Data.SessionManager.CloseSession();
            base.OnActionExecuted(context);
        }

        #endregion

    }
}
