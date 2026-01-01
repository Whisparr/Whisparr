import { sortDirections } from 'Helpers/Props';
import { createThunk, handleThunks } from 'Store/thunks';
import serverSideCollectionHandlers from 'Utilities/serverSideCollectionHandlers';
import createHandleActions from './Creators/createHandleActions';
import createServerSideCollectionHandlers from './Creators/createServerSideCollectionHandlers';
import * as performerActions from './performerActions';

const performerFilters = ((performerActions as Record<string, unknown>)
  .filters ?? []) as unknown[];

export const section = 'performerPages';

export const defaultState = {
  isFetching: false,
  isPopulated: false,
  page: 1,
  pageSize: 100,
  sortKey: 'fullName',
  sortDirection: sortDirections.ASCENDING,
  totalRecords: 0,
  totalPages: 1,
  error: null as unknown,
  items: [] as unknown[],
  itemMap: {} as Record<number, number>,
  selectedFilterKey: 'all',
  filters: performerFilters,
};

export const persistState = [
  'performerPages.pageSize',
  'performerPages.sortKey',
  'performerPages.sortDirection',
  'performerPages.selectedFilterKey',
];

export const FETCH_PERFORMER_PAGES = 'performerPages/fetch';
export const FIRST_PERFORMER_PAGES_PAGE = 'performerPages/firstPage';
export const PREVIOUS_PERFORMER_PAGES_PAGE = 'performerPages/previousPage';
export const NEXT_PERFORMER_PAGES_PAGE = 'performerPages/nextPage';
export const LAST_PERFORMER_PAGES_PAGE = 'performerPages/lastPage';
export const GOTO_PERFORMER_PAGES_PAGE = 'performerPages/gotoPage';
export const SET_PERFORMER_PAGES_SORT = 'performerPages/setSort';
export const SET_PERFORMER_PAGES_FILTER = 'performerPages/setFilter';

export const fetchPerformerPages = createThunk(FETCH_PERFORMER_PAGES);
export const firstPerformerPagesPage = createThunk(FIRST_PERFORMER_PAGES_PAGE);
export const previousPerformerPagesPage = createThunk(
  PREVIOUS_PERFORMER_PAGES_PAGE
);
export const nextPerformerPagesPage = createThunk(NEXT_PERFORMER_PAGES_PAGE);
export const lastPerformerPagesPage = createThunk(LAST_PERFORMER_PAGES_PAGE);
export const gotoPerformerPagesPage = createThunk(GOTO_PERFORMER_PAGES_PAGE);
export const setPerformerPagesSort = createThunk(SET_PERFORMER_PAGES_SORT);
export const setPerformerPagesFilter = createThunk(SET_PERFORMER_PAGES_FILTER);

export const actionHandlers = handleThunks({
  ...createServerSideCollectionHandlers(
    section,
    '/performer/paged',
    fetchPerformerPages,
    {
      [serverSideCollectionHandlers.FETCH]: FETCH_PERFORMER_PAGES,
      [serverSideCollectionHandlers.FIRST_PAGE]: FIRST_PERFORMER_PAGES_PAGE,
      [serverSideCollectionHandlers.PREVIOUS_PAGE]:
        PREVIOUS_PERFORMER_PAGES_PAGE,
      [serverSideCollectionHandlers.NEXT_PAGE]: NEXT_PERFORMER_PAGES_PAGE,
      [serverSideCollectionHandlers.LAST_PAGE]: LAST_PERFORMER_PAGES_PAGE,
      [serverSideCollectionHandlers.EXACT_PAGE]: GOTO_PERFORMER_PAGES_PAGE,
      [serverSideCollectionHandlers.SORT]: SET_PERFORMER_PAGES_SORT,
      [serverSideCollectionHandlers.FILTER]: SET_PERFORMER_PAGES_FILTER,
    }
  ),
});

export const reducers = createHandleActions({}, defaultState, section);
