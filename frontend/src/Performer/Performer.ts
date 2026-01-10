import ModelBase from 'App/ModelBase';
import { Image } from 'Movie/Movie';

interface Performer extends ModelBase {
  foreignId: string;
  tmdbId?: number;
  tpdbId?: string;
  name: string;
  fullName: string;
  monitored: boolean;
  moviesMonitored: boolean;
  rootFolderPath: string;
  images: Image[];
  gender: string;
  age?: number;
  careerStart?: number;
  careerEnd?: number;
  ethnicity?: string;
  hairColor?: string;
  movieCount: number;
  totalMovieCount: number;
  sceneCount: number;
  totalSceneCount: number;
  hasScenes: boolean;
  hasMovies: boolean;
  status: string;
  sortTitle: string;
  added: string;
  qualityProfileId: number;
  tags: number[];
}

export default Performer;
