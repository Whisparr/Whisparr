using System;

namespace Whisparr.Http.REST.Attributes
{
    // Describes a multipart file upload for actions that read Request.Form.Files instead of binding a parameter
    [AttributeUsage(AttributeTargets.Method)]
    public class FileUploadAttribute : Attribute
    {
        public FileUploadAttribute(string fieldName)
        {
            FieldName = fieldName;
        }

        public string FieldName { get; }
    }
}
