using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Razor.Runtime.TagHelpers;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http.Description;
using System.Xml.Linq;

namespace CommandCentral.Framework
{
    public class CustomDocumentFilter : IDocumentFilter
    {
        private static XDocument _documentation;
        private static ConcurrentDictionary<Type, List<string>> _typeSummaries;

        static CustomDocumentFilter()
        {
            string documentationPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "commandcentral.xml");

            if (!File.Exists(documentationPath))
                throw new FileNotFoundException("The xml documentation could not be found.  It should be named 'commandcentral.xml' and should be found colocated with the .exe.", documentationPath);

            _documentation = XDocument.Load(documentationPath);

            _typeSummaries = new ConcurrentDictionary<Type, List<string>>(_documentation.Descendants("doc")
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
            var apiGroups = apiDescriptions.Items.First().Items.ToLookup(x => ((ControllerActionDescriptor)x.ActionDescriptor));

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
