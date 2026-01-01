import { sortDirections } from 'Helpers/Props';
import { createThunk, handleThunks } from 'Store/thunks';
import serverSideCollectionHandlers from 'Utilities/serverSideCollectionHandlers';
import createHandleActions from './Creators/createHandleActions';
import createServerSideCollectionHandlers from './Creators/createServerSideCollectionHandlers';
import { filters as movieFilters } from './movieActions';

export const section = 'moviePages';

export const defaultState = {
  isFetching: false,
  isPopulated: false,
  page: 1,
  pageSize: 100,
  sortKey: 'sortTitle',
  sortDirection: sortDirections.ASCENDING,
  totalRecords: 0,
  totalPages: 1,
  error: null as unknown,
  items: [] as unknown[],
  itemMap: {} as Record<number, number>,
  selectedFilterKey: 'all',
  filters: movieFilters,
};

export const persistState = [
  'moviePages.pageSize',
  'moviePages.sortKey',
  'moviePages.sortDirection',
  'moviePages.selectedFilterKey',
];

export const FETCH_MOVIE_PAGES = 'moviePages/fetch';
export const FIRST_MOVIE_PAGES_PAGE = 'moviePages/firstPage';
export const PREVIOUS_MOVIE_PAGES_PAGE = 'moviePages/previousPage';
export const NEXT_MOVIE_PAGES_PAGE = 'moviePages/nextPage';
export const LAST_MOVIE_PAGES_PAGE = 'moviePages/lastPage';
export const GOTO_MOVIE_PAGES_PAGE = 'moviePages/gotoPage';
export const SET_MOVIE_PAGES_SORT = 'moviePages/setSort';
export const SET_MOVIE_PAGES_FILTER = 'moviePages/setFilter';

export const fetchMoviePages = createThunk(FETCH_MOVIE_PAGES);
export const firstMoviePagesPage = createThunk(FIRST_MOVIE_PAGES_PAGE);
export const previousMoviePagesPage = createThunk(PREVIOUS_MOVIE_PAGES_PAGE);
export const nextMoviePagesPage = createThunk(NEXT_MOVIE_PAGES_PAGE);
export const lastMoviePagesPage = createThunk(LAST_MOVIE_PAGES_PAGE);
export const gotoMoviePagesPage = createThunk(GOTO_MOVIE_PAGES_PAGE);
export const setMoviePagesSort = createThunk(SET_MOVIE_PAGES_SORT);
export const setMoviePagesFilter = createThunk(SET_MOVIE_PAGES_FILTER);

export const actionHandlers = handleThunks({
  ...createServerSideCollectionHandlers(
    section,
    '/movie/paged',
    fetchMoviePages,
    {
      [serverSideCollectionHandlers.FETCH]: FETCH_MOVIE_PAGES,
      [serverSideCollectionHandlers.FIRST_PAGE]: FIRST_MOVIE_PAGES_PAGE,
      [serverSideCollectionHandlers.PREVIOUS_PAGE]: PREVIOUS_MOVIE_PAGES_PAGE,
      [serverSideCollectionHandlers.NEXT_PAGE]: NEXT_MOVIE_PAGES_PAGE,
      [serverSideCollectionHandlers.LAST_PAGE]: LAST_MOVIE_PAGES_PAGE,
      [serverSideCollectionHandlers.EXACT_PAGE]: GOTO_MOVIE_PAGES_PAGE,
      [serverSideCollectionHandlers.SORT]: SET_MOVIE_PAGES_SORT,
      [serverSideCollectionHandlers.FILTER]: SET_MOVIE_PAGES_FILTER,
    }
  ),
});

export const reducers = createHandleActions({}, defaultState, section);
