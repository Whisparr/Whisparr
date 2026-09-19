using System.Collections.Generic;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using Whisparr.Api.V3.System.Backup;

namespace NzbDrone.Host.OpenApi
{
    // UploadAndRestore reads Request.Form.Files, so ApiExplorer sees no body.
    // Any field name works; "restore" is what the UI sends
    public class BackupRestoreUploadOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (context.MethodInfo.DeclaringType != typeof(BackupController) ||
                context.MethodInfo.Name != nameof(BackupController.UploadAndRestore))
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
                            Properties = new Dictionary<string, IOpenApiSchema>
                            {
                                ["restore"] = new OpenApiSchema { Type = JsonSchemaType.String, Format = "binary" }
                            },
                            Required = new HashSet<string> { "restore" }
                        }
                    }
                }
            };
        }
    }
}
