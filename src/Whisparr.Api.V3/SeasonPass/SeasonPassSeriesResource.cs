﻿using System.Collections.Generic;
using Whisparr.Api.V3.Series;

namespace Whisparr.Api.V3.SeasonPass
{
    public class SeasonPassSeriesResource
    {
        public int Id { get; set; }
        public bool? Monitored { get; set; }
        public List<SeasonResource> Seasons { get; set; }
    }
}
