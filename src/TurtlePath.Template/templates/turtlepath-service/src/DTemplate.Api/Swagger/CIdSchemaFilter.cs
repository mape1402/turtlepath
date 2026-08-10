namespace DTemplate.Api.Swagger
{
    using Microsoft.OpenApi;
    using Swashbuckle.AspNetCore.SwaggerGen;
    using TurtlePath.Domain.Identifier;

    /// <summary>
    /// Adjusts the schema for <see cref="CId"/> types in Swagger.
    /// </summary>
    public class CIdSchemaFilter : ISchemaFilter
    {
        /// <summary>
        /// Applies the schema customization.
        /// </summary>
        /// <param name="schema">The schema to update.</param>
        /// <param name="context">The schema filter context.</param>
        public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
        {
            if (context.Type == typeof(CId) && schema is OpenApiSchema openApiSchema)
            {
                openApiSchema.Type = JsonSchemaType.String;
                openApiSchema.Format = "string";
                openApiSchema.Pattern = "";
                openApiSchema.Example = Ulid.NewUlid().ToString();
            }
        }
    }
}
