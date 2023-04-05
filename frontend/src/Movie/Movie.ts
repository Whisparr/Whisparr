import ModelBase from 'App/ModelBase';
import { Image, Language } from 'Series/Series';

export interface Statistics {
  releaseGroups: string[];
  sizeOnDisk: number;
}

interface Movie extends ModelBase {
  added: string;
  cleanTitle: string;
  genres: string[];
  images: Image[];
  monitored: boolean;
  studio: string;
  originalLanguage: Language;
  overview: string;
  path: string;
  qualityProfileId: number;
  rootFolderPath: string;
  runtime: number;
  sortTitle: string;
  statistics: Statistics;
  status: string;
  tags: number[];
  title: string;
  titleSlug: string;
  tmdbId: number;
  year: number;
  isSaving?: boolean;
}

export default Movie;
