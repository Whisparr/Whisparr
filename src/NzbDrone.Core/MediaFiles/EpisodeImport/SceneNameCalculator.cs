using System.IO;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.EpisodeImport
{
    public static class SceneNameCalculator
    {
        public static string GetSceneName(LocalEpisode localEpisode)
        {
            var otherVideoFiles = localEpisode.OtherVideoFiles;
            var downloadClientInfo = localEpisode.DownloadClientEpisodeInfo;

            // The release title can be missing when the download was matched by external ID or
            // recreated from history, in which case fall back to the file/folder name below
            if (!otherVideoFiles && downloadClientInfo != null && downloadClientInfo.ReleaseTitle.IsNotNullOrWhiteSpace())
            {
                return Parser.Parser.RemoveFileExtension(downloadClientInfo.ReleaseTitle);
            }

            var fileName = Path.GetFileNameWithoutExtension(localEpisode.Path.CleanFilePath());

            if (SceneChecker.IsSceneTitle(fileName))
            {
                return fileName;
            }

            var folderTitle = localEpisode.FolderEpisodeInfo?.ReleaseTitle;

            if (!otherVideoFiles &&
                folderTitle.IsNotNullOrWhiteSpace() &&
                SceneChecker.IsSceneTitle(folderTitle))
            {
                return folderTitle;
            }

            return null;
        }
    }
}
