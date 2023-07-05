interface ReleaseEpisode {
  id: number;
  episodeFileId: number;
  seasonNumber: number;
  releaseDate?: string;
  absoluteEpisodeNumber?: number;
  title: string;
}

export default ReleaseEpisode;
