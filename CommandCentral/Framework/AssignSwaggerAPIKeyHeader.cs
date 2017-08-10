using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Filters;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommandCentral.Framework
{
    public class AssignSwaggerAPIKeyHeader : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            var apiKeyParameter = new NonBodyParameter
            {
                Name = "X-Api-Key",
                In = "header",
                Type = "string",
                Default = "E28235AC-57A1-42AC-AA85-1547B755EA7E",
                Description = "An API key identifies the calling application for metrics purposes.  Please do not change this.",
                Required = true
            };

            var sessionIdParameter = new NonBodyParameter
            {
                Name = "X-Session-Id",
                In = "header",
                Type = "string",
                Description = "A session id identifies a client's login session.  This will be automatically set for you when you click authenticate at the top of the page.",
                Required = true
            };

            if (operation.Parameters == null || !operation.Parameters.Any())
                operation.Parameters = new List<IParameter> { apiKeyParameter };
            else
                operation.Parameters.Add(apiKeyParameter);

            if (!(context.ApiDescription.HttpMethod == "POST" && context.ApiDescription.RelativePath.Contains("Authentication")))
            {
                operation.Parameters.Add(sessionIdParameter);
            }
        }
    }
}
