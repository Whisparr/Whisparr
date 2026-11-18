using System.Collections.Generic;
using System.Reflection;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using Whisparr.Http.REST.Attributes;

namespace NzbDrone.Host.OpenApi
{
    // ApiExplorer has no parameter to describe when an action reads Request.Form.Files directly
    public class FileUploadOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var fileUpload = context.MethodInfo.GetCustomAttribute<FileUploadAttribute>();

            if (fileUpload == null)
            {
                return;
            }

            operation.RequestBody = new OpenApiRequestBody
            {
                Required = true,
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["multipart/form-data"] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = JsonSchemaType.Object,
                            Required = new HashSet<string> { fileUpload.FieldName },
                            Properties = new Dictionary<string, IOpenApiSchema>
                            {
                                [fileUpload.FieldName] = new OpenApiSchema { Type = JsonSchemaType.String, Format = "binary" }
                            }
                        }
                    }
                }
            };
        }
    }
}
