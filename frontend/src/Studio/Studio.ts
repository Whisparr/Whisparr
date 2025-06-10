import ModelBase from 'App/ModelBase';
import { Image } from 'Movie/Movie';

interface Studio extends ModelBase {
  foreignId: string;
  title: string;
  network: string;
  monitored: boolean;
  images: Image[];
  sortTitle: string;
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
