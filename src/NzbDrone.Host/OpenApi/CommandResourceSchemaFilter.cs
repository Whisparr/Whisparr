using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using Whisparr.Api.V3.Commands;

namespace NzbDrone.Host.OpenApi
{
    /// <summary>
    /// Allows properties beyond those on CommandResource. POST /api/v3/command re-reads the raw
    /// request body and deserialises it into the concrete Command type, so the arguments a command
    /// requires are not on the resource.
    /// </summary>
    public class CommandResourceSchemaFilter : ISchemaFilter
    {
        public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
        {
            if (context.Type == typeof(CommandResource) && schema is OpenApiSchema concrete)
            {
                concrete.AdditionalPropertiesAllowed = true;
            }
        }
    }
}
