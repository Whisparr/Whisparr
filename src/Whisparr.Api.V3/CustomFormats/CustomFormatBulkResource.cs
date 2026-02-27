using System.Collections.Generic;

namespace Whisparr.Api.V3.CustomFormats
{
    public class CustomFormatBulkResource
    {
        public HashSet<int> Ids { get; set; } = new ();
        public bool? IncludeCustomFormatWhenRenaming { get; set; }
    }
}
