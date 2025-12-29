import ModelBase from 'App/ModelBase';
import { Image } from 'Movie/Movie';
import Performer from 'Performer/Performer';

export type MovieCreditType = 'cast' | 'crew';

interface MovieCredit extends ModelBase {
  foreignId: string;
  performer: Performer;
  personName: string;
  images: Image[];
  type: MovieCreditType;
  department: string;
  job: string;
  character: string;
  order: number;
}

export default MovieCredit;
