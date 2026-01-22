using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Common.Cloud;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Http;
using NzbDrone.Core.Analytics;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Datastore;
using Semver;

namespace NzbDrone.Core.Update
{
    public class GithubUpdatePackageProvider : IUpdatePackageProvider
    {
        private readonly IPlatformInfo _platformInfo;
        private readonly IAnalyticsService _analyticsService;
        private readonly IConfigFileProvider _configFileProvider;
        private readonly IMainDatabase _mainDatabase;
        private readonly IHttpClient _httpClient;
        private readonly IWhisparrCloudRequestBuilder _cloudRequestBuilder;
        private readonly Logger _logger;

        public GithubUpdatePackageProvider(
            IHttpClient httpClient,
            IAnalyticsService analyticsService,
            IPlatformInfo platformInfo,
            IMainDatabase mainDatabase,
            IConfigFileProvider configFileProvider,
            IWhisparrCloudRequestBuilder cloudRequestBuilder)
        {
            _platformInfo = platformInfo;
            _analyticsService = analyticsService;
            _configFileProvider = configFileProvider;
            _httpClient = httpClient;
            _mainDatabase = mainDatabase;
            _cloudRequestBuilder = cloudRequestBuilder;
            _logger = NzbDrone.Common.Instrumentation.NzbDroneLogger.GetLogger(this);
        }

        /// <summary>
        /// Gets the latest update package for the specified branch and current version.
        /// </summary>
        /// <param name="branch">The branch to check for updates (e.g., "v2", "v2-develop").</param>
        /// <param name="currentVersion">The current version of the application.</param>
        /// <returns>The latest UpdatePackage if an update is available; otherwise, null.</returns>
        public UpdatePackage GetLatestUpdate(string branch, Version currentVersion)
        {
            _logger.Info("Checking for latest update (branch: {0}, currentVersion: {1})", branch, currentVersion);
            var updates = GetRecentUpdates(branch, currentVersion);
            var latest = updates.FirstOrDefault();

            if (latest != null)
            {
                _logger.Info("Update found: {0} ({1})", latest.Version, latest.FileName);

                // Convert latest.Version (SemVersion) to .NET Version for comparison
                var latestDotNetVersion = latest.DotNetVersion;

                if (currentVersion >= latestDotNetVersion)
                {
                    _logger.Info("Current version '{0}' is up-to-date or newer than the latest available update '{1}'.",
                        currentVersion,
                        latestDotNetVersion);
                    return null;
                }

                return latest;
            }
            else
            {
                _logger.Warn("No update found from GitHub releases.");
                return null;
            }
        }

        /// <summary>
        /// Gets a list of recent update packages for the specified branch and current version.
        /// </summary>
        /// <param name="branch">The branch to check for updates (e.g., "v2", "v2-develop").</param>
        /// <param name="currentVersion">The current version of the application.</param>
        /// <param name="previousVersion">The previous version of the application (optional).</param>
        /// <returns>A list of recent UpdatePackage objects.</returns>
        public List<UpdatePackage> GetRecentUpdates(string branch, Version currentVersion, Version previousVersion = null)
        {
            var ownerRepo = _configFileProvider.GithubOwnerRepo;
            _logger.Info("Fetching recent updates from GitHub releases (branch: {0}, currentVersion: {1}, previousVersion: {2})",
                branch,
                currentVersion,
                previousVersion);

            var builder = _cloudRequestBuilder.GithubReleases.Create();
            builder.SetSegment("githubownerrepo", ownerRepo);
            builder.AddQueryParam("per_page", "25");

            var request = builder.Build();
            request.Headers.Add("User-Agent", $"Whisparr/{currentVersion}");
            _logger.Debug($"Requesting: {request.Url}");

            var response = _httpClient.Get(request);
            _logger.Debug($"GitHub API response: {response.StatusCode}, {response.Content?.Length ?? 0} bytes");

            var releases = JsonSerializer.Deserialize<List<GithubRelease>>(response.Content) ?? new List<GithubRelease>();

            var osAssetString = GetOsAssetString(OsInfo.Os);
            var arch = RuntimeInformation.OSArchitecture.ToString().ToLowerInvariant();

            var packages = new List<UpdatePackage>();

            foreach (var release in releases)
            {
                // Filter out releases that do not match the requested branch
                // For v2, skip tags with 'develop'; for v2-develop, skip tags without 'develop'
                if (!string.IsNullOrEmpty(branch))
                {
                    var tagLower = release.tag_name.ToLowerInvariant();
                    var branchLower = branch.ToLowerInvariant();

                    // If branch is exactly 'v2', skip develop tags
                    if (branchLower == "v2" && tagLower.Contains("develop"))
                    {
                        _logger.Debug($"Skipping prerelease {release.tag_name} for stable branch {branch}.");
                        continue;
                    }

                    // If branch contains 'develop', skip tags without 'develop'
                    if (branchLower.Contains("develop") && !tagLower.Contains("develop"))
                    {
                        _logger.Debug($"Skipping release {release.tag_name} because it is a release tag and branch is {branch}.");
                        continue;
                    }
                }

                if (release.assets == null)
                {
                    _logger.Debug($"Release {release.tag_name} has no package assets, skipping.");
                    continue;
                }

                // Find the appropriate asset for this OS/architecture
                GithubAsset asset = null;

                if (OsInfo.Os == Os.Osx)
                {
                    // Prefer .tar.gz for macOS, fallback to .zip/.app
                    asset = release.assets.FirstOrDefault(a =>
                        a.name.Contains(osAssetString, StringComparison.OrdinalIgnoreCase) &&
                        a.name.Contains(arch, StringComparison.OrdinalIgnoreCase) &&
                        a.name.EndsWith(".tar.gz", StringComparison.OrdinalIgnoreCase));

                    if (asset == null)
                    {
                        asset = release.assets.FirstOrDefault(a =>
                            a.name.Contains(osAssetString, StringComparison.OrdinalIgnoreCase) &&
                            a.name.Contains(arch, StringComparison.OrdinalIgnoreCase) &&
                            (a.name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) || a.name.EndsWith(".app", StringComparison.OrdinalIgnoreCase)));
                    }
                }
                else if (OsInfo.Os == Os.Windows)
                {
                    asset = release.assets.FirstOrDefault(a =>
                        a.name.Contains(osAssetString, StringComparison.OrdinalIgnoreCase) &&
                        a.name.Contains(arch, StringComparison.OrdinalIgnoreCase) &&
                        a.name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase));
                }
                else
                {
                    asset = release.assets.FirstOrDefault(a =>
                        a.name.Contains(osAssetString, StringComparison.OrdinalIgnoreCase) &&
                        a.name.Contains(arch, StringComparison.OrdinalIgnoreCase));
                }

