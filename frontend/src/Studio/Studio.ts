import ModelBase from 'App/ModelBase';
import { Image } from 'Movie/Movie';

interface Studio extends ModelBase {
  foreignId: string;
  tmdbId: number;
  tpdbId: string;
  title: string;
  network: string;
  monitored: boolean;
  moviesMonitored: boolean;
  images: Image[];
  sortTitle: string;
  movieCount: number;
  totalMovieCount: number;
  sceneCount: number;
  totalSceneCount: number;
  hasScenes: boolean;
  hasMovies: boolean;
  tags: number[];
  aliases: string[];
  rootFolderPath: string;
  website: string;
}

export default Studio;
