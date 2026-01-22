using System;
using System.Collections.Generic;

namespace NzbDrone.Core.Update
{
    public interface IUpdatePackageProvider
    {
        UpdatePackage GetLatestUpdate(string branch, Version currentVersion);
        List<UpdatePackage> GetRecentUpdates(string branch, Version currentVersion, Version previousVersion = null);
    }
}
