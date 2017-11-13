using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Filters;
using CommandCentral.Entities;
using NHibernate;
using CommandCentral.Authentication;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using CommandCentral.Utilities;

namespace CommandCentral.Framework
{
    /// <summary>
    /// Provides authentication handling and other services for all controllers.  All controllers in the project should inherit from this class.
    /// <para/>
    /// I understand that the authentication middle ware for ASP.NET core is where the authentication should be taking place.  I haven't done it there for two reasons:
    /// I don't know how yet and I gotta do other things.
    /// Until Command Central moves away from NIPR net, the added functionality the asp.net core identity provider gives us is pretty useless.
    /// </summary>
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Consumes("application/json")]
    public class CommandCentralController : Controller
    {
        /// <summary>
        /// Represents the currently logged in user for this HTTP context.  Invalid outside a web request context.
        /// </summary>
        public new Person User => (Person) HttpContext.Items["User"];

        /// <summary>
        /// The earliest time at which the client called the web service.
        /// </summary>
        public DateTime CallTime => (DateTime) HttpContext.Items["CallTime"];

        /// <summary>
        /// Represents a database session for this web request session.
        /// </summary>
        public ISession DBSession => Data.SessionManager.GetCurrentSession(HttpContext);

        /// <summary>
        /// Flush the <seealso cref="DBSession"/> associated with this controller within a transaction.  Automatically rolls back any changes if an exception is thrown.
        /// </summary>
        public void CommitChanges()
        {
            using (var transaction = DBSession.BeginTransaction())
                transaction.Commit();
        }

        /// <summary>
        /// Rolls back any transaction associated with the <seealso cref="DBSession"/> within this controller and clears all pending changes from the <seealso cref="DBSession"/>.
        /// <para />
        /// NOTE: Entities associated with the session may reapply their changes after the session is reverted/cleared.  If it's absolutely necessary, consider using .Evict on an entity to disable NHibernate's tracking of that entity.
        /// </summary>
        public void RevertChanges()
        {
            if (DBSession.Transaction.IsActive && !DBSession.Transaction.WasCommitted &&
                !DBSession.Transaction.WasRolledBack)
                DBSession.Transaction.Rollback();

            DBSession.Clear();
        }

        /// <summary>
        /// The logging instance that should be used for logging... things.
        /// </summary>
        private static ILogger Logger => Log.LoggerInstance;

        #region Logging

        /// <summary>
        /// Logs an exception.
        /// </summary>
        /// <param name="e"></param>
        [NonAction]
        public void LogException(Exception e)
        {
            Logger.LogError(new EventId(), e, e.ToString());
        }

        /// <summary>
        /// Logs information.
        /// </summary>
        /// <param name="message"></param>
        [NonAction]
        public void LogInformation(string message)
        {
            Logger.LogInformation(message);
        }

        /// <summary>
        /// Logs a debug message.
        /// </summary>
        /// <param name="message"></param>
        [NonAction]
        public void LogDebug(string message)
        {
            Logger.LogDebug(message);
        }

        #endregion

        #region Return Actions

        /// <summary>
        /// Returns a 422 Unprocessable entity result, indicating The server understands the content type of the request entity 
        /// (hence a 415 Unsupported Media Type status code is inappropriate), and the syntax of the request entity is correct 
        /// (thus a 400 Bad Request status code is inappropriate) but was unable to process the contained instructions.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [NonAction]
        public ObjectResult UnprocessableEntity(object data = null)
        {
            return StatusCode(422, data);
        }

        /// <summary>
        /// Returns a <seealso cref="BadRequestObjectResult"/> that indicates the limit must be greater than 0.
        /// </summary>
        /// <param name="limit"></param>
        /// <param name="limitParameterName"></param>
        /// <returns></returns>
        [NonAction]
        public BadRequestObjectResult BadRequestLimit(int limit, string limitParameterName)
        {
            return BadRequest(
                $"The value '{limit}' for the property '{limitParameterName}' was invalid.  It must be greater than zero.");
        }

        /// <summary>
        /// Returns a <seealso cref="BadRequestObjectResult"/> that indicates an error in parsing caused the dto to be null on arrival.
        /// </summary>
        /// <returns></returns>
        [NonAction]
        public BadRequestObjectResult BadRequestDTONull()
        {
            return BadRequest(ControllerContext.ModelState);
        }

        /// <summary>
        /// Returns a <seealso cref="NotFoundObjectResult"/> with the body set to an error message indicating an object identified by the parameter could not be found.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="parameterName"></param>
        /// <returns></returns>
        [NonAction]
        public NotFoundObjectResult NotFoundParameter(object id, string parameterName)
        {
            return NotFound(
                $"An object with the identifier '{id}', identified by your parameter '{parameterName}', could not be found.");
        }

        /// <summary>
        /// Returns a <seealso cref="NotFoundObjectResult"/> with the body set to an error message indicating an object identified by the parameter could not be found.
        /// </summary>
        /// <param name="childId"></param>
        /// <param name="childParameterName"></param>
        /// <param name="parentId"></param>
        /// <param name="parentParamenentName"></param>
        /// <returns></returns>
        [NonAction]
        public NotFoundObjectResult NotFoundChildParameter(Guid parentId, string parentParamenentName, Guid childId,
            string childParameterName)
        {
            return NotFound(
                $"An object with Id '{childId}' identified by your parameter '{childParameterName}', child of an object with Id '{parentId}' identified by your parameter '{childParameterName},' could not be found.");
        }

        /// <summary>
        /// Returns an unauthorized (401) result with a value.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [NonAction]
        public IActionResult Unauthorized(object value = null)
        {
            return StatusCode((int) HttpStatusCode.Unauthorized, value);
        }

        /// <summary>
        /// Returns a 500 Internal Server Error with the given value as the body.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [NonAction]
        public IActionResult InternalServerError(object value = null)
        {
            return StatusCode((int) HttpStatusCode.InternalServerError, value);
        }

        /// <summary>
        /// Returns a 403 FORBID status.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [NonAction]
        public IActionResult Forbid(object value)
        {
            return StatusCode((int) HttpStatusCode.Forbidden, value);
        }

        /// <summary>
        /// Returns a 403 FORBID status.
        /// </summary>
        [NonAction]
        public new IActionResult Forbid()
        {
            return StatusCode((int) HttpStatusCode.Forbidden);
        }

        /// <summary>
        /// Returns a 409 CONFLICT status code indicating that the requested action would cause a conflict that can not be resolved.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [NonAction]
        public IActionResult Conflict(object value = null)
        {
            return StatusCode((int) HttpStatusCode.Conflict, value);
        }

        #endregion

        #region On Actions

        /// <summary>
        /// Executes when the http session request is just about to handed to the controller.
        /// </summary>
        /// <param name="context"></param>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            HttpContext.Items["CallTime"] = DateTime.UtcNow;

            //Pull out the api key too.
            if (!Request.Headers.TryGetValue("X-Api-Key", out var apiKeyHeader)
                || !Guid.TryParse(apiKeyHeader.FirstOrDefault(), out var apiKey)
                || DBSession.Get<APIKey>(apiKey) == null)
            {
                context.Result = Unauthorized(
                    "Your api key was not valid or was not provided.  You must provide an api key (Guid) in the header 'X-Api-Key'.  " +
                    "If you do not have an api key for your application, please contact the development team and we'll hook you up.");
                return;
            }

            //Handle Authentication.  Do we require authentication?  If we're in debug mode, then we can allow the client
            //to "impersonate" any user they want.  Otherwise, we need to validate the client certificate and read the DoD Id.
            if (ConfigurationUtility.InDebugMode &&
                Request.Headers.TryGetValue("X-Impersonate-Person-Id", out var impersonatePersonIdHeader))
            {
                if (!Guid.TryParse(impersonatePersonIdHeader.FirstOrDefault(), out var impersonatePersonId))
                {
                    context.Result = BadRequest("You passed a 'X-Impersonate-Person-Id' header; however, " +
                                                "it could not be parsed to a person id.");
                    return;
                }

                var impersonatedPerson = DBSession.Get<Person>(impersonatePersonId);
                if (impersonatedPerson == null)
                {
                    context.Result = NotFoundParameter(impersonatePersonId, "X-Impersonate-Person-Id");
                    return;
                }

                HttpContext.Items["User"] = impersonatedPerson;
            }
            else
            {
                var cert = HttpContext.Connection.ClientCertificate;
                
                var isCertificateValid = new X509Chain
                {
                    ChainPolicy =
                    {
                        UrlRetrievalTimeout = TimeSpan.MaxValue,
                        RevocationMode = X509RevocationMode.Online,
                        RevocationFlag = X509RevocationFlag.EntireChain,
                        VerificationFlags = X509VerificationFlags.NoFlag
                    }
                }.Build(cert);

                if (!isCertificateValid)
                {
                    context.Result = Unauthorized("The passed certificate was invalid.");
                    return;
                }

                var dodId = cert.Subject.Substring(cert.Subject.LastIndexOf('.'));

                var client = DBSession.Query<Person>().SingleOrDefault(x => x.DoDId == dodId);
                if (client == null)
                {
                    context.Result =
                        NotFound("We were unable to find a user in the database with your DoD Id!  " +
                                 "Please communicate with admin to have someone update or create your profile.");
                    return;
                }

                HttpContext.Items["User"] = client;
            }

            base.OnActionExecuting(context);
        }

        /// <summary>
        /// Executes when an action has been completed and handled.
        /// </summary>
        /// <param name="context"></param>
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            //Here we choose to revert any changes before cleaning up the session.  
            //The assumption is that any changes should've been explictly handled by the controller.
            //Any changes not handled by the controller must not have been intended to be committed.
            RevertChanges();
            Data.SessionManager.UnbindSession();

            base.OnActionExecuted(context);
        }

        #endregion
    }
}