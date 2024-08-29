using static NzbDrone.Core.ImportLists.ThePornDb.TPDbSceneSettings;

namespace NzbDrone.Core.ImportLists.ThePornDb
{
    public interface ITPDbSettings : IImportListSettings
    {
        string ApiKey { get; set; }

        string OrderBy { get; set; }

        string Date {  get; set; }

        string DateContext { get; set; }

        TriPosition Collected { get; set; }

        TriPosition Favourites { get; set; }
    }
}
