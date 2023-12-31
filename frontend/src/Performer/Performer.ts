import ModelBase from 'App/ModelBase';
import { Image } from 'Movie/Movie';

interface Performer extends ModelBase {
  foreignId: string;
  name: string;
  monitored: boolean;
  images: Image[];
  gender: string;
  sortTitle: string;
}

export default Performer;
