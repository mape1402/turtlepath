using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DTemplate.Api.Swagger
{
    public class RemoveVersionParametersFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // Remove version parameter from all Operations
            var versionParameter = operation.Parameters.FirstOrDefault(p => p.Name == "version");
            if (versionParameter != null)
                operation.Parameters.Remove(versionParameter);
        }
    }
}
