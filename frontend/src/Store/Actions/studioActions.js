import { createAction } from 'redux-actions';
import { filterBuilderTypes, filterBuilderValueTypes, sortDirections } from 'Helpers/Props';
import { createThunk, handleThunks } from 'Store/thunks';
import translate from 'Utilities/String/translate';
import createFetchHandler from './Creators/createFetchHandler';
import createHandleActions from './Creators/createHandleActions';
import createSetClientSideCollectionFilterReducer from './Creators/Reducers/createSetClientSideCollectionFilterReducer';
import createSetClientSideCollectionSortReducer from './Creators/Reducers/createSetClientSideCollectionSortReducer';

//
// Variables

export const section = 'studios';

//
// State

export const defaultState = {
  isFetching: false,
  isPopulated: false,
  error: null,
  items: [],
  isSaving: false,
  saveError: null,
  sortKey: 'sortTitle',
  sortDirection: sortDirections.ASCENDING,
  secondarySortKey: 'sortTitle',
  secondarySortDirection: sortDirections.ASCENDING,
  view: 'posters',

  posterOptions: {
    detailedProgressBar: false,
    size: 'large',
    showTitle: false
  },

  selectedFilterKey: 'all',

  filters: [
    {
      key: 'all',
      label: () => translate('All'),
      filters: []
    }
  ],

  filterBuilderProps: [
    {
      name: 'title',
      label: () => translate('Title'),
      type: filterBuilderTypes.EXACT,
      valueType: filterBuilderValueTypes.DEFAULT
    }
  ]
};

export const persistState = [
  'studios.sortKey',
  'studios.sortDirection',
  'studios.selectedFilterKey',
  'studios.customFilters',
  'studios.posterOptions'
];

//
// Actions Types

export const FETCH_STUDIOS = 'studios/fetchStudios';

export const SET_STUDIO_SORT = 'studios/setStudioSort';
export const SET_STUDIO_FILTER = 'studios/setStudioFilter';
export const SET_STUDIO_POSTER_OPTION = 'studios/setStudioPosterOption';

//
// Action Creators

export const fetchStudios = createThunk(FETCH_STUDIOS);

export const setStudioSort = createAction(SET_STUDIO_SORT);
export const setStudioFilter = createAction(SET_STUDIO_FILTER);
export const setStudioPosterOption = createAction(SET_STUDIO_POSTER_OPTION);

//
// Action Handlers

export const actionHandlers = handleThunks({
  [FETCH_STUDIOS]: createFetchHandler(section, '/studio')
});

//
// Reducers

export const reducers = createHandleActions({

  [SET_STUDIO_SORT]: createSetClientSideCollectionSortReducer(section),
  [SET_STUDIO_FILTER]: createSetClientSideCollectionFilterReducer(section),

  [SET_STUDIO_POSTER_OPTION]: function(state, { payload }) {
    const posterOptions = state.posterOptions;

    return {
      ...state,
      posterOptions: {
        ...posterOptions,
        ...payload
      }
    };
  }

}, defaultState, section);
