import ModelBase from 'App/ModelBase';
import { Image } from 'Movie/Movie';

interface Performer extends ModelBase {
  foreignId: string;
  fullName: string;
  monitored: boolean;
  rootFolderPath: string;
  images: Image[];
  gender: string;
  ethnicity?: string;
  hairColor?: string;
  status: string;
  sortTitle: string;
  added: string;
  qualityProfileId: number;
  tags: number[];
}

export default Performer;
