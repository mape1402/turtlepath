using System.Diagnostics.CodeAnalysis;
using Asp.Versioning.ApiExplorer;
using DTemplate.Api.Swagger;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DTemplate.Api.DependencyInjection
{
    [ExcludeFromCodeCoverage]
    internal static class SwaggerExtensions
    {
        internal static IServiceCollection AddSwaggerDefaults(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
            services.AddSwaggerGen(opts =>
            {
                opts.ResolveConflictingActions(x => x.First());
                opts.OperationFilter<RemoveVersionParametersFilter>();
                opts.DocumentFilter<SetVersionInPathsFilter>();
                opts.SchemaFilter<CIdSchemaFilter>();

                foreach (var fileName in Directory.GetFiles(AppContext.BaseDirectory, "*.xml"))
                    opts.IncludeXmlComments(fileName);
            });

            return services;
        }

        internal static void UseSwaggerDefaults(this IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (!env.IsDevelopment())
                return;

            app.UseSwagger(opts =>
            {
                opts.RouteTemplate = "/swagger/{documentName}/swagger.json";
            });

            app.UseSwaggerUI(opts =>
            {
                opts.RoutePrefix = string.Empty;
                opts.DocumentTitle = SwaggerConstants.Docs.ApiName;

                //Display
                opts.DefaultModelExpandDepth(3);
                opts.DefaultModelsExpandDepth(0);
                opts.DisplayRequestDuration();
                opts.EnableDeepLinking();
                opts.ShowExtensions();

                // Network
                opts.EnableValidator();

                if (env.IsDevelopment())
                {
                    var provider = app.ApplicationServices.GetRequiredService<IApiVersionDescriptionProvider>();

                    foreach (var description in provider.ApiVersionDescriptions)
                    {
                        opts.SwaggerEndpoint(
                            $"/swagger/{description.GroupName}/swagger.json",
                            $"{SwaggerConstants.Docs.ApiName} {description.GroupName}");
                    }
                }
            });
        }
    }
}
