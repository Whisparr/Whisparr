import { sortDirections } from 'Helpers/Props';
import { createThunk, handleThunks } from 'Store/thunks';
import serverSideCollectionHandlers from 'Utilities/serverSideCollectionHandlers';
import createHandleActions from './Creators/createHandleActions';
import createServerSideCollectionHandlers from './Creators/createServerSideCollectionHandlers';
import { filters as movieFilters } from './movieActions';

export const section = 'scenePages';

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
  'scenePages.pageSize',
  'scenePages.sortKey',
  'scenePages.sortDirection',
  'scenePages.selectedFilterKey',
];

export const FETCH_SCENE_PAGES = 'scenePages/fetch';
export const FIRST_SCENE_PAGES_PAGE = 'scenePages/firstPage';
export const PREVIOUS_SCENE_PAGES_PAGE = 'scenePages/previousPage';
export const NEXT_SCENE_PAGES_PAGE = 'scenePages/nextPage';
export const LAST_SCENE_PAGES_PAGE = 'scenePages/lastPage';
export const GOTO_SCENE_PAGES_PAGE = 'scenePages/gotoPage';
export const SET_SCENE_PAGES_SORT = 'scenePages/setSort';
export const SET_SCENE_PAGES_FILTER = 'scenePages/setFilter';

export const fetchScenePages = createThunk(FETCH_SCENE_PAGES);
export const firstScenePagesPage = createThunk(FIRST_SCENE_PAGES_PAGE);
export const previousScenePagesPage = createThunk(PREVIOUS_SCENE_PAGES_PAGE);
export const nextScenePagesPage = createThunk(NEXT_SCENE_PAGES_PAGE);
export const lastScenePagesPage = createThunk(LAST_SCENE_PAGES_PAGE);
export const gotoScenePagesPage = createThunk(GOTO_SCENE_PAGES_PAGE);
export const setScenePagesSort = createThunk(SET_SCENE_PAGES_SORT);
export const setScenePagesFilter = createThunk(SET_SCENE_PAGES_FILTER);

export const actionHandlers = handleThunks({
  ...createServerSideCollectionHandlers(
    section,
    '/movie/scenes/paged',
    fetchScenePages,
    {
      [serverSideCollectionHandlers.FETCH]: FETCH_SCENE_PAGES,
      [serverSideCollectionHandlers.FIRST_PAGE]: FIRST_SCENE_PAGES_PAGE,
      [serverSideCollectionHandlers.PREVIOUS_PAGE]: PREVIOUS_SCENE_PAGES_PAGE,
      [serverSideCollectionHandlers.NEXT_PAGE]: NEXT_SCENE_PAGES_PAGE,
      [serverSideCollectionHandlers.LAST_PAGE]: LAST_SCENE_PAGES_PAGE,
      [serverSideCollectionHandlers.EXACT_PAGE]: GOTO_SCENE_PAGES_PAGE,
      [serverSideCollectionHandlers.SORT]: SET_SCENE_PAGES_SORT,
      [serverSideCollectionHandlers.FILTER]: SET_SCENE_PAGES_FILTER,
    }
  ),
});

export const reducers = createHandleActions({}, defaultState, section);
