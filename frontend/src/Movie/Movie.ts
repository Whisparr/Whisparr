import ModelBase from 'App/ModelBase';
import Language from 'Language/Language';
import { MovieFile } from 'MovieFile/MovieFile';

export interface Image {
  coverType: string;
  url: string;
  remoteUrl: string;
}

export interface Collection {
  title: string;
}

export interface Statistics {
  movieFileCount: number;
  releaseGroups: string[];
  sizeOnDisk: number;
}

export interface Ratings {
  imdb: object;
  tmdb: object;
  metacritic: object;
  rottenTomatoes: object;
}

interface Movie extends ModelBase {
  tmdbId: number;
  imdbId: string;
  stashId: string;
  itemType: string;
  foreignId: string;
  sortTitle: string;
  cleanTitle: string;
  overview: string;
  monitored: boolean;
  status: string;
  title: string;
  credits: Array<object>;
  titleSlug: string;
  collection: Collection;
  studioTitle: string;
  studioForeignId: string;
  qualityProfileId: number;
  added: string;
  year: number;
  releaseDate: string;
  originalLanguage: Language;
  runtime: number;
  path: string;
  genres: string[];
  ratings: Ratings;
  statistics: Statistics;
  tags: number[];
  images: Image[];
  movieFile: MovieFile;
  hasFile: boolean;
  isAvailable: boolean;
  isSaving?: boolean;
}

export default Movie;
