using System;
using System.Collections.Generic;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Performers;
using NzbDrone.Core.Movies.Studios;

namespace NzbDrone.Core.MetadataSource
{
    public interface IProvideMovieInfo
    {
        MovieMetadata GetMovieByImdbId(string imdbId);
        Tuple<MovieMetadata, Studio, List<Performer>> GetMovieInfo(int tmdbId);
        Tuple<MovieMetadata, Studio, List<Performer>> GetTpdbMovieInfo(string tpdbId);
        Tuple<MovieMetadata, Studio, List<Performer>> GetSceneInfo(string stashId);
        List<MovieMetadata> GetBulkMovieInfo(List<int> tmdbIds);
        List<MovieMetadata> GetBulkTpdbMovieInfo(List<string> tpdbIds);
        Performer GetPerformerInfo(string stashId);
        Studio GetStudioInfo(string stashId);
        (List<string> Scenes, List<string> TpdbMovies, List<int> Movies) GetPerformerWorks(string stashId);
        List<string> GetStudioScenes(string stashId);
        (List<string> Scenes, List<string> TpdbMovies) GetStudioWorks(string stashId);
        HashSet<int> GetChangedMovies(DateTime startTime);
        HashSet<int> GetChangedTpdbMovies(DateTime startTime);
        HashSet<string> GetChangedScenes(DateTime startTime);
        HashSet<string> GetChangedStudios(DateTime startTime);
        HashSet<string> GetChangedPerformers(DateTime startTime);
    }
}
