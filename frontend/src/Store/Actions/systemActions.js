import { createAction } from 'redux-actions';
import { filterTypes, sortDirections } from 'Helpers/Props';
import { setAppValue } from 'Store/Actions/appActions';
import { createThunk, handleThunks } from 'Store/thunks';
import createAjaxRequest from 'Utilities/createAjaxRequest';
import serverSideCollectionHandlers from 'Utilities/State/serverSideCollectionHandlers';
import translate from 'Utilities/String/translate';
import { pingServer } from './appActions';
import createFetchHandler from './Creators/createFetchHandler';
import createHandleActions from './Creators/createHandleActions';
import createServerSideCollectionHandlers from './Creators/createServerSideCollectionHandlers';
import createClearReducer from './Creators/Reducers/createClearReducer';
import createSetTableOptionReducer from './Creators/Reducers/createSetTableOptionReducer';

//
// Variables

export const section = 'system';

//
// State

export const defaultState = {
  updates: {
    isFetching: false,
    isPopulated: false,
    error: null,
    items: []
  },

  logs: {
    isFetching: false,
    isPopulated: false,
    pageSize: 50,
    sortKey: 'time',
    sortDirection: sortDirections.DESCENDING,
    error: null,
    items: [],

    columns: [
      {
        name: 'level',
        columnLabel: () => translate('Level'),
        isSortable: false,
        isVisible: true,
        isModifiable: 'disabled'
      },
      {
        name: 'time',
        label: () => translate('Time'),
        isSortable: true,
        isVisible: true,
        isModifiable: 'disabled'
      },
      {
        name: 'logger',
        label: () => translate('Component'),
        isSortable: false,
        isVisible: true,
        isModifiable: 'disabled'
      },
      {
        name: 'message',
        label: () => translate('Message'),
        isVisible: true,
        isModifiable: 'disabled'
      },
      {
        name: 'actions',
        columnLabel: () => translate('Actions'),
        isVisible: true,
        isModifiable: 'disabled'
      }
    ],

    selectedFilterKey: 'all',

    filters: [
      {
        key: 'all',
        label: () => translate('All'),
        filters: []
      },
      {
        key: 'info',
        label: () => translate('Info'),
        filters: [
          {
            key: 'level',
            value: 'info',
            type: filterTypes.EQUAL
          }
        ]
      },
      {
        key: 'warn',
        label: () => translate('Warn'),
        filters: [
          {
            key: 'level',
            value: 'warn',
            type: filterTypes.EQUAL
          }
        ]
      },
      {
        key: 'error',
        label: () => translate('Error'),
        filters: [
          {
            key: 'level',
            value: 'error',
            type: filterTypes.EQUAL
          }
        ]
      }
    ]
  }
};

export const persistState = [
  'system.logs.pageSize',
  'system.logs.sortKey',
  'system.logs.sortDirection',
  'system.logs.selectedFilterKey'
];

//
// Actions Types

export const FETCH_UPDATES = 'system/updates/fetchUpdates';

export const FETCH_LOGS = 'system/logs/fetchLogs';
export const GOTO_FIRST_LOGS_PAGE = 'system/logs/gotoLogsFirstPage';
export const GOTO_PREVIOUS_LOGS_PAGE = 'system/logs/gotoLogsPreviousPage';
export const GOTO_NEXT_LOGS_PAGE = 'system/logs/gotoLogsNextPage';
export const GOTO_LAST_LOGS_PAGE = 'system/logs/gotoLogsLastPage';
export const GOTO_LOGS_PAGE = 'system/logs/gotoLogsPage';
export const SET_LOGS_SORT = 'system/logs/setLogsSort';
export const SET_LOGS_FILTER = 'system/logs/setLogsFilter';
export const SET_LOGS_TABLE_OPTION = 'system/logs/setLogsTableOption';
export const CLEAR_LOGS_TABLE = 'system/logs/clearLogsTable';

export const RESTART = 'system/restart';
export const SHUTDOWN = 'system/shutdown';

//
// Action Creators

export const fetchUpdates = createThunk(FETCH_UPDATES);

export const fetchLogs = createThunk(FETCH_LOGS);
export const gotoLogsFirstPage = createThunk(GOTO_FIRST_LOGS_PAGE);
export const gotoLogsPreviousPage = createThunk(GOTO_PREVIOUS_LOGS_PAGE);
export const gotoLogsNextPage = createThunk(GOTO_NEXT_LOGS_PAGE);
export const gotoLogsLastPage = createThunk(GOTO_LAST_LOGS_PAGE);
export const gotoLogsPage = createThunk(GOTO_LOGS_PAGE);
export const setLogsSort = createThunk(SET_LOGS_SORT);
export const setLogsFilter = createThunk(SET_LOGS_FILTER);
export const setLogsTableOption = createAction(SET_LOGS_TABLE_OPTION);
export const clearLogsTable = createAction(CLEAR_LOGS_TABLE);

export const restart = createThunk(RESTART);
export const shutdown = createThunk(SHUTDOWN);

//
// Action Handlers

export const actionHandlers = handleThunks({

  [FETCH_UPDATES]: createFetchHandler('system.updates', '/update'),

  ...createServerSideCollectionHandlers(
    'system.logs',
    '/log',
    fetchLogs,
    {
      [serverSideCollectionHandlers.FETCH]: FETCH_LOGS,
      [serverSideCollectionHandlers.FIRST_PAGE]: GOTO_FIRST_LOGS_PAGE,
      [serverSideCollectionHandlers.PREVIOUS_PAGE]: GOTO_PREVIOUS_LOGS_PAGE,
      [serverSideCollectionHandlers.NEXT_PAGE]: GOTO_NEXT_LOGS_PAGE,
      [serverSideCollectionHandlers.LAST_PAGE]: GOTO_LAST_LOGS_PAGE,
      [serverSideCollectionHandlers.EXACT_PAGE]: GOTO_LOGS_PAGE,
      [serverSideCollectionHandlers.SORT]: SET_LOGS_SORT,
      [serverSideCollectionHandlers.FILTER]: SET_LOGS_FILTER
    }
  ),

  [RESTART]: function(getState, payload, dispatch) {
    const promise = createAjaxRequest({
      url: '/system/restart',
      method: 'POST'
    }).request;

    promise.done(() => {
      dispatch(setAppValue({ isRestarting: true }));
      dispatch(pingServer());
    });
  },

  [SHUTDOWN]: function() {
    createAjaxRequest({
      url: '/system/shutdown',
      method: 'POST'
    });
  }
});

//
// Reducers

export const reducers = createHandleActions({

  [SET_LOGS_TABLE_OPTION]: createSetTableOptionReducer('logs'),

  [CLEAR_LOGS_TABLE]: createClearReducer(section, {
    isFetching: false,
    isPopulated: false,
    error: null,
    items: [],
    totalPages: 0,
    totalRecords: 0
  })

}, defaultState, section);
