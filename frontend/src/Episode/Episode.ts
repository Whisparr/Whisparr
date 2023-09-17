import ModelBase from 'App/ModelBase';
import Series from 'Series/Series';

export interface Actor {
  tpdbId: number;
  name: string;
  character: string;
  gender: string;
}

interface Episode extends ModelBase {
  seriesId: number;
  tvdbId: number;
  episodeFileId: number;
  seasonNumber: number;
  releaseDate?: string;
  actors: Actor[];
  runtime: number;
  absoluteEpisodeNumber?: number;
  sceneSeasonNumber?: number;
  sceneEpisodeNumber?: number;
  sceneAbsoluteEpisodeNumber?: number;
  overview: string;
  title: string;
  episodeFile?: object;
  hasFile: boolean;
  monitored: boolean;
  unverifiedSceneNumbering: boolean;
  endTime?: string;
  grabDate?: string;
  seriesTitle?: string;
  series?: Series;
}

export default Episode;
