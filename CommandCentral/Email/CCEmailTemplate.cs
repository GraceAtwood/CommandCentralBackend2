﻿using System.IO;
using System.Reflection;
using RazorEngine;
using RazorEngine.Templating;

namespace CommandCentral.Email
{
    /// <summary>
    /// An email template that can be rendered with an instance of <see cref="TModel"/>
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    public class CCEmailTemplate<TModel>
    {
        /// <summary>
        /// The name of the template as given during instantiation.
        /// </summary>
        public string TemplateName { get; }

        /// <summary>
        /// The underlying template runner that is used to render the templates.
        /// </summary>
        private readonly ITemplateRunner<TModel> _templateRunner;

        /// <summary>
        /// Creates a new template and loads the text template from the resources manifest.
        /// </summary>
        /// <param name="templateName"></param>
        /// <exception cref="FileNotFoundException"></exception>
        public CCEmailTemplate(string templateName)
        {
            TemplateName = templateName;
            
            using (var resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("CommandCentral.Email.TextTemplates." + templateName))
            {
                if (resourceStream == null)
                    throw new  FileNotFoundException("The resource could not be found.", "CommandCentral.Email.TextTemplates." + templateName);
                
                using (var reader = new StreamReader(resourceStream))
                {
                    _templateRunner = Engine.Razor.CompileRunner<TModel>(reader.ReadToEnd());
                }
            }
        }

        /// <summary>
        /// Renders the underlying template against the given model.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public string Render(TModel model)
        {
            return _templateRunner.Run(model);
        }
    }
}