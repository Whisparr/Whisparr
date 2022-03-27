using System;
using Microsoft.AspNetCore.Mvc;

namespace Whisparr.Http.REST.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RestPostByIdAttribute : HttpPostAttribute
    {
    }
}
