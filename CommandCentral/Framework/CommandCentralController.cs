using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Filters;
using CommandCentral.Entities;
using NHibernate;
using CommandCentral.Authentication;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using CommandCentral.Enums;
using CommandCentral.Utilities;
using FluentValidation.Results;

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

        #region NHibernate Session Members

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
        /// Attempts to retireve an entity identified by the given id.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="entity"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool TryGet<T>(Guid id, out T entity) where T : Entity
        {
            entity = DBSession.Get<T>(id);

            return entity != null;
        }

        /// <summary>
        /// Attempts to retireve an entity identified by the given id.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="entity"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool TryGet<T>(Guid? id, out T entity) where T : Entity
        {
            if (!id.HasValue)
            {
                entity = null;
                return true;
            }

            entity = DBSession.Get<T>(id);

            return entity != null;
        }

        /// <summary>
        /// Shortcut for DBSession.Save()
        /// </summary>
        /// <param name="entity"></param>
        public void Save<T>(T entity) where T : Entity
        {
            DBSession.Save(entity);
        }

        /// <summary>
        /// Shortcut for ()
        /// </summary>
        /// <param name="entity"></param>
        public void Delete<T>(T entity) where T : Entity
        {
            DBSession.Delete(entity);
        }

        #endregion

        #region Change Tracking

        /// <summary>
        /// Call this method on a created entity after saving it to register it with change tracking.
        /// </summary>
        /// <param name="entity"></param>
        /// <typeparam name="T"></typeparam>
        public void LogEntityCreation<T>(T entity) where T : Entity
        {
            var change = new Change
            {
                ChangeTime = CallTime,
                Editor = User,
                Entity = entity,
                Id = Guid.NewGuid(),
                ChangeType = ChangeTypes.Create,
                PropertyPath = ""
            };

            DBSession.Save(change);
        }

        /// <summary>
        /// Call this method on the deletion of an entity to register that deletion with change tracking.
        /// </summary>
        /// <param name="entity"></param>
        /// <typeparam name="T"></typeparam>
        public void LogEntityDeletion<T>(T entity) where T : Entity
        {
            DBSession.Save(new Change
            {
                ChangeTime = CallTime,
                Editor = User,
                Entity = entity,
                Id = Guid.NewGuid(),
                ChangeType = ChangeTypes.Delete,
                PropertyPath = ""
            });
        }

        /// <summary>
        /// Call this method after making changes to your entity to register the changes with change tracking.
        /// </summary>
        /// <param name="entity"></param>
        /// <typeparam name="T"></typeparam>
        public void LogEntityModification<T>(T entity) where T : Entity
        {
            foreach (var change in GetEntityChanges(entity))
            {
                DBSession.Save(change);
            }
        }

        /// <summary>
        /// Retrieves a sequence of change objects that indicate which properties were changed.
        /// </summary>
        /// <param name="entity"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IEnumerable<Change> GetEntityChanges<T>(T entity) where T : Entity
        {
            foreach (var change in DBSession.GetChangesFromDirtyProperties(entity))
            {
                change.Editor = User;
                change.ChangeTime = CallTime;
                change.ChangeType = ChangeTypes.Modify;
                yield return change;
            }
        }

        #endregion

        #region Return Actions

        /// <summary>
        /// Returns a bad request with the error messages included in the body of the request.
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        [NonAction]
        public BadRequestObjectResult BadRequestWithValidationErrors(ValidationResult result)
        {
            return BadRequest(result.Errors.Select(x => x.ErrorMessage).ToList());
        }

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

        private bool TryGetPersonFromCert(X509Certificate2 cert, ActionExecutingContext context, out Person person)
        {
            person = null;

            if (cert == null)
                return false;

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
                return false;
            }

            var temp = cert.Subject.Substring(0, cert.Subject.Length - cert.Subject.IndexOf(','));
            var dodId = temp.Substring(temp.LastIndexOf('.') + 1, temp.Length - temp.IndexOf(',') - 1);

            person = DBSession.Query<Person>().SingleOrDefault(x => x.DoDId == dodId);
            if (person != null) 
                return true;
            
            return false;
        }

        /// <summary>
        /// Executes when the http session request is just about to handed to the controller.
        /// </summary>
        /// <param name="context"></param>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            HttpContext.Items["CallTime"] = DateTime.UtcNow;

            //Pull out the api key too.  I'm not doing anything with it yet :/
            APIKey apiKey;
            if (Request.Headers.TryGetValue("X-Api-Key", out var apiKeyHeader) &&
                Guid.TryParse(apiKeyHeader.FirstOrDefault(), out var apiKeyId))
            {
                apiKey = DBSession.Get<APIKey>(apiKeyId);
                if (apiKey == null)
                    context.Result = BadRequest($"The api key ({apiKeyId}) you provided was not valid.");
            }
            else
            {
                apiKey = Utilities.TestDatabaseBuilder.UnknownApplicationApiKey;
            }

            var cert = HttpContext.Connection.ClientCertificate;
            Person user;

            //We're in debug, and there's no impersonate header or cert.  assume the developer.
            if (ConfigurationUtility.InDebugMode)
            {
                if (!TryGetPersonFromCert(cert, context, out user))
                {
                    if (Request.Headers.TryGetValue("X-Impersonate-Person-Id", out var impersonatePersonIdHeader))
                    {
                        user = DBSession.Get<Person>(impersonatePersonIdHeader);
                        if (user == null)
                        {
                            context.Result = BadRequest("Your person identified by the  'X-Impersonate-Person-Id' " +
                                                        "header was not found in the database.");
                            return;
                        }
                    }
                    else
                    {
                        user = DBSession.Get<Person>(TestDatabaseBuilder.DeveloperId);
                        if (user == null)
                        {
                            context.Result = BadRequest("You must send either a 'X-Impersonate-Person-Id' header or " +
                                                        "a client certificate because the developer id was not recognized.");
                            return;
                        }
                    }

                }
            }
            else
            {
                if (!TryGetPersonFromCert(cert, context, out user))
                {
                    context.Result = BadRequest("Your certificate was invalid.");
                    return;
                }
            }

            HttpContext.Items["User"] = user;

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