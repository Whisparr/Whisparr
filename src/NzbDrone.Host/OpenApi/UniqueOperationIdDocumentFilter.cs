using System;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace NzbDrone.Host.OpenApi
{
    /// <summary>
    /// Makes operation ids unique. CustomOperationIds names every operation after its controller
    /// and action, which collides where one action serves several routes or several methods.
    /// Collisions are broken by the last path segment first, then by the HTTP method.
    /// </summary>
    public class UniqueOperationIdDocumentFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            var operations = swaggerDoc.Paths
                .SelectMany(path => path.Value.Operations.Select(o => (Path: path.Key, Method: o.Key, Operation: o.Value)))
                .ToList();

            foreach (var collision in operations.GroupBy(o => o.Operation.OperationId).Where(g => g.Count() > 1).ToList())
            {
                var suffixed = collision.Select(o => (o.Method, o.Operation, Suffix: Discriminator(o.Path))).ToList();
                var stillAmbiguous = suffixed.GroupBy(o => o.Suffix).Where(g => g.Count() > 1).Select(g => g.Key).ToHashSet();

                foreach (var (method, operation, suffix) in suffixed)
                {
                    operation.OperationId += suffix;

                    if (stillAmbiguous.Contains(suffix))
                    {
                        operation.OperationId += Pascal(method.ToString());
                    }
                }
            }
        }

        private static string Discriminator(string path)
        {
            var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);

            if (segments.Length == 0)
            {
                return string.Empty;
            }

            var last = segments[^1];

            if (last.StartsWith('{'))
            {
                return "By" + Pascal(last.Trim('{', '}'));
            }

            // A literal tail that is the controller's own root segment says nothing the id does not
            // already say.
            var beyondVersion = segments.Length - (segments.Length > 1 && ApiVersion().IsMatch(segments[1]) ? 2 : 0);

            return beyondVersion <= 1 ? string.Empty : Pascal(last);
        }

        private static Regex ApiVersion() => new("^v[0-9]+$", RegexOptions.IgnoreCase);

        private static string Pascal(string value)
        {
            return char.ToUpperInvariant(value[0]) + value[1..].ToLowerInvariant();
        }
    }
}
