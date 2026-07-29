namespace TurtlePath.Swagger
{
    using Microsoft.OpenApi.Any;
    using Microsoft.OpenApi.Models;
    using Swashbuckle.AspNetCore.SwaggerGen;
    using TurtlePath.Identifier;

    /// <summary>
    /// Adjusts OpenAPI schemas for TurtlePath identifier values.
    /// </summary>
    public sealed class CIdSchemaFilter : ISchemaFilter
    {
        /// <inheritdoc/>
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (context.Type != typeof(CId) && context.Type != typeof(CId?))
                return;

            schema.Type = "string";
            schema.Format = "string";
            schema.Pattern = string.Empty;
            schema.Example = new OpenApiString(CId.Empty.ToString());
        }
    }
}
