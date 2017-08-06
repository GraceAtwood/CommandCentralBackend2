using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CommandCentral.Enums;
using Microsoft.AspNetCore.Mvc.Cors.Internal;
using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.Application;
using System.Reflection;

namespace CommandCentral.CLI
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
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
                config.Filters.Add(typeof(Framework.GlobalExceptionFilter));
            })
            .AddJsonOptions(options =>
            {
                options.SerializerSettings.DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Utc;
                options.SerializerSettings.DateFormatHandling = Newtonsoft.Json.DateFormatHandling.IsoDateFormat;
                options.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented;
                options.SerializerSettings.DateFormatString = "yyyy-MM-ddTHH:mm:ss.fffZ";
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
                options.IncludeXmlComments(@"bin\Debug\net47\win7-x86\commandcentral.xml");

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

                options.CustomSchemaIds(x => x.FullName);
            });

            services.AddSwaggerGen(options =>
            {
                options.DescribeAllEnumsAsStrings();
                options.DescribeAllParametersInCamelCase();
                options.IncludeXmlComments(@"bin\Debug\net47\win7-x86\commandcentral.xml");

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
                c.ShowJsonEditor();
                c.ShowRequestHeaders();
                c.RoutePrefix = "help";
               
                c.InjectOnCompleteJavaScript("/swagger-ui/basic-auth.js");

                c.SwaggerEndpoint("/swagger/v1/swagger.json", "V1 Docs");
            });
        }
    }
}
