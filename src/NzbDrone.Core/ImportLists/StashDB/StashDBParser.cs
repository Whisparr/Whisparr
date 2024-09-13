using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.ImportLists.Exceptions;
using NzbDrone.Core.ImportLists.ImportListMovies;

namespace NzbDrone.Core.ImportLists.StashDB
{
    public class StashDBParser : IParseImportListResponse
    {
        public virtual IList<ImportListMovie> ParseResponse(ImportListResponse importResponse)
        {
            var scenes = new List<ImportListMovie>();

            if (!PreProcess(importResponse))
            {
                return scenes;
            }

            var jsonResponse = JsonConvert.DeserializeObject<QueryScenesResult>(importResponse.Content);

            // empty response
            if (jsonResponse == null)
            {
                return scenes;
            }

            foreach (var scene in jsonResponse.Data.QueryScenes.Scenes)
            {
                scenes.Add(MapListMovie(scene));
            }

            return scenes;
        }

        protected ImportListMovie MapListMovie(Scene stashDBScene)
        {
            var scene = new ImportListMovie()
            {
                StashId = stashDBScene.Id.ToString(),
                Title = stashDBScene.Title,
            };

            if (stashDBScene.ReleaseDate.IsNotNullOrWhiteSpace() && DateTime.TryParse(stashDBScene.ReleaseDate, out var releaseDate))
            {
                scene.Year = releaseDate.Year;
            }

            return scene;
        }

        protected virtual bool PreProcess(ImportListResponse listResponse)
        {
            if (listResponse.HttpResponse.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new ImportListException(listResponse,
                    "StashDB API call resulted in an unexpected StatusCode [{0}]",
                    listResponse.HttpResponse.StatusCode);
            }

            return true;
        }
    }
}
