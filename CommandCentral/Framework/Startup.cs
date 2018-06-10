using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Swashbuckle.AspNetCore.Swagger;
using System.Reflection;
using FluentScheduler;

namespace CommandCentral.Framework
{
    /// <summary>
    /// This class is passed to the WebHostBuilder to start up our service.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// The date time format to be used throughout the application when parsing is required.
        /// </summary>
        public const string DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";

        /// <summary>
        /// The constructor for our class, which simply sets up Cron jobs.
        /// </summary>
        public Startup()
        {
            RegisterCronOperations();
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc(config =>
            {
                config.Filters.Add(typeof(GlobalExceptionFilter));
            })
            .AddJsonOptions(options =>
            {
                options.SerializerSettings.DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Utc;
                options.SerializerSettings.DateFormatHandling = Newtonsoft.Json.DateFormatHandling.IsoDateFormat;
                options.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented;
                options.SerializerSettings.DateFormatString = DateTimeFormat;
                options.SerializerSettings.Converters.Add(new StringEnumConverter { CamelCaseText = false });
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Error;
                options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            });

            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder =>
                    builder.AllowCredentials().AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            });

            services.ConfigureSwaggerGen(options =>
            {
                options.DescribeAllEnumsAsStrings();
                options.DescribeAllParametersInCamelCase();
                options.IncludeXmlComments(Utilities.ConfigurationUtility.XmlDocumentationPath);

                options.SwaggerDoc("v1", new Info
                {
                    Version = "v2.0.3",
                    Contact = new Contact
                    {
                        Email = "daniel.k.atwood@gmail.com",
                        Name = "Daniel Atwood",
                        Url = "http://commandcentral/"
                    },
                    Description = "The Command Central REST API provides common, standardized authentication, authorization, data access, and processing for U.S. Navy Sailor personnel and other administrative data.",
                    License = new License { Name = "The please don't freaking sue me license, 2017" },
                    TermsOfService = "This API is provided pretty much as is even though it's my job to ensure it works."
                });

                options.OperationFilter<AssignSwaggerAPIKeyHeader>();
                options.DocumentFilter<CustomDocumentFilter>();

                options.CustomSchemaIds(x => x.FullName);
            });

            services.AddSwaggerGen(options =>
            {
                options.DescribeAllEnumsAsStrings();
                options.DescribeAllParametersInCamelCase();
                options.IncludeXmlComments(Utilities.ConfigurationUtility.XmlDocumentationPath);

                options.CustomSchemaIds(x => x.FullName);
            });
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        /// <param name="loggerFactory"></param>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseCors("CorsPolicy");
            loggerFactory.AddConsole();
            loggerFactory.AddDebug();

            Log.Initialize(loggerFactory);

            app.UseMvc();
            app.UseStaticFiles();

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.RoutePrefix = "help";

                c.SwaggerEndpoint("/swagger/v1/swagger.json", "V1 Docs");
            });
        }

        private void RegisterCronOperations()
        {

            var registries = Assembly.GetExecutingAssembly()
                .GetTypes().Where(x => typeof(Registry).IsAssignableFrom(x))
                .Select(x => (Registry)Activator.CreateInstance(x));

            JobManager.Initialize(registries.ToArray());
        }
    }
}
