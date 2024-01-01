import ModelBase from 'App/ModelBase';
import { Image } from 'Movie/Movie';

interface Studio extends ModelBase {
  foreignId: string;
  title: string;
  monitored: boolean;
  images: Image[];
  sortTitle: string;
  tags: number[];
  rootFolderPath: string;
  website: string;
}

export default Studio;
