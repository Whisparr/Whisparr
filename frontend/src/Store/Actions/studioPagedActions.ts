import { sortDirections } from 'Helpers/Props';
import { createThunk, handleThunks } from 'Store/thunks';
import serverSideCollectionHandlers from 'Utilities/serverSideCollectionHandlers';
import createHandleActions from './Creators/createHandleActions';
import createServerSideCollectionHandlers from './Creators/createServerSideCollectionHandlers';
import * as studioActions from './studioActions';

const studioFilters = ((studioActions as Record<string, unknown>).filters ??
  []) as unknown[];

export const section = 'studioPages';

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
  filters: studioFilters,
};

export const persistState = [
  'studioPages.pageSize',
  'studioPages.sortKey',
  'studioPages.sortDirection',
  'studioPages.selectedFilterKey',
];

export const FETCH_STUDIO_PAGES = 'studioPages/fetch';
export const FIRST_STUDIO_PAGES_PAGE = 'studioPages/firstPage';
export const PREVIOUS_STUDIO_PAGES_PAGE = 'studioPages/previousPage';
export const NEXT_STUDIO_PAGES_PAGE = 'studioPages/nextPage';
export const LAST_STUDIO_PAGES_PAGE = 'studioPages/lastPage';
export const GOTO_STUDIO_PAGES_PAGE = 'studioPages/gotoPage';
export const SET_STUDIO_PAGES_SORT = 'studioPages/setSort';
export const SET_STUDIO_PAGES_FILTER = 'studioPages/setFilter';

export const fetchStudioPages = createThunk(FETCH_STUDIO_PAGES);
export const firstStudioPagesPage = createThunk(FIRST_STUDIO_PAGES_PAGE);
export const previousStudioPagesPage = createThunk(PREVIOUS_STUDIO_PAGES_PAGE);
export const nextStudioPagesPage = createThunk(NEXT_STUDIO_PAGES_PAGE);
export const lastStudioPagesPage = createThunk(LAST_STUDIO_PAGES_PAGE);
export const gotoStudioPagesPage = createThunk(GOTO_STUDIO_PAGES_PAGE);
export const setStudioPagesSort = createThunk(SET_STUDIO_PAGES_SORT);
export const setStudioPagesFilter = createThunk(SET_STUDIO_PAGES_FILTER);

export const actionHandlers = handleThunks({
  ...createServerSideCollectionHandlers(
    section,
    '/studio/paged',
    fetchStudioPages,
    {
      [serverSideCollectionHandlers.FETCH]: FETCH_STUDIO_PAGES,
      [serverSideCollectionHandlers.FIRST_PAGE]: FIRST_STUDIO_PAGES_PAGE,
      [serverSideCollectionHandlers.PREVIOUS_PAGE]: PREVIOUS_STUDIO_PAGES_PAGE,
      [serverSideCollectionHandlers.NEXT_PAGE]: NEXT_STUDIO_PAGES_PAGE,
      [serverSideCollectionHandlers.LAST_PAGE]: LAST_STUDIO_PAGES_PAGE,
      [serverSideCollectionHandlers.EXACT_PAGE]: GOTO_STUDIO_PAGES_PAGE,
      [serverSideCollectionHandlers.SORT]: SET_STUDIO_PAGES_SORT,
      [serverSideCollectionHandlers.FILTER]: SET_STUDIO_PAGES_FILTER,
    }
  ),
});

export const reducers = createHandleActions({}, defaultState, section);
