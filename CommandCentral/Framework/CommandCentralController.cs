using Microsoft.AspNetCore.Mvc;
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

        public ILogger Logger
        {
            get
            {
                return Log.LoggerInstance;
            }
        }

        #region Error Handling

        [NonAction]
        public void LogException(Exception e)
        {

        }

        #endregion

        #region Return Actions

        /// <summary>
        /// Returns an unauthorized (401) result with a value.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public IActionResult Unauthorized(object value)
        {
            return StatusCode(401, value);
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
                    context.Result = Unauthorized("Your sesion id was not valid.");
                    return;
                }

                var authSession = DBSession.Get<AuthenticationSession>(sessionId);

                if (authSession == null || !authSession.IsValid())
                {
                    context.Result = Unauthorized("Your session has timed out.");
                    return;
                }

                HttpContext.Items["User"] = authSession.Person;
                authSession.LastUsedTime = this.CallTime;

                DBSession.Update(authSession);
            }

            if (context.ActionDescriptor.Parameters.Count > 1)
                throw new Exception("Why do we have more than one parameter?  HAVE WE LEARNED SOMETHING NEW?!");

            if (context.ActionDescriptor.Parameters.Count == 1)
            {
                var parameter = context.ActionDescriptor.Parameters.First();

                if (parameter.ParameterType.GetCustomAttribute<FromBodyAttribute>() != null)
                {
                    var value = context.ActionArguments.First().Value;

                    //First, if the model is required, let's check to see if its value is null.
                    if (((ControllerParameterDescriptor)parameter).ParameterInfo.GetCustomAttribute<RequiredModelAttribute>() != null && value == null)
                    {
                        //So the value is null.  In this case, let's tell the client how to call this endpoint.
                        context.ModelState.AddModelError(parameter.Name, "Please send the request properly.");
                    }

                    //If the model is not null, let's call the validator for the dto if possible.
                    if (value is IValidatable validatableDTO)
                    {
                        var result = validatableDTO.Validate();

                        if (!result.IsValid)
                        {
                            foreach (var error in result.Errors)
                            {
                                context.ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                            }
                        }

                        if (!context.ModelState.IsValid)
                        {
                            context.Result = BadRequest(context.ModelState.Values.Where(x => x.ValidationState == Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Invalid)
                                .SelectMany(x => x.Errors.Select(y => y.ErrorMessage)).ToList());
                            return;
                        }
                    }
                }
            }

            base.OnActionExecuting(context);
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            base.OnActionExecuted(context);

            Data.SessionManager.CloseSession();
        }

        #endregion

    }
}
