using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace NzbDrone.Host.OpenApi
{
    // Nullable reference types are off, so ApiExplorer reports every [FromBody] parameter as optional
    public class RequiredRequestBodyOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.RequestBody is OpenApiRequestBody requestBody &&
                context.ApiDescription.ParameterDescriptions.Any(p => p.Source == BindingSource.Body))
            {
                requestBody.Required = true;
            }
        }
    }
}
