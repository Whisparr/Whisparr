import _ from 'lodash';
import { createAction } from 'redux-actions';
import { batchActions } from 'redux-batched-actions';
import { filterBuilderTypes, filterBuilderValueTypes, sortDirections } from 'Helpers/Props';
import { createThunk, handleThunks } from 'Store/thunks';
import sortByName from 'Utilities/Array/sortByName';
import createAjaxRequest from 'Utilities/createAjaxRequest';
import camelCaseToString from 'Utilities/String/camelCaseToString';
import translate from 'Utilities/String/translate';
import { set, updateItem } from './baseActions';
import createFetchHandler from './Creators/createFetchHandler';
import createHandleActions from './Creators/createHandleActions';
import createSaveProviderHandler from './Creators/createSaveProviderHandler';
import createSetClientSideCollectionFilterReducer from './Creators/Reducers/createSetClientSideCollectionFilterReducer';
import createSetClientSideCollectionSortReducer from './Creators/Reducers/createSetClientSideCollectionSortReducer';
import createSetSettingValueReducer from './Creators/Reducers/createSetSettingValueReducer';
import createSetTableOptionReducer from './Creators/Reducers/createSetTableOptionReducer';

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
  sortKey: 'fullName',
  sortDirection: sortDirections.ASCENDING,
  secondarySortKey: 'fullName',
  secondarySortDirection: sortDirections.ASCENDING,
  view: 'posters',
  pendingChanges: {},

  sceneSortKey: 'releaseDate',
  sceneSortDirection: sortDirections.DESCENDING,

  posterOptions: {
    detailedProgressBar: false,
    size: 'large',
    showName: true
  },

  tableOptions: {
  },

  defaults: {
    rootFolderPath: '',
    monitor: 'movieOnly',
    qualityProfileId: 0,
    searchForMovie: true,
    tags: []
  },

  columns: [
    {
      name: 'status',
      columnLabel: () => translate('Monitored'),
      isSortable: true,
      isVisible: true,
      isModifiable: false
    },
    {
      name: 'fullName',
      label: () => translate('PerformerName'),
      isSortable: true,
      isVisible: true,
      isModifiable: false
    },
    {
      name: 'gender',
      label: () => translate('Gender'),
      isSortable: true,
      isVisible: true
    },
    {
      name: 'hairColor',
      label: () => translate('HairColor'),
      isSortable: true,
      isVisible: true
    },
    {
      name: 'ethnicity',
      label: () => translate('Ethnicity'),
      isSortable: true,
      isVisible: true
    },
    {
      name: 'qualityProfileId',
      label: () => translate('QualityProfile'),
      isSortable: true,
      isVisible: true
    },
    {
      name: 'rootFolderPath',
      label: () => translate('RootFolder'),
      isSortable: true,
      isVisible: true
    },
    {
      name: 'tags',
      label: () => translate('Tags'),
      isSortable: false,
      isVisible: false
    },
    {
      name: 'actions',
      columnLabel: () => translate('Actions'),
      isVisible: true,
      isModifiable: false
    }
  ],

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
      name: 'monitored',
      label: () => translate('Monitored'),
      type: filterBuilderTypes.EXACT,
      valueType: filterBuilderValueTypes.BOOL
    },
    {
      name: 'qualityProfileId',
      label: () => translate('QualityProfile'),
      type: filterBuilderTypes.EXACT,
      valueType: filterBuilderValueTypes.QUALITY_PROFILE
    },
    {
      name: 'gender',
      label: () => translate('Gender'),
      type: filterBuilderTypes.EXACT,
      optionsSelector: function(items) {
        const tagList = ['male', 'female', 'transMale', 'transFemale', 'nonBinary', 'intersex'];

        const tags = tagList.map((tag) => {
          return {
            id: tag,
            name: camelCaseToString(tag)
          };
        });

        return tags.sort(sortByName);
      }
    },
    {
      name: 'hairColor',
      label: () => translate('HairColor'),
      type: filterBuilderTypes.EXACT,
      optionsSelector: function(items) {
        const tagList = ['blonde', 'black', 'red', 'auburn', 'grey', 'various', 'bald', 'other'];

        const tags = tagList.map((tag) => {
          return {
            id: tag,
            name: camelCaseToString(tag)
          };
        });

        return tags.sort(sortByName);
      }
    },
    {
      name: 'ethnicity',
      label: () => translate('Ethnicity'),
      type: filterBuilderTypes.EXACT,
      optionsSelector: function(items) {
        const tagList = ['caucasian', 'black', 'asian', 'latin', 'indian', 'middleEastern', 'other'];

        const tags = tagList.map((tag) => {
          return {
            id: tag,
            name: camelCaseToString(tag)
          };
        });

        return tags.sort(sortByName);
      }
    },
    {
      name: 'tags',
      label: () => translate('Tags'),
      type: filterBuilderTypes.ARRAY,
      valueType: filterBuilderValueTypes.TAG
    }
  ]
};

