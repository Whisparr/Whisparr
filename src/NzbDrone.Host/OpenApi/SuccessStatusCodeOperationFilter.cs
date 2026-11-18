using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using Whisparr.Http.REST.Attributes;

namespace NzbDrone.Host.OpenApi
{
    // [RestPostById] actions answer through RestController.Created and [RestPutById] actions through
    // RestController.Accepted, but ApiExplorer only sees the declared return type and documents a 200.
    // Moving that response keeps the schema inferred from ActionResult<T>.
    public class SuccessStatusCodeOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var statusCode = GetStatusCode(context.MethodInfo);

            if (statusCode == null || operation.Responses == null || !operation.Responses.Remove("200", out var response))
            {
                return;
            }

            if (response is OpenApiResponse openApiResponse)
            {
                openApiResponse.Description = ReasonPhrases.GetReasonPhrase(statusCode.Value);
            }

            operation.Responses[statusCode.Value.ToString()] = response;
        }

        private static int? GetStatusCode(MethodInfo method)
        {
            if (method.GetCustomAttribute<RestPostByIdAttribute>(true) != null)
            {
                return StatusCodes.Status201Created;
            }

            if (method.GetCustomAttribute<RestPutByIdAttribute>(true) != null)
            {
                return StatusCodes.Status202Accepted;
            }

            return null;
        }
    }
}
