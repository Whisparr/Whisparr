import _ from 'lodash';
import { createAction } from 'redux-actions';
import { filterBuilderTypes, filterBuilderValueTypes, sortDirections } from 'Helpers/Props';
import { createThunk, handleThunks } from 'Store/thunks';
import createAjaxRequest from 'Utilities/createAjaxRequest';
import translate from 'Utilities/String/translate';
import { updateItem } from './baseActions';
import createFetchHandler from './Creators/createFetchHandler';
import createHandleActions from './Creators/createHandleActions';
import createSaveProviderHandler from './Creators/createSaveProviderHandler';
import createSetClientSideCollectionFilterReducer from './Creators/Reducers/createSetClientSideCollectionFilterReducer';
import createSetClientSideCollectionSortReducer from './Creators/Reducers/createSetClientSideCollectionSortReducer';
import createSetSettingValueReducer from './Creators/Reducers/createSetSettingValueReducer';

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
  pendingChanges: {},

  posterOptions: {
    detailedProgressBar: false,
    size: 'large',
    showTitle: false
  },

  defaults: {
    rootFolderPath: '',
    monitor: 'movieOnly',
    qualityProfileId: 0,
    searchForMovie: true,
    tags: []
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
  'studios.defaults',
  'studios.sortKey',
  'studios.sortDirection',
  'studios.selectedFilterKey',
  'studios.customFilters',
  'studios.posterOptions'
];

//
// Actions Types

export const FETCH_STUDIOS = 'studios/fetchStudios';
export const SAVE_STUDIO = 'studios/saveStudio';
export const SAVE_STUDIOS = 'studios/saveStudios';
export const SET_STUDIO_VALUE = 'studios/setStudioValue';

export const TOGGLE_STUDIO_MONITORED = 'studios/toggleStudioMonitored';

export const SET_STUDIO_SORT = 'studios/setStudioSort';
export const SET_STUDIO_FILTER = 'studios/setStudioFilter';
export const SET_STUDIO_POSTER_OPTION = 'studios/setStudioPosterOption';

//
// Action Creators

export const fetchStudios = createThunk(FETCH_STUDIOS);
export const saveStudio = createThunk(SAVE_STUDIO);
export const saveStudios = createThunk(SAVE_STUDIOS);

export const toggleStudioMonitored = createThunk(TOGGLE_STUDIO_MONITORED);

export const setStudioSort = createAction(SET_STUDIO_SORT);
export const setStudioFilter = createAction(SET_STUDIO_FILTER);
export const setStudioPosterOption = createAction(SET_STUDIO_POSTER_OPTION);

export const setStudioValue = createAction(SET_STUDIO_VALUE, (payload) => {
  return {
    section,
    ...payload
  };
});

//
// Action Handlers

export const actionHandlers = handleThunks({
  [FETCH_STUDIOS]: createFetchHandler(section, '/studio'),
  [SAVE_STUDIO]: createSaveProviderHandler(section, '/studio'),
  [TOGGLE_STUDIO_MONITORED]: (getState, payload, dispatch) => {
    const {
      studioId: id,
      monitored
    } = payload;

    const performer = _.find(getState().studios.items, { id });

    dispatch(updateItem({
      id,
      section,
      isSaving: true
    }));

    const promise = createAjaxRequest({
      url: `/studio/${id}`,
      method: 'PUT',
      data: JSON.stringify({
        ...performer,
        monitored
      }),
      dataType: 'json'
    }).request;

    promise.done((data) => {
      dispatch(updateItem({
        id,
        section,
        isSaving: false,
        monitored
      }));
    });

    promise.fail((xhr) => {
      dispatch(updateItem({
        id,
        section,
        isSaving: false
      }));
    });
  }
});

//
// Reducers

export const reducers = createHandleActions({

  [SET_STUDIO_SORT]: createSetClientSideCollectionSortReducer(section),
  [SET_STUDIO_FILTER]: createSetClientSideCollectionFilterReducer(section),
  [SET_STUDIO_VALUE]: createSetSettingValueReducer(section),

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
