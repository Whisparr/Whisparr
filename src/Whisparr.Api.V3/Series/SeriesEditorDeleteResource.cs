﻿using System.Collections.Generic;

namespace Whisparr.Api.V3.Series
{
    public class SeriesEditorDeleteResource
    {
        public List<int> SeriesIds { get; set; }
        public bool DeleteFiles { get; set; }
    }
}
