using System.Diagnostics.CodeAnalysis;
using Scalar.AspNetCore;
using TurtlePath.Template.Api.OpenApi;

namespace TurtlePath.Template.Api.DependencyInjection
{
    [ExcludeFromCodeCoverage]
    internal static class OpenApiExtensions
    {
        internal static IServiceCollection AddOpenApiDefaults(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddOpenApi(OpenApiConstants.Docs.ApiVersion, options =>
            {
                options.AddSchemaTransformer<CIdSchemaTransformer>();
            });

            return services;
        }

        internal static void UseOpenApiDefaults(this IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (!env.IsDevelopment())
                return;

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapOpenApi();
                endpoints.MapScalarApiReference(options =>
                {
                    options.Title = OpenApiConstants.Docs.ApiName;
                    options.DefaultHttpClient = new(ScalarTarget.CSharp, ScalarClient.HttpClient);
                    options.DisableAgent();
                });
            });
        }
    }
}
