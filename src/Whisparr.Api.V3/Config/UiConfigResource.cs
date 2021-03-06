using NzbDrone.Core.Configuration;
using Whisparr.Http.REST;

namespace Whisparr.Api.V3.Config
{
    public class UiConfigResource : RestResource
    {
        //Calendar
        public int FirstDayOfWeek { get; set; }
        public string CalendarWeekColumnHeader { get; set; }

        // Movies
        public MovieRuntimeFormatType MovieRuntimeFormat { get; set; }

        //Dates
        public string ShortDateFormat { get; set; }
        public string LongDateFormat { get; set; }
        public string TimeFormat { get; set; }
        public bool ShowRelativeDates { get; set; }

        public bool EnableColorImpairedMode { get; set; }
        public int MovieInfoLanguage { get; set; }
        public int UILanguage { get; set; }
    }

    public static class UiConfigResourceMapper
    {
        public static UiConfigResource ToResource(IConfigService model)
        {
            return new UiConfigResource
            {
                FirstDayOfWeek = model.FirstDayOfWeek,
                CalendarWeekColumnHeader = model.CalendarWeekColumnHeader,

                MovieRuntimeFormat = model.MovieRuntimeFormat,

                ShortDateFormat = model.ShortDateFormat,
                LongDateFormat = model.LongDateFormat,
                TimeFormat = model.TimeFormat,
                ShowRelativeDates = model.ShowRelativeDates,

                EnableColorImpairedMode = model.EnableColorImpairedMode,
                MovieInfoLanguage = model.MovieInfoLanguage,
                UILanguage = model.UILanguage
            };
        }
    }
}
