import _ from 'lodash';
import { createAction } from 'redux-actions';
import { batchActions } from 'redux-batched-actions';
import { filterBuilderTypes, filterBuilderValueTypes, sortDirections } from 'Helpers/Props';
import { createThunk, handleThunks } from 'Store/thunks';
import sortByName from 'Utilities/Array/sortByName';
import createAjaxRequest from 'Utilities/createAjaxRequest';
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
      name: 'sortTitle',
      label: () => translate('StudioTitle'),
      isSortable: true,
      isVisible: true,
      isModifiable: false
    },
    {
      name: 'network',
      label: () => translate('Network'),
      isSortable: true,
      isVisible: true,
      isModifiable: false
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
      name: 'title',
      label: () => translate('Title'),
      type: filterBuilderTypes.EXACT,
      valueType: filterBuilderValueTypes.DEFAULT
    },
    {
      name: 'network',
      label: () => translate('Network'),
      type: filterBuilderTypes.EXACT,
      optionsSelector: function(items) {
        const tagList = items.reduce((acc, studio) => {
          if (studio.network) {
            acc.push({
              id: studio.network,
              name: studio.network
            });
          }

          return acc;
        }, []);

        return tagList.sort(sortByName);
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
  'studios.defaults',
  'studios.sortKey',
  'studios.sortDirection',
  'studios.view',
  'studios.columns',
  'studios.selectedFilterKey',
  'studios.customFilters',
  'studios.posterOptions',
  'studios.tableOptions'
];

//
// Actions Types

export const FETCH_STUDIOS = 'studios/fetchStudios';
export const SAVE_STUDIO = 'studios/saveStudio';
export const SAVE_STUDIO_EDITOR = 'studios/saveStudioEditor';
export const SET_STUDIO_VALUE = 'studios/setStudioValue';

export const TOGGLE_STUDIO_MONITORED = 'studios/toggleStudioMonitored';

export const SET_STUDIO_SORT = 'studios/setStudioSort';
export const SET_STUDIO_FILTER = 'studios/setStudioFilter';
export const SET_STUDIO_VIEW = 'studios/setStudioView';
export const SET_STUDIO_TABLE_OPTION = 'studios/setStudioTableOption';
export const SET_STUDIO_POSTER_OPTION = 'studios/setStudioPosterOption';

//
// Action Creators

export const fetchStudios = createThunk(FETCH_STUDIOS);
export const saveStudio = createThunk(SAVE_STUDIO);
export const saveStudioEditor = createThunk(SAVE_STUDIO_EDITOR);

export const toggleStudioMonitored = createThunk(TOGGLE_STUDIO_MONITORED);

export const setStudioSort = createAction(SET_STUDIO_SORT);
export const setStudioFilter = createAction(SET_STUDIO_FILTER);
export const setStudioView = createAction(SET_STUDIO_VIEW);
export const setStudioTableOption = createAction(SET_STUDIO_TABLE_OPTION);
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

    const studio = _.find(getState().studios.items, { id });

    dispatch(updateItem({
      id,
      section,
      isSaving: true
    }));

    const promise = createAjaxRequest({
      url: `/studio/${id}`,
      method: 'PUT',
      data: JSON.stringify({
        ...studio,
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

  [SAVE_STUDIO_EDITOR]: function(getState, payload, dispatch) {
    dispatch(set({
      section,
      isSaving: true
    }));

    const promise = createAjaxRequest({
      url: '/studio/editor',
      method: 'PUT',
      data: JSON.stringify(payload),
      dataType: 'json'
    }).request;

    promise.done((data) => {
      dispatch(batchActions([
        ...data.map((studio) => {
          return updateItem({
            id: studio.id,
            section: 'studios',
            ...studio
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

  [SET_STUDIO_SORT]: createSetClientSideCollectionSortReducer(section),
  [SET_STUDIO_FILTER]: createSetClientSideCollectionFilterReducer(section),
  [SET_STUDIO_VIEW]: function(state, { payload }) {
    return Object.assign({}, state, { view: payload.view });
  },

  [SET_STUDIO_TABLE_OPTION]: createSetTableOptionReducer(section),
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
