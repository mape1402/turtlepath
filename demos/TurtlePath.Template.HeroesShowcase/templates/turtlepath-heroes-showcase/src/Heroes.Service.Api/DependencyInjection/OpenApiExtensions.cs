using Heroes.Service.Api.OpenApi;
using Scalar.AspNetCore;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Provides OpenAPI and Scalar documentation defaults.
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal static class OpenApiExtensions
    {
        /// <summary>
        /// Registers OpenAPI generation with TurtlePath identifier schema support.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>The service collection.</returns>
        internal static IServiceCollection AddOpenApiDefaults(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddOpenApi(OpenApiConstants.Docs.ApiVersion, options =>
            {
                options.AddSchemaTransformer<CIdSchemaTransformer>();
            });

            return services;
        }

        /// <summary>
        /// Maps the OpenAPI endpoint and Scalar API reference in development.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <param name="environment">The web host environment.</param>
        internal static void UseOpenApiDefaults(this IApplicationBuilder app, IWebHostEnvironment environment)
        {
            if (!environment.IsDevelopment())
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
