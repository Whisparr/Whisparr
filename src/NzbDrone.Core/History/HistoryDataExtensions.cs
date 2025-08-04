using System.Collections.Generic;

namespace NzbDrone.Core.History
{
    public static class HistoryDataExtensions
    {
        public static bool GetShouldOverride(this Dictionary<string, string> data)
        {
            return data?.GetValueOrDefault(HistoryDataKeys.ShouldOverride) == "true";
        }

        public static void SetShouldOverride(this Dictionary<string, string> data, bool value)
        {
            if (value)
            {
                data[HistoryDataKeys.ShouldOverride] = "true";
            }
            else
            {
                data.Remove(HistoryDataKeys.ShouldOverride);
            }
        }
    }
}
