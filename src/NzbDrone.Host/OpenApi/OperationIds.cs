using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace NzbDrone.Host.OpenApi
{
    public static class OperationIds
    {
        private const string ApiPrefix = "api/v3/";

        private static readonly Regex ParameterRegex = new Regex(@"^\{(?<name>[^:?}]+)[^}]*\}$", RegexOptions.Compiled);
        private static readonly Regex WordSeparatorRegex = new Regex("[^A-Za-z0-9]+", RegexOptions.Compiled);

        // Built from the method and path rather than the controller action, so ids only change when the
        // public route does, not when an action is renamed. GET api/v3/tag/{id} becomes getTagById.
        public static string FromRoute(string httpMethod, string relativePath)
        {
            var path = relativePath ?? string.Empty;

            if (path.StartsWith(ApiPrefix, StringComparison.OrdinalIgnoreCase))
            {
                path = path.Substring(ApiPrefix.Length);
            }

            var operationId = new StringBuilder((httpMethod ?? string.Empty).ToLowerInvariant());

            foreach (var segment in path.Split('/', StringSplitOptions.RemoveEmptyEntries))
            {
                var parameter = ParameterRegex.Match(segment);

                if (parameter.Success)
                {
                    operationId.Append("By");
                    AppendWords(operationId, parameter.Groups["name"].Value);
                }
                else
                {
                    AppendWords(operationId, segment);
                }
            }

            return operationId.ToString();
        }

        private static void AppendWords(StringBuilder builder, string text)
        {
            foreach (var word in WordSeparatorRegex.Split(text).Where(w => w.Length > 0))
            {
                builder.Append(char.ToUpperInvariant(word[0]));
                builder.Append(word, 1, word.Length - 1);
            }
        }
    }
}
