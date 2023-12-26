import ModelBase from 'App/ModelBase';
import { Image } from 'Movie/Movie';

interface Studio extends ModelBase {
  foreignId: string;
  title: string;
  images: Image[];
  sortTitle: string;
}

export default Studio;
