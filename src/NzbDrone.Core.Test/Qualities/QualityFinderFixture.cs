using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Test.Qualities
{
    [TestFixture]
    public class QualityFinderFixture
    {
        [TestCase(QualitySource.DVDRaw, 480)]
        public void should_return_DVD_Remux(QualitySource source, int resolution)
        {
            QualityFinder.FindBySourceAndResolution(source, resolution).Should().Be(Quality.DVDR);
        }

        [TestCase(QualitySource.DVD, 480)]
        [TestCase(QualitySource.DVD, 576)]
        public void should_return_DVD(QualitySource source, int resolution)
        {
            QualityFinder.FindBySourceAndResolution(source, resolution).Should().Be(Quality.DVD);
        }

        [TestCase(QualitySource.Television, 480)]
        public void should_return_SDTV(QualitySource source, int resolution)
        {
            QualityFinder.FindBySourceAndResolution(source, resolution).Should().Be(Quality.SDTV);
        }

        [TestCase(QualitySource.Television, 720)]
        [TestCase(QualitySource.Unknown, 720)]
        public void should_return_HDTV_720p(QualitySource source, int resolution)
        {
            QualityFinder.FindBySourceAndResolution(source, resolution).Should().Be(Quality.HDTV720p);
        }

        [TestCase(QualitySource.Television, 1080)]
        [TestCase(QualitySource.Unknown, 1080)]
        public void should_return_HDTV_1080p(QualitySource source, int resolution)
        {
            QualityFinder.FindBySourceAndResolution(source, resolution).Should().Be(Quality.HDTV1080p);
        }

        [TestCase(QualitySource.Bluray, 720)]
        public void should_return_Bluray720p(QualitySource source, int resolution)
        {
            QualityFinder.FindBySourceAndResolution(source, resolution).Should().Be(Quality.Bluray720p);
        }
    }
}