export const persistState = [
  'performers.defaults',
  'performers.sortKey',
  'performers.sortDirection',
  'performers.view',
  'performers.columns',
  'performers.selectedFilterKey',
  'performers.customFilters',
  'performers.posterOptions',
  'performers.tableOptions'
];

//
// Actions Types

export const FETCH_PERFORMERS = 'performers/fetchPerformers';
export const SAVE_PERFORMER = 'performers/savePerformer';
export const SAVE_PERFORMER_EDITOR = 'performers/savePerformerEditor';
export const SET_PERFORMER_VALUE = 'performers/setPerformerValue';

export const TOGGLE_PERFORMER_MONITORED = 'performers/togglePerformerMonitored';

export const SET_PERFORMER_SORT = 'performers/setPerformerSort';
export const SET_PERFORMER_FILTER = 'performers/setPerformerFilter';
export const SET_PERFORMER_VIEW = 'performers/setPerformerView';
export const SET_PERFORMER_TABLE_OPTION = 'performers/setPerformerTableOption';
export const SET_PERFORMER_POSTER_OPTION = 'performers/setPerformerPosterOption';

//
// Action Creators

export const fetchPerformers = createThunk(FETCH_PERFORMERS);
export const savePerformer = createThunk(SAVE_PERFORMER);
export const savePerformerEditor = createThunk(SAVE_PERFORMER_EDITOR);

export const togglePerformerMonitored = createThunk(TOGGLE_PERFORMER_MONITORED);

export const setPerformerSort = createAction(SET_PERFORMER_SORT);
export const setPerformerFilter = createAction(SET_PERFORMER_FILTER);
export const setPerformerView = createAction(SET_PERFORMER_VIEW);
export const setPerformerTableOption = createAction(SET_PERFORMER_TABLE_OPTION);
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
  },

  [SAVE_PERFORMER_EDITOR]: function(getState, payload, dispatch) {
    dispatch(set({
      section,
      isSaving: true
    }));

    const promise = createAjaxRequest({
      url: '/performer/editor',
      method: 'PUT',
      data: JSON.stringify(payload),
      dataType: 'json'
    }).request;

    promise.done((data) => {
      dispatch(batchActions([
        ...data.map((performer) => {
          return updateItem({
            id: performer.id,
            section: 'performers',
            ...performer
          });
        }),

        set({
          section,
          isSaving: false,
          saveError: null
        })
      ]));
    });

    promise.fail((xhr) => {
      dispatch(set({
        section,
        isSaving: false,
        saveError: xhr
      }));
    });
  }
});

//
// Reducers

export const reducers = createHandleActions({

  [SET_PERFORMER_SORT]: createSetClientSideCollectionSortReducer(section),
  [SET_PERFORMER_FILTER]: createSetClientSideCollectionFilterReducer(section),
  [SET_PERFORMER_VIEW]: function(state, { payload }) {
    return Object.assign({}, state, { view: payload.view });
  },

  [SET_PERFORMER_TABLE_OPTION]: createSetTableOptionReducer(section),
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