                if (asset == null)
                {
                    _logger.Debug("No asset found for release {0} matching OS asset string '{1}' and arch '{2}'",
                        release.tag_name,
                        osAssetString,
                        arch);
                    continue;
                }

                _logger.Debug($"Found update: {release.tag_name} - {asset.name}");
                var tag = release.tag_name.TrimStart('v');

                // Attempt to strip "what's new", as it's repetitive in our UI
                var body = release?.body != null
                    ? Regex.Replace(release.body, @"^## What's Changed\s*\r?\n", "", RegexOptions.Multiline)
                    : string.Empty;

                if (!SemVersion.TryParse(tag, SemVersionStyles.Any, out var version))
                {
                    _logger.Warn("Could not parse semver from tag '{0}'. Skipping this release.", release.tag_name);
                    continue;
                }

                // Extract hash from digest or body if available
                var hash = string.Empty;
                if (!string.IsNullOrEmpty(asset.digest))
                {
                    hash = asset.digest.Replace("sha256:", "", StringComparison.OrdinalIgnoreCase);
                }

                packages.Add(new UpdatePackage
                {
                    Version = version,
                    ReleaseDate = release.published_at,
                    FileName = asset.name,
                    Url = asset.browser_download_url,
                    Changes = new UpdateChanges { New = new List<string> { body } },
                    Hash = hash,
                    Branch = branch
                });
            }

            _logger.Debug($"Total updates found: {packages.Count}");
            return packages;
        }

        /// <summary>
        /// Maps the OsInfo.Os enum to the asset string prefix used in GitHub release asset names.
        /// </summary>
        /// <param name="os">The OsInfo.Os enum value.</param>
        /// <returns>The asset string prefix (e.g., "win", "linux-musl").</returns>
        private static string GetOsAssetString(Os os)
        {
            switch (os)
            {
                case Os.Windows:
                    return "win";
                case Os.LinuxMusl:
                    return "linux-musl";
                case Os.Linux:
                    return "linux";
                case Os.Osx:
                    return "osx";
                case Os.Bsd:
                    return "freebsd";
                default:
                    throw new ArgumentOutOfRangeException(nameof(os), os, null);
            }
        }

        internal class GithubRelease
        {
            /// <summary>The tag name of the release.</summary>
            public string tag_name { get; set; }

            /// <summary>The body/description of the release.</summary>
            public string body { get; set; }

            /// <summary>The publication date/time of the release.</summary>
            public DateTime published_at { get; set; }

            /// <summary>The list of assets (packages) associated with the release.</summary>
            public List<GithubAsset> assets { get; set; }
        }

        /// <summary>Represents an asset in a GitHub release.</summary>
        internal class GithubAsset
        {
            /// <summary>The name of the asset file.</summary>
            public string name { get; set; }

            /// <summary>The digest (sha256 hash) of the asset.</summary>
            public string digest { get; set; }

            /// <summary>The download URL of the asset.</summary>
            public string browser_download_url { get; set; }
        }
    }
}
