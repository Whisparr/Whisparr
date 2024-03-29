using System;
using System.Collections.Generic;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Qualities;
using Whisparr.Api.V3.CustomFormats;
using Whisparr.Api.V3.Series;
using Whisparr.Http.REST;

namespace Whisparr.Api.V3.Blocklist
{
    public class BlocklistResource : RestResource
    {
        public int SeriesId { get; set; }
        public List<int> EpisodeIds { get; set; }
        public string SourceTitle { get; set; }
        public List<Language> Languages { get; set; }
        public QualityModel Quality { get; set; }
        public List<CustomFormatResource> CustomFormats { get; set; }
        public DateTime Date { get; set; }
        public DownloadProtocol Protocol { get; set; }
        public string Indexer { get; set; }
        public string Message { get; set; }

        public SeriesResource Series { get; set; }
    }

    public static class BlocklistResourceMapper
    {
        public static BlocklistResource MapToResource(this NzbDrone.Core.Blocklisting.Blocklist model, ICustomFormatCalculationService formatCalculator)
        {
            if (model == null)
            {
                return null;
            }

            return new BlocklistResource
            {
                Id = model.Id,

                SeriesId = model.SeriesId,
                EpisodeIds = model.EpisodeIds,
                SourceTitle = model.SourceTitle,
                Languages = model.Languages,
                Quality = model.Quality,
                CustomFormats = formatCalculator.ParseCustomFormat(model, model.Series).ToResource(false),
                Date = model.Date,
                Protocol = model.Protocol,
                Indexer = model.Indexer,
                Message = model.Message,

                Series = model.Series.ToResource()
            };
        }
    }
}
