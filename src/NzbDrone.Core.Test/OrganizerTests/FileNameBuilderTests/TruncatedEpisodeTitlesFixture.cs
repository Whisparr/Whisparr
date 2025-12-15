using System.Collections.Generic;
using System.IO;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.OrganizerTests.FileNameBuilderTests
{
    [TestFixture]

    public class TruncatedEpisodeTitlesFixture : CoreTest<FileNameBuilder>
    {
        private Series _series;
        private List<Episode> _episodes;
        private EpisodeFile _episodeFile;
        private NamingConfig _namingConfig;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Series>
                    .CreateNew()
                    .With(s => s.Title = "Series Title")
                    .Build();

            _namingConfig = NamingConfig.Default;
            _namingConfig.MultiEpisodeStyle = 0;
            _namingConfig.RenameEpisodes = true;

            Mocker.GetMock<INamingConfigService>()
                  .Setup(c => c.GetConfig()).Returns(_namingConfig);

            _episodes = new List<Episode>
                        {
                            Builder<Episode>.CreateNew()
                                            .With(e => e.Title = "Episode Title 1")
                                            .With(e => e.SeasonNumber = 1)
                                            .With(e => e.AirDate = "2016-05-06")
                                            .Build(),

                            Builder<Episode>.CreateNew()
                                            .With(e => e.Title = "Another Episode Title")
                                            .With(e => e.SeasonNumber = 1)
                                            .With(e => e.AirDate = "2016-05-06")
                                            .Build(),

                            Builder<Episode>.CreateNew()
                                            .With(e => e.Title = "Yet Another Episode Title")
                                            .With(e => e.SeasonNumber = 1)
                                            .With(e => e.AirDate = "2016-05-06")
                                            .Build(),

                            Builder<Episode>.CreateNew()
                                            .With(e => e.Title = "Yet Another Episode Title Take 2")
                                            .With(e => e.SeasonNumber = 1)
                                            .With(e => e.AirDate = "2016-05-06")
                                            .Build(),

                            Builder<Episode>.CreateNew()
                                            .With(e => e.Title = "Yet Another Episode Title Take 3")
                                            .With(e => e.SeasonNumber = 1)
                                            .With(e => e.AirDate = "2016-05-06")
                                            .Build(),

                            Builder<Episode>.CreateNew()
                                            .With(e => e.Title = "Yet Another Episode Title Take 4")
                                            .With(e => e.SeasonNumber = 1)
                                            .With(e => e.AirDate = "2016-05-06")
                                            .Build(),

                            Builder<Episode>.CreateNew()
                                            .With(e => e.Title = "A Really Really Really Really Long Episode Title")
                                            .With(e => e.SeasonNumber = 1)
                                            .With(e => e.AirDate = "2016-05-06")
                                            .Build()
                        };

            _episodeFile = new EpisodeFile { Quality = new QualityModel(Quality.HDTV720p), ReleaseGroup = "WhisparrTest" };

            Mocker.GetMock<IQualityDefinitionService>()
                .Setup(v => v.Get(Moq.It.IsAny<Quality>()))
                .Returns<Quality>(v => Quality.DefaultQualityDefinitions.First(c => c.Quality == v));

            Mocker.GetMock<ICustomFormatService>()
                  .Setup(v => v.All())
                  .Returns(new List<CustomFormat>());
        }

        private void GivenProper()
        {
            _episodeFile.Quality.Revision.Version = 2;
        }

        [Test]
        public void should_truncate_with_extension()
        {
            _series.Title = "The Fantastic Life of Mr. Sisko";

            _episodes[0].SeasonNumber = 2;
            _episodes[0].Title = "This title has to be 197 characters in length, combined with the series title, quality and episode number it becomes 254ish and the extension puts it above the 255 limit and triggers the truncation";
            _episodeFile.Quality.Quality = Quality.Bluray1080p;
            _episodes = _episodes.Take(1).ToList();
            _namingConfig.StandardEpisodeFormat = "{Site Title} - {Release Date} - {Episode Title} {Quality Full}";

            var result = Subject.BuildFileName(_episodes, _series, _episodeFile, ".mkv");
            result.Length.Should().BeLessOrEqualTo(235);
            result.Should().Be("The Fantastic Life of Mr. Sisko - 2016 05 06 - This title has to be 197 characters in length, combined with the series title, quality and episode number it becomes 254ish and the extension puts it above the 255 limi... Bluray-1080p.mkv");
        }

        [Test]
        public void should_truncate_with_ellipsis_between_first_and_last_episode_titles()
        {
            _namingConfig.StandardEpisodeFormat = "{Site Title} - {Release Date} - {Episode Title} {Quality Full}";

            var result = Subject.BuildFileName(_episodes, _series, _episodeFile);
            result.Length.Should().BeLessOrEqualTo(235);
            result.Should().Be("Series Title - 2016 05 06 - Episode Title 1...A Really Really Really Really Long Episode Title HDTV-720p");
        }

        [Test]
        public void should_truncate_with_ellipsis_if_only_first_episode_title_fits()
        {
            _series.Title = "Lorem ipsum dolor sit amet, consectetur adipiscing elit Maecenas et magna sem Morbi vitae volutpat quam, id porta arcu Orci varius natoque";
            _namingConfig.StandardEpisodeFormat = "{Site Title} - {Release Date} - {Episode Title} {Quality Full}";

            var result = Subject.BuildFileName(_episodes, _series, _episodeFile);
            result.Length.Should().BeLessOrEqualTo(235);
            result.Should().Be("Lorem ipsum dolor sit amet, consectetur adipiscing elit Maecenas et magna sem Morbi vitae volutpat quam, id porta arcu Orci varius natoque - 2016 05 06 - Episode Title 1...A Really Really Really Really Long Episode Title HDTV-720p");
        }

        [Test]
        public void should_truncate_first_episode_title_with_ellipsis_if_only_partially_fits()
        {
            _series.Title = "Lorem ipsum dolor sit amet, consectetur adipiscing elit Maecenas et magna sem Morbi vitae volutpat quam, id porta arcu Orci varius natoque penatibus et magnis dis parturient montes nascetur ridiculus";
            _namingConfig.StandardEpisodeFormat = "{Site Title} - {Release Date} - {Episode Title} {Quality Full}";

            var result = Subject.BuildFileName(new List<Episode> { _episodes.First() }, _series, _episodeFile);
            result.Length.Should().BeLessOrEqualTo(235);
            result.Should().Be("Lorem ipsum dolor sit amet, consectetur adipiscing elit Maecenas et magna sem Morbi vitae volutpat quam, id porta arcu Orci varius natoque penatibus et magnis dis parturient montes nascetur ridiculus - 2016 05 06 - Episode... HDTV-720p");
        }

        [Test]
        public void should_truncate_titles_measuring_series_title_bytes()
        {
            _series.Title = "Lor\u00E9m ipsum dolor sit amet, consectetur adipiscing elit Maecenas et magna sem Morbi vitae volutpat quam, id porta arcu Orci varius natoque penatibus et magnis dis parturient montes nascetur ridiculus";
            _namingConfig.StandardEpisodeFormat = "{Site Title} - {Release Date} - {Episode Title} {Quality Full}";

            var result = Subject.BuildFileName(new List<Episode> { _episodes.First() }, _series, _episodeFile);
            result.GetByteCount().Should().BeLessOrEqualTo(235);

            result.Should().Be("Lor\u00E9m ipsum dolor sit amet, consectetur adipiscing elit Maecenas et magna sem Morbi vitae volutpat quam, id porta arcu Orci varius natoque penatibus et magnis dis parturient montes nascetur ridiculus - 2016 05 06 - Episod... HDTV-720p");
        }

        [Test]
        public void should_truncate_titles_measuring_episode_title_bytes()
        {
            _series.Title = "Lorem ipsum dolor sit amet, consectetur adipiscing elit Maecenas et magna sem Morbi vitae volutpat quam, id porta arcu Orci varius natoque penatibus et magnis dis parturient montes nascetur ridiculus";
            _namingConfig.StandardEpisodeFormat = "{Site Title} - {Release Date} - {Episode Title} {Quality Full}";

            _episodes.First().Title = "Episod\u00E9 Title";

            var result = Subject.BuildFileName(new List<Episode> { _episodes.First() }, _series, _episodeFile);
            result.GetByteCount().Should().BeLessOrEqualTo(235);

            result.Should().Be("Lorem ipsum dolor sit amet, consectetur adipiscing elit Maecenas et magna sem Morbi vitae volutpat quam, id porta arcu Orci varius natoque penatibus et magnis dis parturient montes nascetur ridiculus - 2016 05 06 - Episod... HDTV-720p");
        }

        [Test]
        public void should_truncate_titles_measuring_episode_title_bytes_middle()
        {
            _series.Title = "Lorem ipsum dolor sit amet, consectetur adipiscing elit Maecenas et magna sem Morbi vitae volutpat quam, id porta arcu Orci varius natoque penatibus et magnis dis parturient montes nascetur ridiculus";
            _namingConfig.StandardEpisodeFormat = "{Site Title} - {Release Date} - {Episode Title} {Quality Full}";

            _episodes.First().Title = "Episode T\u00E9tle";

            var result = Subject.BuildFileName(new List<Episode> { _episodes.First() }, _series, _episodeFile);
            result.GetByteCount().Should().BeLessOrEqualTo(235);

            result.Should().Be("Lorem ipsum dolor sit amet, consectetur adipiscing elit Maecenas et magna sem Morbi vitae volutpat quam, id porta arcu Orci varius natoque penatibus et magnis dis parturient montes nascetur ridiculus - 2016 05 06 - Episode... HDTV-720p");
        }

        [Test]
        public void should_smartly_truncate_only_episode_title_component()
        {
            _series.Title = "Short Series Title";
            _namingConfig.StandardEpisodeFormat = "{Site Title} - {Release Date} - {Episode Title} - {Quality Full}";

            _episodes.First().Title = "This is an extremely long episode title that definitely needs to be truncated because it makes the filename exceed the 255 character limit when combined with all other components including series title release date and quality information which should all be preserved completely while only the episode title gets truncated smartly";

            var result = Subject.BuildFileName(new List<Episode> { _episodes.First() }, _series, _episodeFile);
            result.GetByteCount().Should().BeLessOrEqualTo(235);

            // Should contain ellipsis indicating truncation occurred
            result.Should().Contain("...");

            // Should start with series title and date (preserved components)
            result.Should().StartWith("Short Series Title - 2016 05 06 -");

            // Should end with quality info and ellipsis since episode title was truncated
            result.Should().EndWith("... - HDTV-720p");

            // Verify key components are preserved
            result.Should().Contain("Short Series Title");
            result.Should().Contain("2016 05 06");
            result.Should().Contain("HDTV-720p");
        }

        [Test]
        public void should_smartly_truncate_longest_component_when_episode_title_identified()
        {
            _series.Title = "Series";
            _namingConfig.StandardEpisodeFormat = "{Site Title}/{Release Date}/{Episode Title}/{Quality Full}";

            _episodes.First().Title = "A Really Really Really Really Really Really Really Really Really Really Really Really Really Really Really Really Really Really Really Really Really Really Really Really Really Really Really Really Really Really Really Really Really Really Really Really Really Really Really Really Really Really Really Really Really Long Episode Title That Should Be Truncated";

            var result = Subject.BuildFileName(new List<Episode> { _episodes.First() }, _series, _episodeFile);
            result.GetByteCount().Should().BeLessOrEqualTo(235);

            // Should contain truncated episode title with ellipsis
            result.Should().Contain("...");
            result.Should().EndWith($"{Path.DirectorySeparatorChar}HDTV-720p");

            // Other components should remain intact
            result.Should().StartWith($"Series{Path.DirectorySeparatorChar}2016 05 06{Path.DirectorySeparatorChar}");
        }

        [Test]
        public void should_fallback_to_simple_truncation_when_no_episode_title_component_found()
        {
            _series.Title = "A Very Long Series Title That Takes Up Most Of The Filename Space And Definitely Exceeds The Character Limit For Filenames When Combined With Other Components Like Release Date And Quality Information Making The Entire Filename Too Long For The Filesystem";
            _namingConfig.StandardEpisodeFormat = "{Site Title} - {Release Date} - {Quality Full}";

            // No episode title in format, so should fallback to simple truncation
            var result = Subject.BuildFileName(new List<Episode> { _episodes.First() }, _series, _episodeFile);
            result.GetByteCount().Should().BeLessOrEqualTo(235);

            // Should contain ellipsis indicating truncation occurred
            result.Should().Contain("...");

            // With simple truncation, the entire string is truncated and may not end with the expected format
            result.Should().StartWith("A Very Long Series Title That Takes Up Most Of The Filename Space And Definitely Exceeds The Character Limit For Filenames");
        }

        [Test]
        public void should_fallback_to_simple_truncation_when_non_title_components_too_long()
        {
            // Create a series title that is so long that even without episode title, it exceeds the limit
            _series.Title = "An Extremely Long Series Title That Takes Up So Much Space That Even Without Episode Title The Filename Would Be Too Long For The Filesystem Limit And Needs Much More Characters To Actually Trigger The Truncation Logic Because The Current Length Is Still Under The 255 Character Limit So We Need To Add Even More Text Here To Make Sure It Goes Over The Limit When Combined With Date And Quality Information Which Should Push It Past 255 Characters Total";
            _namingConfig.StandardEpisodeFormat = "{Site Title} - {Release Date} - {Episode Title} - {Quality Full}";

            _episodes.First().Title = "Short";

            var result = Subject.BuildFileName(new List<Episode> { _episodes.First() }, _series, _episodeFile);
            result.GetByteCount().Should().BeLessOrEqualTo(235);

            // Should contain ellipsis indicating truncation occurred
            result.Should().Contain("...");

            // With fallback to simple truncation, the entire string gets truncated
            result.Should().StartWith("An Extremely Long Series Title That Takes Up So Much Space That Even Without Episode Title The Filename Would Be Too Long For The");
        }

        [Test]
        public void should_preserve_file_extension_in_smart_truncation()
        {
            _series.Title = "Series";
            _namingConfig.StandardEpisodeFormat = "{Site Title} - {Episode Title} - {Quality Full}";

            _episodes.First().Title = "A Very Long Episode Title That Should Be Truncated While Preserving The File Extension And Other Components That Make The Filename Way Too Long For The Filesystem To Handle Properly When Combined With The Extension And All The Other Components Which Need To Be Much Longer To Actually Trigger The Smart Truncation Logic Because Currently The Generated Filename Is Still Under The 255 Character Limit And We Need More Text To Push It Over";

            var result = Subject.BuildFileName(new List<Episode> { _episodes.First() }, _series, _episodeFile, ".mkv");
            result.GetByteCount().Should().BeLessOrEqualTo(235);

            result.Should().EndWith(".mkv");
            result.Should().Contain("...");
            result.Should().StartWith("Series -");
            result.Should().Contain("- HDTV-720p");
        }

        [Test]
        public void should_truncate_episode_title_at_word_boundary_when_possible()
        {
            _series.Title = "Series";
            _namingConfig.StandardEpisodeFormat = "{Site Title} - {Episode Title} - {Quality Full}";

            _episodes.First().Title = "This is a very long episode title with multiple words that should be truncated smartly at word boundaries when the filename becomes too long for filesystem limits and needs to have much more text to actually trigger the truncation logic in the smart filename builder because the current length is still under the required limit to activate the truncation functionality and we need even more words here to make it really really really really really really really really really really really really really really really really really really really really really really really long so that it definitely exceeds the character limit";

            var result = Subject.BuildFileName(new List<Episode> { _episodes.First() }, _series, _episodeFile);
            result.GetByteCount().Should().BeLessOrEqualTo(235);

            result.Should().Contain("...");
            result.Should().EndWith("- HDTV-720p");

            // The truncated part should not end mid-word (though this is more of an implementation detail)
            var truncatedPart = result.Substring(result.IndexOf(" - ") + 3, result.LastIndexOf("...") - result.IndexOf(" - ") - 3);
            truncatedPart.Should().NotEndWith(" ");
        }
    }
}
