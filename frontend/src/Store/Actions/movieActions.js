import _ from 'lodash';
import moment from 'moment';
import { createAction } from 'redux-actions';
import { batchActions } from 'redux-batched-actions';
import { filterTypePredicates, filterTypes, sortDirections } from 'Helpers/Props';
import { createThunk, handleThunks } from 'Store/thunks';
// import { batchActions } from 'redux-batched-actions';
import createAjaxRequest from 'Utilities/createAjaxRequest';
import dateFilterPredicate from 'Utilities/Date/dateFilterPredicate';
import padNumber from 'Utilities/Number/padNumber';
import translate from 'Utilities/String/translate';
import { set, update, updateItem } from './baseActions';
import createHandleActions from './Creators/createHandleActions';
import createRemoveItemHandler from './Creators/createRemoveItemHandler';
import createSaveProviderHandler from './Creators/createSaveProviderHandler';
import createSetSettingValueReducer from './Creators/Reducers/createSetSettingValueReducer';

//
// Variables

export const section = 'movies';

export const filters = [
  {
    key: 'all',
    label: () => translate('All'),
    filters: []
  },
  {
    key: 'monitored',
    label: () => translate('MonitoredOnly'),
    filters: [
      {
        key: 'monitored',
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
    key: 'missing',
    label: () => translate('Missing'),
    filters: [
      {
        key: 'monitored',
        value: true,
        type: filterTypes.EQUAL
      },
      {
        key: 'hasFile',
        value: false,
        type: filterTypes.EQUAL
      }
    ]
  },
  {
    key: 'wanted',
    label: () => translate('Wanted'),
    filters: [
      {
        key: 'monitored',
        value: true,
        type: filterTypes.EQUAL
      },
      {
        key: 'hasFile',
        value: false,
        type: filterTypes.EQUAL
      },
      {
        key: 'isAvailable',
        value: true,
        type: filterTypes.EQUAL
      }
    ]
  },
  {
    key: 'cutoffunmet',
    label: () => translate('CutoffUnmet'),
    filters: [
      {
        key: 'monitored',
        value: true,
        type: filterTypes.EQUAL
      },
      {
        key: 'hasFile',
        value: true,
        type: filterTypes.EQUAL
      },
      {
        key: 'qualityCutoffNotMet',
        value: true,
        type: filterTypes.EQUAL
      }
    ]
  }
];

export const filterPredicates = {
  added: function(item, filterValue, type) {
    return dateFilterPredicate(item.added, filterValue, type);
  },

  collection: function(item, filterValue, type) {
    const predicate = filterTypePredicates[type];
    const { collection } = item;

    return predicate(collection && collection.title ? collection.title : '', filterValue);
  },

  originalLanguage: function(item, filterValue, type) {
    const predicate = filterTypePredicates[type];
    const { originalLanguage } = item;

    return predicate(originalLanguage ? originalLanguage.name : '', filterValue);
  },

  releaseDate: function(item, filterValue, type) {
    return dateFilterPredicate(item.releaseDate, filterValue, type);
  },

  tmdbRating: function(item, filterValue, type) {
    const predicate = filterTypePredicates[type];

    const rating = item.ratings.tmdb ? item.ratings.tmdb.value : 0;

    return predicate(rating * 10, filterValue);
  },

  tmdbVotes: function(item, filterValue, type) {
    const predicate = filterTypePredicates[type];

    const rating = item.ratings.tmdb ? item.ratings.tmdb.votes : 0;

    return predicate(rating, filterValue);
  },

  rottenTomatoesRating: function(item, filterValue, type) {
    const predicate = filterTypePredicates[type];

    const rating = item.ratings.rottenTomatoes ? item.ratings.rottenTomatoes.value : 0;

    return predicate(rating, filterValue);
  },

  qualityCutoffNotMet: function(item) {
    const { movieFile = {} } = item;

    return movieFile.qualityCutoffNotMet;
  }
};

export const sortPredicates = {
  status: function(item) {
    let result = 0;

    if (item.monitored) {
      result += 4;
    }

    if (item.status === 'announced') {
      result++;
    }

    if (item.status === 'released') {
      result += 3;
    }

    return result;
  },

  movieStatus: function(item) {
    let result = 0;
    let qualityName = '';

    const hasMovieFile = !!item.movieFile;

    if (item.isAvailable) {
      result++;
    }

    if (item.monitored) {
      result += 2;
    }

    if (hasMovieFile) {
      // TODO: Consider Quality Weight for Sorting within status of hasMovie
      if (item.movieFile.qualityCutoffNotMet) {
        result += 4;
      } else {
        result += 8;
      }
      qualityName = item.movieFile.quality.quality.name;
    }

    return padNumber(result.toString(), 2) + qualityName;
  },

  year: function(item) {
    return item.year || undefined;
  },

  releaseDate: function(item, direction) {
    if (item.releaseDate) {
      return moment(item.releaseDate).unix();
    }

    if (direction === sortDirections.DESCENDING) {
      return -1 * Number.MAX_VALUE;
    }

    return Number.MAX_VALUE;
  }
};

//
// State

export const defaultState = {
  isFetching: false,
  isPopulated: false,
  error: null,
  isSaving: false,
  saveError: null,
  isDeleting: false,
  deleteError: null,
  items: [],
  sortKey: 'sortTitle',
  sortDirection: sortDirections.ASCENDING,
  pendingChanges: {},
  deleteOptions: {
    addImportExclusion: false
  }
};

export const persistState = [
  'movies.deleteOptions'
];

//
// Actions Types

export const FETCH_MOVIES = 'movies/fetchMovies';
export const SET_MOVIE_VALUE = 'movies/setMovieValue';
export const SAVE_MOVIE = 'movies/saveMovie';
export const DELETE_MOVIE = 'movies/deleteMovie';
export const SAVE_MOVIE_EDITOR = 'movies/saveMovieEditor';
export const BULK_DELETE_MOVIE = 'movies/bulkDeleteMovie';

export const SET_DELETE_OPTION = 'movies/setDeleteOption';

export const TOGGLE_MOVIE_MONITORED = 'movies/toggleMovieMonitored';

//
// Action Creators

export const fetchMovies = createThunk(FETCH_MOVIES);
export const saveMovie = createThunk(SAVE_MOVIE, (payload) => {
  const newPayload = {
    ...payload
  };

  if (payload.moveFiles) {
    newPayload.queryParams = {
      moveFiles: true
    };
  }

  delete newPayload.moveFiles;

  return newPayload;
});

export const deleteMovie = createThunk(DELETE_MOVIE, (payload) => {
  return {
    ...payload,
    queryParams: {
      deleteFiles: payload.deleteFiles
    }
  };
});

export const toggleMovieMonitored = createThunk(TOGGLE_MOVIE_MONITORED);
export const saveMovieEditor = createThunk(SAVE_MOVIE_EDITOR);
export const bulkDeleteMovie = createThunk(BULK_DELETE_MOVIE);

export const setMovieValue = createAction(SET_MOVIE_VALUE, (payload) => {
  return {
    section,
    ...payload
  };
});

export const setDeleteOption = createAction(SET_DELETE_OPTION);

//
// Helpers

function getSaveAjaxOptions({ ajaxOptions, payload }) {
  if (payload.moveFolder) {
    ajaxOptions.url = `${ajaxOptions.url}?moveFolder=true`;
  }

  return ajaxOptions;
}

//
// Action Handlers

export const actionHandlers = handleThunks({

  [FETCH_MOVIES]: (getState, payload, dispatch) => {
    dispatch(set({ section, isFetching: true }));

    const {
      ...otherPayload
    } = payload;

    const { request, abortRequest } = createAjaxRequest({
      url: '/movie/list',
      data: otherPayload,
      traditional: true
    });

    request.done((movieids) => {
      const requests = [];

      const chunkSize = 50000;
      for (let i = 0; i < movieids.length; i += chunkSize) {
        const chunk = movieids.slice(i, i + chunkSize);

        const promise = createAjaxRequest({
          url: '/movie/bulk',
          method: 'POST',
          contentType: 'application/json',
          dataType: 'json',
          data: JSON.stringify(chunk)
        });

        requests.push(promise.request);
      }

      Promise.all(requests)
        .catch((xhr) => {
          dispatch(set({
            section,
            isFetching: false,
            isPopulated: false,
            error: xhr.aborted ? null : xhr
          }));
        })
        .then((results) => {
          const data = results.flat();
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
    });

    request.fail((xhr) => {
      dispatch(set({
        section,
        isFetching: false,
        isPopulated: false,
        error: xhr.aborted ? null : xhr
      }));
    });

    return abortRequest;
  },
  [SAVE_MOVIE]: createSaveProviderHandler(section, '/movie', { getAjaxOptions: getSaveAjaxOptions }),
  [DELETE_MOVIE]: (getState, payload, dispatch) => {
    createRemoveItemHandler(section, '/movie')(getState, payload, dispatch);

    if (!payload.collectionTmdbId) {
      return;
    }

    const collectionToUpdate = getState().movieCollections.items.find((collection) => collection.tmdbId === payload.collectionTmdbId);

    // Skip updating if the last movie in the collection is being deleted
    if (collectionToUpdate.movies.length - collectionToUpdate.missingMovies === 1) {
      return;
    }

    const collectionData = { ...collectionToUpdate, missingMovies: collectionToUpdate.missingMovies + 1 };

    dispatch(updateItem({
      section: 'movieCollections',
      ...collectionData
    }));
  },

  [TOGGLE_MOVIE_MONITORED]: (getState, payload, dispatch) => {
    const {
      movieId: id,
      monitored
    } = payload;

    const movie = _.find(getState().movies.items, { id });

    dispatch(updateItem({
      id,
      section,
      isSaving: true
    }));

    const promise = createAjaxRequest({
      url: `/movie/${id}`,
      method: 'PUT',
      data: JSON.stringify({
        ...movie,
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

  [SAVE_MOVIE_EDITOR]: function(getState, payload, dispatch) {
    dispatch(set({
      section,
      isSaving: true
    }));

    const promise = createAjaxRequest({
      url: '/movie/editor',
      method: 'PUT',
      data: JSON.stringify(payload),
      dataType: 'json'
    }).request;

    promise.done((data) => {
      dispatch(batchActions([
        ...data.map((movie) => {
          return updateItem({
            id: movie.id,
            section: 'movies',
            ...movie
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
  },

  [BULK_DELETE_MOVIE]: function(getState, payload, dispatch) {
    dispatch(set({
      section,
      isDeleting: true
    }));

    const promise = createAjaxRequest({
      url: '/movie/editor',
      method: 'DELETE',
      data: JSON.stringify(payload),
      dataType: 'json'
    }).request;

    promise.done(() => {
      // SignaR will take care of removing the movie from the collection

      dispatch(set({
        section,
        isDeleting: false,
        deleteError: null
      }));
    });

    promise.fail((xhr) => {
      dispatch(set({
        section,
        isDeleting: false,
        deleteError: xhr
      }));
    });
  }
});

//
// Reducers

export const reducers = createHandleActions({

  [SET_MOVIE_VALUE]: createSetSettingValueReducer(section),
  [SET_DELETE_OPTION]: (state, { payload }) => {
    return {
      ...state,
      deleteOptions: {
        ...payload
      }
    };
  }

}, defaultState, section);
