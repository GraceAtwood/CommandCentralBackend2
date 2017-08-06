using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Razor.Runtime.TagHelpers;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http.Description;

namespace CommandCentral
{
    public class CustomDocumentFilter : IDocumentFilter
    {
        private static void AddControllerDescriptions(SwaggerDocument swaggerDoc, ApiDescriptionGroupCollection apiDescriptions)
        {
            var doc = new XmlDocumentationProvider(GetXmlCommentsPath());

            List<Tag> lst = new List<Tag>();

            var apiGroups = apiDescriptions.Items.First().Items.ToLookup(x => ((ControllerActionDescriptor)x.ActionDescriptor));
            
            foreach (var apiGroup in apiGroups)
            {
                var tag = new Tag { Name = apiGroup.Key.ControllerName };
                var test = XmlDocumentationProvider.GetId(apiGroup.Key.ControllerTypeInfo);
                var apiDoc = doc.GetSummary(XmlDocumentationProvider.GetId(apiGroup.Key.ControllerTypeInfo));
                if (!String.IsNullOrWhiteSpace(apiDoc))
                    tag.Description = apiDoc;
                lst.Add(tag);

            }

            if (lst.Count() > 0)
                swaggerDoc.Tags = lst.ToList();
        }

        private static string GetXmlCommentsPath()
        {
            return @"bin\Debug\net47\win7-x86\commandcentral.xml";
        }

        public void Apply(SwaggerDocument swaggerDoc, DocumentFilterContext context)
        {
            var paths = swaggerDoc.Paths.OrderBy(x => x.Key).ToList();
            swaggerDoc.Paths = paths.ToDictionary(x => x.Key, x => x.Value);

            AddControllerDescriptions(swaggerDoc, context.ApiDescriptionsGroups);
        }
    }
}
