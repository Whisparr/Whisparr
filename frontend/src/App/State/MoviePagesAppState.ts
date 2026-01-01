import Movie from 'Movie/Movie';
import AppSectionState, { PagedAppSectionState } from './AppSectionState';
import { Filter } from './AppState';

interface MoviePagesAppState
  extends AppSectionState<Movie>,
    PagedAppSectionState {
  totalRecords: number;
  itemMap: Record<number, number>;
  selectedFilterKey: string;
  filters: Filter[];
}

export default MoviePagesAppState;
