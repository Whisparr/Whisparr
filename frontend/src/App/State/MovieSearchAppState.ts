import Movie from 'Movie/Movie';

export default interface MovieSearchAppState {
  isFetching: boolean;
  isPopulated: boolean;
  items: Movie[];
}
