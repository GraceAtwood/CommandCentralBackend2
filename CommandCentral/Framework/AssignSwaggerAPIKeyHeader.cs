using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Linq;

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
                Description =
                    "An API key identifies the calling application for metrics purposes.  Please do not change this.",
                Required = true
            };

            var impersonatePersonIdParameter = new NonBodyParameter
            {
                Name = "X-Impersonate-Person-Id",
                In = "header",
                Type = "string",
                Description =
                    "If choosing not to send a client certificate, you must send the person id of the person " +
                    "you want to log in as.  Only valid if the application is in debug mode.",
                Required = false
            };

            if (operation.Parameters == null || !operation.Parameters.Any())
                operation.Parameters = new List<IParameter> {apiKeyParameter};
            else
                operation.Parameters.Add(apiKeyParameter);
            
            operation.Parameters.Add(impersonatePersonIdParameter);
        }
    }
}