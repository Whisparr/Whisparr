import { Image } from 'Movie/Movie';

interface Performer {
  id: string;
  foreignId: string;
  name: string;
  images: Image[];
  gender: string;
  movieId: number;
}

export default Performer;
