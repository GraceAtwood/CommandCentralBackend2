using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace CommandCentral.Framework
{
    public class CustomDocumentFilter : IDocumentFilter
    {
        private static readonly ConcurrentDictionary<Type, List<string>> _typeSummaries;

        static CustomDocumentFilter()
        {
            if (!File.Exists(Utilities.ConfigurationUtility.XmlDocumentationPath))
                throw new FileNotFoundException("The xml documentation could not be found.  It should be named 'commandcentral.xml' and should be found colocated with the .exe.", Utilities.ConfigurationUtility.XmlDocumentationPath);

            var documentation = XDocument.Load(Utilities.ConfigurationUtility.XmlDocumentationPath);

            _typeSummaries = new ConcurrentDictionary<Type, List<string>>(documentation.Descendants("doc")
                .Descendants("members")
                .Descendants("member")
                .Where(x => x.Attribute("name").Value.StartsWith("T:"))
                .Select(x => new
                {
                    Type = Type.GetType($"{x.Attribute("name").Value.Substring(2)}, {Assembly.GetExecutingAssembly().FullName}"),
                    Summary = ParseXmlSummary(x.Descendants("summary").FirstOrDefault()).ToList()
                })
                .Where(x => x.Type != null)
                .ToDictionary(x => x.Type, x => x.Summary));
        }

        private static IEnumerable<string> ParseXmlSummary(XElement summary)
        {
            return summary.Value.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).Where(x => !String.IsNullOrWhiteSpace(x));
        }

        private void AddControllerDescriptions(SwaggerDocument swaggerDoc, ApiDescriptionGroupCollection apiDescriptions)
        {
            var apiGroups = apiDescriptions.Items.First().Items.ToLookup(x => (ControllerActionDescriptor)x.ActionDescriptor);

            var tags = new List<Tag>();
            foreach (var apiGroup in apiGroups)
            {
                var tag = new Tag
                {
                    Name = apiGroup.Key.ControllerName
                };

                tags.Add(tag);

                if (_typeSummaries.TryGetValue(apiGroup.Key.ControllerTypeInfo.UnderlyingSystemType, out List<string> summaryLines))
                {
                    tag.Description = "<br /><br />" + String.Join("<br /><br />", summaryLines);
                }
            }

            swaggerDoc.Tags = tags;
        }

        public void Apply(SwaggerDocument swaggerDoc, DocumentFilterContext context)
        {
            var paths = swaggerDoc.Paths.OrderBy(x => x.Key).ToList();
            swaggerDoc.Paths = paths.ToDictionary(x => x.Key, x => x.Value);

            AddControllerDescriptions(swaggerDoc, context.ApiDescriptionsGroups);
        }
        
    }
}
