using System;
using System.Linq;

namespace NzbDrone.Core.Update
{
    public class UpdatePackage
    {
        public Semver.SemVersion Version { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string FileName { get; set; }
        public string Url { get; set; }
        public UpdateChanges Changes { get; set; }
        public string Hash { get; set; }
        public string Branch { get; set; }

        /// <summary>
        /// Converts SemVersion to System.Version using the formula:
        /// major.minor.patch-branch.buildnumber = major.minor.patch.buildnumber
        /// </summary>
        public Version DotNetVersion
        {
            get
            {
                if (Version == null)
                {
                    return null;
                }

                // Try to extract build number from prerelease (e.g. develop.88 or release.98)
                var build = 0;
                if (!string.IsNullOrEmpty(Version.Prerelease))
                {
                    var parts = Version.Prerelease.Split('.');
                    if (parts.Length > 1 && int.TryParse(parts.Last(), out var parsedBuild))
                    {
                        build = parsedBuild;
                    }
                }

                return new Version((int)Version.Major, (int)Version.Minor, (int)Version.Patch, build);
            }
        }
    }
}
