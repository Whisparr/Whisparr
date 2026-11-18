using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace NzbDrone.Host.OpenApi
{
    // Nullable reference types are off, so ApiExplorer reports every [FromBody] parameter as optional,
    // but empty input is not allowed in body model binding and the request is rejected before the action runs
    public class RequiredRequestBodyFilter : IRequestBodyFilter
    {
        public void Apply(IOpenApiRequestBody requestBody, RequestBodyFilterContext context)
        {
            if (context.BodyParameterDescription != null && requestBody is OpenApiRequestBody openApiRequestBody)
            {
                openApiRequestBody.Required = true;
            }
        }
    }
}
