using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using Whisparr.Api.V3.Commands;

namespace NzbDrone.Host.OpenApi
{
    // CommandController.StartCommand re-reads the body as the concrete command, so arguments such as
    // seriesIds are accepted even though CommandResource does not declare them
    public class CommandResourceSchemaFilter : ISchemaFilter
    {
        public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
        {
            if (context.Type == typeof(CommandResource) && schema is OpenApiSchema openApiSchema)
            {
                openApiSchema.AdditionalPropertiesAllowed = true;
                openApiSchema.AdditionalProperties = null;
            }
        }
    }
}
