using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace NzbDrone.Host.OpenApi
{
    /// <summary>
    /// Clears the document level API key requirement on operations marked [AllowAnonymous].
    /// An empty security list on an operation overrides the document level one.
    /// </summary>
    public class AnonymousOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (context.ApiDescription.ActionDescriptor.EndpointMetadata.OfType<IAllowAnonymous>().Any())
            {
                operation.Security = new List<OpenApiSecurityRequirement>();
            }
        }
    }
}
