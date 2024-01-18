import _ from 'lodash';
import { createAction } from 'redux-actions';
import { batchActions } from 'redux-batched-actions';
import { createThunk, handleThunks } from 'Store/thunks';
import createAjaxRequest from 'Utilities/createAjaxRequest';
import getNewMovie from 'Utilities/Movie/getNewMovie';
import getSectionState from 'Utilities/State/getSectionState';
import updateSectionState from 'Utilities/State/updateSectionState';
import { set, update, updateItem } from './baseActions';
import createHandleActions from './Creators/createHandleActions';
import createSetSettingValueReducer from './Creators/Reducers/createSetSettingValueReducer';

//
// Variables

export const section = 'addMovie';
let abortCurrentRequest = null;

//
// State

export const defaultState = {
  isFetching: false,
  isPopulated: false,
  error: null,
  isAdding: false,
  isAdded: false,
  addError: null,
  items: [],

  movieDefaults: {
    rootFolderPath: '',
    monitor: 'movieOnly',
    qualityProfileId: 0,
    searchForMovie: true,
    tags: []
  },
  performerDefaults: {
    rootFolderPath: '',
    monitor: 'movieOnly',
    qualityProfileId: 0,
    searchForPerformer: true,
    tags: []
  },
};

export const persistState = [
  'addMovie.defaults',
  'addPerformer.defaults'
];

//
// Actions Types

export const LOOKUP_MOVIE = 'addMovie/lookupMovie';
export const LOOKUP_SCENE = 'addMovie/lookupScene';
export const ADD_MOVIE = 'addMovie/addMovie';
export const ADD_PERFORMER = 'addMovie/addPerformer';
export const SET_ADD_MOVIE_VALUE = 'addMovie/setAddMovieValue';
export const SET_ADD_PERFORMER_VALUE = 'addMovie/setAddPerformerValue';
export const CLEAR_ADD_MOVIE = 'addMovie/clearAddMovie';
export const SET_ADD_MOVIE_DEFAULT = 'addMovie/setAddMovieDefault';
export const SET_ADD_PERFORMER_DEFAULT = 'addMovie/setAddPerformerDefault';

//
// Action Creators

export const lookupMovie = createThunk(LOOKUP_MOVIE);
export const lookupScene = createThunk(LOOKUP_SCENE);
export const addMovie = createThunk(ADD_MOVIE);
export const addPerformer = createThunk(ADD_PERFORMER);
export const clearAddMovie = createAction(CLEAR_ADD_MOVIE);
export const setAddMovieDefault = createAction(SET_ADD_MOVIE_DEFAULT);
export const setAddPerformerDefault = createAction(SET_ADD_PERFORMER_DEFAULT);

export const setAddMovieValue = createAction(SET_ADD_MOVIE_VALUE, (payload) => {
  return {
    section,
    ...payload
  };
});
export const setAddPerformerValue = createAction(SET_ADD_PERFORMER_VALUE, (payload) => {
  return {
    section,
    ...payload
  };
});

//
// Action Handlers

