using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace NzbDrone.Core.ImportLists.WhisparrList
{
    public class WhisparrListException : Exception
    {
        public WhisparrErrors APIErrors;

        public WhisparrListException(WhisparrErrors apiError)
            : base(HumanReadable(apiError))
        {
            APIErrors = apiError;
        }

        private static string HumanReadable(WhisparrErrors apiErrors)
        {
            var firstError = apiErrors.Errors.First();
            var details = string.Join("\n", apiErrors.Errors.Select(error =>
            {
                return $"{error.Title} ({error.Status}, RayId: {error.RayId}), Details: {error.Detail}";
            }));
            return $"Error while calling api: {firstError.Title}\nFull error(s): {details}";
        }
    }

    public class WhisparrError
    {
        [JsonProperty("id")]
        public string RayId { get; set; }

        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("detail")]
        public string Detail { get; set; }
    }

    public class WhisparrErrors
    {
        [JsonProperty("errors")]
        public IList<WhisparrError> Errors { get; set; }
    }
}
