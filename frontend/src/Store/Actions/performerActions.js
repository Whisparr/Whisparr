import _ from 'lodash';
import { createAction } from 'redux-actions';
import { batchActions } from 'redux-batched-actions';
import { filterBuilderTypes, filterBuilderValueTypes, filterTypes, sortDirections } from 'Helpers/Props';
import { createThunk, handleThunks } from 'Store/thunks';
import sortByProp from 'Utilities/Array/sortByProp';
import createAjaxRequest from 'Utilities/createAjaxRequest';
import camelCaseToString from 'Utilities/String/camelCaseToString';
import translate from 'Utilities/String/translate';
import { set, updateItem } from './baseActions';
import createFetchHandler from './Creators/createFetchHandler';
import createHandleActions from './Creators/createHandleActions';
import createRemoveItemHandler from './Creators/createRemoveItemHandler';
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

  tableOptions: {},

  deleteOptions: {
    addImportExclusion: false
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
      name: 'age',
      label: () => translate('Age'),
      isSortable: true,
      isVisible: false
    },
    {
      name: 'careerStart',
      label: () => translate('CareerStart'),
      isSortable: true,
      isVisible: false
    },
    {
      name: 'careerEnd',
      label: () => translate('CareerEnd'),
      isSortable: true,
      isVisible: false
    },
    {
      name: 'status',
      label: () => translate('Status'),
      isSortable: true,
      isVisible: false
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
      name: 'totalSceneCount',
      label: () => translate('Scenes'),
      isSortable: true,
      isVisible: true
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
    },
    {
      key: 'monitoredscenesonly',
      label: () => translate('MonitoredScenesOnly'),
      filters: [
        {
          key: 'monitored',
          value: true,
          type: filterTypes.EQUAL
        }
      ]
    },
    {
      key: 'monitoredmoviessonly',
      label: () => translate('MonitoredMoviesOnly'),
      filters: [
        {
          key: 'moviesMonitored',
          value: true,
          type: filterTypes.EQUAL
        }
      ]
    },
    {
      key: 'unmonitored',
      label: () => translate('Unmonitored'),
      filters: [
        {
          key: 'monitored',
          value: false,
          type: filterTypes.EQUAL
        }
      ]
    },
    {
      key: 'deleted',
      label: () => translate('Deleted'),
      filters: [
        {
          key: 'status',
          value: 'deleted',
          type: filterTypes.EQUAL
        }
      ]
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
      name: 'sceneCount',
      label: () => translate('SceneCount'),
      type: filterBuilderTypes.NUMBER,
      valueType: filterBuilderValueTypes.DEFAULT
    },
    {
      name: 'totalSceneCount',
      label: () => translate('TotalSceneCount'),
      type: filterBuilderTypes.NUMBER,
      valueType: filterBuilderValueTypes.DEFAULT
    },
    {
      name: 'age',
      label: () => translate('Age'),
      type: filterBuilderTypes.NUMBER,
      valueType: filterBuilderValueTypes.DEFAULT
    },
    {
      name: 'careerStart',
      label: () => translate('CareerStart'),
      type: filterBuilderTypes.NUMBER,
      valueType: filterBuilderValueTypes.DEFAULT
    },
    {
      name: 'careerEnd',
      label: () => translate('CareerEnd'),
      type: filterBuilderTypes.NUMBER,
      valueType: filterBuilderValueTypes.DEFAULT
    },
    {
      name: 'status',
      label: () => translate('Status'),
      type: filterBuilderTypes.EXACT,
      optionsSelector: function(items) {
        const tagList = ['active', 'inactive', 'unknown', 'deleted'];

        const tags = tagList.map((tag) => {
          return {
            id: tag,
            name: camelCaseToString(tag)
          };
        });

        return tags.sort(sortByProp('name'));
      }
    },
    {
      name: 'fullName',
      label: () => translate('PerformerName'),
      type: filterBuilderTypes.EXACT,
      valueType: filterBuilderValueTypes.DEFAULT
    },
    {
      name: 'rootFolderPath',
      label: () => translate('RootFolder'),
      type: filterBuilderTypes.EXACT,
      valueType: filterBuilderValueTypes.FOLDER
    },
    {
      name: 'monitor',
      label: () => translate('Monitor'),
      type: filterBuilderTypes.EXACT,
      optionsSelector: function(items) {
        const tagList = ['movieOnly', 'all', 'none'];

        const tags = tagList.map((tag) => {
          return {
            id: tag,
            name: camelCaseToString(tag)
          };
        });

        return tags.sort(sortByProp('name'));
      }
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
        const tagList = [
          'male',
          'female',
          'transMale',
          'transFemale',
          'nonBinary',
          'intersex'
        ];

        const tags = tagList.map((tag) => {
          return {
            id: tag,
            name: camelCaseToString(tag)
          };
        });

        return tags.sort(sortByProp('name'));
      }
    },
    {
      name: 'hairColor',
      label: () => translate('HairColor'),
      type: filterBuilderTypes.EXACT,
      optionsSelector: function(items) {
        const tagList = [
          'blonde',
          'black',
          'red',
          'auburn',
          'grey',
          'various',
          'bald',
          'other'
        ];

        const tags = tagList.map((tag) => {
          return {
            id: tag,
            name: camelCaseToString(tag)
          };
        });

        return tags.sort(sortByProp('name'));
      }
    },
    {
      name: 'ethnicity',
      label: () => translate('Ethnicity'),
      type: filterBuilderTypes.EXACT,
      optionsSelector: function(items) {
        const tagList = [
          'caucasian',
          'black',
          'asian',
          'latin',
          'indian',
          'middleEastern',
          'other'
        ];

        const tags = tagList.map((tag) => {
          return {
            id: tag,
            name: camelCaseToString(tag)
          };
        });

        return tags.sort(sortByProp('name'));
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

export const DELETE_PERFORMER = 'performers/deletePerformer';
export const SET_DELETE_OPTION = 'performers/setDeleteOption';

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

export const deletePerformer = createThunk(DELETE_PERFORMER, (payload) => {
  return {
    ...payload,
    queryParams: {
      deleteFiles: payload.deleteFiles,
      addImportExclusion: payload.addImportExclusion
    }
  };
});

export const setDeleteOption = createAction(SET_DELETE_OPTION);

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
  [DELETE_PERFORMER]: createRemoveItemHandler(section, '/performer'),

  [TOGGLE_PERFORMER_MONITORED]: (getState, payload, dispatch) => {
    const {
      performerId: id,
      monitored,
      moviesMonitored
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
        monitored,
        moviesMonitored
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
  [SET_DELETE_OPTION]: (state, { payload }) => {
    return {
      ...state,
      deleteOptions: {
        ...payload
      }
    };
  },

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
