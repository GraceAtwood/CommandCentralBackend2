using System;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;

namespace CommandCentral.Framework.ETag
{
    /// <summary>
    /// An <see cref="T:Microsoft.AspNetCore.Mvc.ActionResult" /> that returns a NotModified (304) response with Location, ETag, and Date headers.
    /// </summary>
    public class NotModifiedResult : ObjectResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:NotModifiedResult" /> with the values provided.
        /// </summary>
        /// <param name="actionName">The name of the action to use for generating the URL.</param>
        /// <param name="controllerName">The name of the controller to use for generating the URL.</param>
        /// <param name="routeValues">The route data to use for generating the URL.</param>
        /// <param name="value">The value to format in the entity body.</param>
        /// <param name="date">The date at which the cache was created.</param>
        /// <param name="eTag">The ETag representing the cached resource version.</param>
        public NotModifiedResult(string actionName, string controllerName, object routeValues, object value,
            DateTime date, string eTag)
            : base(value)
        {
            ActionName = actionName;
            ControllerName = controllerName;
            RouteValues = routeValues == null ? null : new RouteValueDictionary(routeValues);
            StatusCode = (int) HttpStatusCode.NotModified;
            Date = date;
            ETag = eTag;
        }

        /// <summary>
        /// Gets or sets the ETag that represents the cached resource.
        /// </summary>
        private string ETag { get; set; }

        /// <summary>
        /// Gets or sets the date/time the cache was created at.
        /// </summary>
        private DateTime Date { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="T:Microsoft.AspNetCore.Mvc.IUrlHelper" /> used to generate URLs.
        /// </summary>
        private IUrlHelper UrlHelper { get; set; }

        /// <summary>
        /// Gets or sets the name of the action to use for generating the URL.
        /// </summary>
        private string ActionName { get; set; }

        /// <summary>
        /// Gets or sets the name of the controller to use for generating the URL.
        /// </summary>
        private string ControllerName { get; set; }

        /// <summary>
        /// Gets or sets the route data to use for generating the URL.
        /// </summary>
        private RouteValueDictionary RouteValues { get; set; }

        /// <inheritdoc />
        public override void OnFormatting(ActionContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            base.OnFormatting(context);

            var request = context.HttpContext.Request;

            var url = (UrlHelper ?? context.HttpContext.RequestServices.GetRequiredService<IUrlHelperFactory>()
                           .GetUrlHelper(context))
                .Action(ActionName, ControllerName, RouteValues, request.Scheme, request.Host.ToUriComponent());

            if (string.IsNullOrEmpty(url))
                throw new InvalidOperationException("No routes matched!");

            context.HttpContext.Response.Headers["Location"] = url;
            context.HttpContext.Response.Headers["Date"] = Date.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            context.HttpContext.Request.Headers["ETag"] = ETag;
        }
    }
}