export const actionHandlers = handleThunks({

  [LOOKUP_MOVIE]: function(getState, payload, dispatch) {
    dispatch(set({ section, isFetching: true }));

    if (abortCurrentRequest) {
      abortCurrentRequest();
    }

    const { request, abortRequest } = createAjaxRequest({
      url: '/lookup/movie',
      data: {
        term: payload.term
      }
    });

    abortCurrentRequest = abortRequest;

    request.done((data) => {
      data = data.map((movie) => ({ ...movie, internalId: movie.id, id: movie.foreignId }));

      dispatch(batchActions([
        update({ section, data }),

        set({
          section,
          isFetching: false,
          isPopulated: true,
          error: null
        })
      ]));
    });

    request.fail((xhr) => {
      dispatch(set({
        section,
        isFetching: false,
        isPopulated: false,
        error: xhr.aborted ? null : xhr
      }));
    });
  },

  [LOOKUP_SCENE]: function(getState, payload, dispatch) {
    dispatch(set({ section, isFetching: true }));

    if (abortCurrentRequest) {
      abortCurrentRequest();
    }

    const { request, abortRequest } = createAjaxRequest({
      url: '/lookup/scene',
      data: {
        term: payload.term
      }
    });

    abortCurrentRequest = abortRequest;

    request.done((data) => {
      data = data.map((movie) => ({ ...movie, internalId: movie.id, id: movie.foreignId }));

      dispatch(batchActions([
        update({ section, data }),

        set({
          section,
          isFetching: false,
          isPopulated: true,
          error: null
        })
      ]));
    });

    request.fail((xhr) => {
      dispatch(set({
        section,
        isFetching: false,
        isPopulated: false,
        error: xhr.aborted ? null : xhr
      }));
    });
  },

  [ADD_MOVIE]: function(getState, payload, dispatch) {
    dispatch(set({ section, isAdding: true }));

    const foreignId = payload.foreignId;
    const items = getState().addMovie.items;
    const itemToAdd = _.find(items, { foreignId });
    const newMovie = getNewMovie(_.cloneDeep(itemToAdd.movie), payload);
    newMovie.id = 0;

    const promise = createAjaxRequest({
      url: '/movie',
      method: 'POST',
      dataType: 'json',
      contentType: 'application/json',
      data: JSON.stringify(newMovie)
    }).request;

    promise.done((data) => {
      const updatedItem = _.cloneDeep(data);
      updatedItem.internalId = updatedItem.id;
      updatedItem.id = updatedItem.foreignId;
      delete updatedItem.images;

      const actions = [
        updateItem({ section: 'movies', ...data }),
        updateItem({ section: 'addMovie', ...updatedItem }),

        set({
          section,
          isAdding: false,
          isAdded: true,
          addError: null
        })
      ];

      dispatch(batchActions(actions));
    });

    promise.fail((xhr) => {
      dispatch(set({
        section,
        isAdding: false,
        isAdded: false,
        addError: xhr
      }));
    });
  },

  [ADD_PERFORMER]: function(getState, payload, dispatch) {
    dispatch(set({ section, isAdding: true }));

    const foreignId = payload.foreignId;
    const items = getState().addMovie.items;
    const itemToAdd = _.find(items, { foreignId });
    const newPerformer = getNewMovie(_.cloneDeep(itemToAdd.performer), payload);
    newPerformer.id = 0;

    const promise = createAjaxRequest({
      url: '/performer',
      method: 'POST',
      dataType: 'json',
      contentType: 'application/json',
      data: JSON.stringify(newPerformer)
    }).request;

    promise.done((data) => {
      const updatedItem = _.cloneDeep(data);
      updatedItem.internalId = updatedItem.id;
      updatedItem.id = updatedItem.foreignId;
      delete updatedItem.images;

      const actions = [
        updateItem({ section: 'performers', ...data }),
        updateItem({ section: 'addMovie', ...updatedItem }),

        set({
          section,
          isAdding: false,
          isAdded: true,
          addError: null
        })
      ];

      dispatch(batchActions(actions));
    });

    promise.fail((xhr) => {
      dispatch(set({
        section,
        isAdding: false,
        isAdded: false,
        addError: xhr
      }));
    });
  },
});

//
// Reducers

export const reducers = createHandleActions({

  [SET_ADD_MOVIE_VALUE]: createSetSettingValueReducer(section),
  [SET_ADD_PERFORMER_VALUE]: createSetSettingValueReducer(section),

  [SET_ADD_MOVIE_DEFAULT]: function(state, { payload }) {
    const newState = getSectionState(state, section);

    newState.defaults = {
      ...newState.defaults,
      ...payload
    };

    return updateSectionState(state, section, newState);
  },
  [SET_ADD_PERFORMER_DEFAULT]: function(state, { payload }) {
    const newState = getSectionState(state, section);

    newState.defaults = {
      ...newState.defaults,
      ...payload
    };

    return updateSectionState(state, section, newState);
  },

  [CLEAR_ADD_MOVIE]: function(state) {
    const {
      movieDefaults,
      performerDefaults,
      view,
      ...otherDefaultState
    } = defaultState;

    return Object.assign({}, state, otherDefaultState);
  }

}, defaultState, section);
