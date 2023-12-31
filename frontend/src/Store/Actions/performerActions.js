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

export const section = 'performers';

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
    showName: false
  },

  defaults: {
    rootFolderPath: '',
    monitor: 'movieOnly',
    qualityProfileId: 0,
    searchForMovie: true,
    tags: []
  },

  sortPredicates: {
    gender: function(item) {
      const gender = item.gender;

      return gender ? gender.toLowerCase() : '';
    }
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
      name: 'gender',
      label: () => translate('Gender'),
      type: filterBuilderTypes.EXACT,
      valueType: filterBuilderValueTypes.DEFAULT
    }
  ]
};

export const persistState = [
  'performers.defaults',
  'performers.sortKey',
  'performers.sortDirection',
  'performers.selectedFilterKey',
  'performers.customFilters',
  'performers.posterOptions'
];

//
// Actions Types

export const FETCH_PERFORMERS = 'performers/fetchPerformers';
export const SAVE_PERFORMER = 'performers/savePerformer';
export const SAVE_PERFORMERS = 'performers/savePerformers';
export const SET_PERFORMER_VALUE = 'performers/setPerformerValue';

export const TOGGLE_PERFORMER_MONITORED = 'performers/togglePerformerMonitored';

export const SET_PERFORMER_SORT = 'performers/setPerformerSort';
export const SET_PERFORMER_FILTER = 'performers/setPerformerFilter';
export const SET_PERFORMER_POSTER_OPTION = 'performers/setPerformerPosterOption';

//
// Action Creators

export const fetchPerformers = createThunk(FETCH_PERFORMERS);
export const savePerformer = createThunk(SAVE_PERFORMER);
export const savePerformers = createThunk(SAVE_PERFORMERS);

export const togglePerformerMonitored = createThunk(TOGGLE_PERFORMER_MONITORED);

export const setPerformerSort = createAction(SET_PERFORMER_SORT);
export const setPerformerFilter = createAction(SET_PERFORMER_FILTER);
export const setPerformerPosterOption = createAction(SET_PERFORMER_POSTER_OPTION);

export const setPerformerValue = createAction(SET_PERFORMER_VALUE, (payload) => {
  return {
    section,
    ...payload
  };
});

//
// Action Handlers

export const actionHandlers = handleThunks({
  [FETCH_PERFORMERS]: createFetchHandler(section, '/performer'),
  [SAVE_PERFORMER]: createSaveProviderHandler(section, '/performer'),
  [TOGGLE_PERFORMER_MONITORED]: (getState, payload, dispatch) => {
    const {
      performerId: id,
      monitored
    } = payload;

    const performer = _.find(getState().performers.items, { id });

    dispatch(updateItem({
      id,
      section,
      isSaving: true
    }));

    const promise = createAjaxRequest({
      url: `/performer/${id}`,
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

  [SET_PERFORMER_SORT]: createSetClientSideCollectionSortReducer(section),
  [SET_PERFORMER_FILTER]: createSetClientSideCollectionFilterReducer(section),
  [SET_PERFORMER_VALUE]: createSetSettingValueReducer(section),

  [SET_PERFORMER_POSTER_OPTION]: function(state, { payload }) {
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
