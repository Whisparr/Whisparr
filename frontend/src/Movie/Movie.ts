import ModelBase from 'App/ModelBase';
import Language from 'Language/Language';
import { MovieFile } from 'MovieFile/MovieFile';

export type MovieMonitor = 'movieOnly' | 'sceneOnly' | 'movieAndScene' | 'none';

export type MovieStatus =
  | 'tba'
  | 'announced'
  | 'inCinemas'
  | 'released'
  | 'deleted';

export type CoverType = 'poster' | 'fanart' | 'screenshot';

export interface Image {
  coverType: CoverType;
  url: string;
  remoteUrl: string;
}

export interface Collection {
  tmdbId: number;
  title: string;
}

export interface Statistics {
  movieFileCount: number;
  releaseGroups: string[];
  sizeOnDisk: number;
}

export interface RatingValues {
  votes: number;
  value: number;
}

export interface Ratings {
  tmdb: RatingValues;
}

export interface MovieAddOptions {
  monitor: MovieMonitor;
  searchForMovie: boolean;
}

interface Movie extends ModelBase {
  tmdbId: number;
  tpdbId: string;
  stashId: string;
  itemType: string;
  foreignId: string;
  sortTitle: string;
  cleanTitle: string;
  overview: string;
  website: string;
  monitored: boolean;
  status: MovieStatus;
  title: string;
  credits: Array<object>;
  titleSlug: string;
  originalTitle: string;
  originalLanguage: Language;
  collection: Collection;
  studioTitle: string;
  studioForeignId: string;
  qualityProfileId: number;
  added: string;
  year: number;
  releaseDate: string;
  rootFolderPath: string;
  runtime: number;
  path: string;
  genres: string[];
  ratings: Ratings;
  statistics: Statistics;
  tags: number[];
  images: Image[];
  movieFileId: number;
  movieFile?: MovieFile;
  hasFile: boolean;
  grabbed?: boolean;
  lastSearchTime?: string;
  isAvailable: boolean;
  isSaving?: boolean;
  addOptions: MovieAddOptions;
}

export default Movie;
