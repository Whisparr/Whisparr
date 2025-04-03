import ModelBase from 'App/ModelBase';
import Language from 'Language/Language';
import { QualityModel } from 'Quality/Quality';
// import CustomFormat from 'typings/CustomFormat';
import MediaInfo from 'typings/MediaInfo';

export interface UnmappedMovieFile extends ModelBase {
  movieId: number;
  path: string;
  size: number;
  dateAdded: string;
  sceneName: string;
  releaseGroup: string;
  languages: Language[];
  quality: QualityModel;
  mediaInfo: MediaInfo;
  qualityCutoffNotMet: boolean;
}
