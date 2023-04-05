import _ from 'lodash';
import { createAction } from 'redux-actions';
import { batchActions } from 'redux-batched-actions';
import { filterBuilderTypes, filterBuilderValueTypes, filterTypePredicates, filterTypes, sortDirections } from 'Helpers/Props';
import { createThunk, handleThunks } from 'Store/thunks';
import sortByName from 'Utilities/Array/sortByName';
import createAjaxRequest from 'Utilities/createAjaxRequest';
import dateFilterPredicate from 'Utilities/Date/dateFilterPredicate';
import { set, updateItem } from './baseActions';
import createFetchHandler from './Creators/createFetchHandler';
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
    label: 'All',
    filters: []
  },
  {
    key: 'monitored',
    label: 'Monitored Only',
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
    label: 'Unmonitored Only',
    filters: [
      {
        key: 'monitored',
        value: false,
        type: filterTypes.EQUAL
      }
    ]
  }
];

export const filterPredicates = {
  missing: function(item) {
    const { statistics = {} } = item;

    return statistics.episodeCount - statistics.episodeFileCount > 0;
  },

  added: function(item, filterValue, type) {
    return dateFilterPredicate(item.added, filterValue, type);
  },

  originalLanguage: function(item, filterValue, type) {
    const predicate = filterTypePredicates[type];
    const { originalLanguage } = item;

    return predicate(originalLanguage ? originalLanguage.name : '', filterValue);
  },

  releaseGroups: function(item, filterValue, type) {
    const { statistics = {} } = item;

    const {
      releaseGroups = []
    } = statistics;

    const predicate = filterTypePredicates[type];

    return predicate(releaseGroups, filterValue);
  },

  sizeOnDisk: function(item, filterValue, type) {
    const predicate = filterTypePredicates[type];
    const sizeOnDisk = item.statistics && item.statistics.sizeOnDisk ?
      item.statistics.sizeOnDisk :
      0;

    return predicate(sizeOnDisk, filterValue);
  }
};

export const filterBuilderProps = [
  {
    name: 'monitored',
    label: 'Monitored',
    type: filterBuilderTypes.EXACT,
    valueType: filterBuilderValueTypes.BOOL
  },
  {
    name: 'status',
    label: 'Status',
    type: filterBuilderTypes.EXACT,
    valueType: filterBuilderValueTypes.MOVIE_STATUS
  },
  {
    name: 'studio',
    label: 'Studio',
    type: filterBuilderTypes.STRING,
    optionsSelector: function(items) {
      const tagList = items.reduce((acc, movie) => {
        if (movie.studio) {
          acc.push({
            id: movie.studio,
            name: movie.studio
          });
        }

        return acc;
      }, []);

      return tagList.sort(sortByName);
    }
  },
  {
    name: 'qualityProfileId',
    label: 'Quality Profile',
    type: filterBuilderTypes.EXACT,
    valueType: filterBuilderValueTypes.QUALITY_PROFILE
  },
  {
    name: 'added',
    label: 'Added',
    type: filterBuilderTypes.DATE,
    valueType: filterBuilderValueTypes.DATE
  },
  {
    name: 'path',
    label: 'Path',
    type: filterBuilderTypes.STRING
  },
  {
    name: 'rootFolderPath',
    label: 'Root Folder Path',
    type: filterBuilderTypes.EXACT
  },
  {
    name: 'sizeOnDisk',
    label: 'Size on Disk',
    type: filterBuilderTypes.NUMBER,
    valueType: filterBuilderValueTypes.BYTES
  },
  {
    name: 'genres',
    label: 'Genres',
    type: filterBuilderTypes.ARRAY,
    optionsSelector: function(items) {
      const tagList = items.reduce((acc, movie) => {
        movie.genres.forEach((genre) => {
          acc.push({
            id: genre,
            name: genre
          });
        });

        return acc;
      }, []);

      return tagList.sort(sortByName);
    }
  },
  {
    name: 'originalLanguage',
    label: 'Original Language',
    type: filterBuilderTypes.EXACT,
    optionsSelector: function(items) {
      const languageList = items.reduce((acc, movie) => {
        if (movie.originalLanguage) {
          acc.push({
            id: movie.originalLanguage.name,
            name: movie.originalLanguage.name
          });
        }

        return acc;
      }, []);

      return languageList.sort(sortByName);
    }
  },
  {
    name: 'releaseGroups',
    label: 'Release Groups',
    type: filterBuilderTypes.ARRAY
  },
  {
    name: 'certification',
    label: 'Certification',
    type: filterBuilderTypes.EXACT
  },
  {
    name: 'tags',
    label: 'Tags',
    type: filterBuilderTypes.ARRAY,
    valueType: filterBuilderValueTypes.TAG
  }
];

export const sortPredicates = {
  status: function(item) {
    let result = 0;

    if (item.monitored) {
      result += 2;
    }

    if (item.status === 'continuing') {
      result++;
    }

    return result;
  },

  sizeOnDisk: function(item) {
    const { statistics = {} } = item;

    return statistics.sizeOnDisk || 0;
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
    addImportListExclusion: false
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

export const TOGGLE_MOVIE_MONITORED = 'movies/toggleMovieMonitored';
export const UPDATE_MOVIE_MONITOR = 'movies/updateMovieMonitor';
export const SAVE_MOVIE_EDITOR = 'movies/saveMovieEditor';
export const BULK_DELETE_MOVIE = 'movies/bulkDeleteMovie';

export const SET_DELETE_OPTION = 'movies/setDeleteOption';

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
      deleteFiles: payload.deleteFiles,
      addImportListExclusion: payload.addImportListExclusion
    }
  };
});

export const toggleMovieMonitored = createThunk(TOGGLE_MOVIE_MONITORED);
export const updateMovieMonitor = createThunk(UPDATE_MOVIE_MONITOR);
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

  [FETCH_MOVIES]: createFetchHandler(section, '/movie'),
  [SAVE_MOVIE]: createSaveProviderHandler(section, '/movie', { getAjaxOptions: getSaveAjaxOptions }),
  [DELETE_MOVIE]: createRemoveItemHandler(section, '/movie'),

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

  [UPDATE_MOVIE_MONITOR]: function(getState, payload, dispatch) {
    const {
      seriesIds,
      monitor,
      monitored
    } = payload;

    const series = [];

    seriesIds.forEach((id) => {
      const seriesToUpdate = { id };

      if (monitored != null) {
        seriesToUpdate.monitored = monitored;
      }

      series.push(seriesToUpdate);
    });

    dispatch(set({
      section,
      isSaving: true
    }));

    const promise = createAjaxRequest({
      url: '/moviePass',
      method: 'POST',
      data: JSON.stringify({
        series,
        monitoringOptions: { monitor }
      }),
      dataType: 'json'
    }).request;

    promise.done((data) => {
      dispatch(set({
        section,
        isSaving: false,
        saveError: null
      }));
    });

    promise.fail((xhr) => {
      dispatch(set({
        section,
        isSaving: false,
        saveError: xhr
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

          const {
            alternateTitles,
            images,
            rootFolderPath,
            statistics,
            ...propsToUpdate
          } = movie;

          return updateItem({
            id: movie.id,
            section: 'movies',
            ...propsToUpdate
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
      // SignaR will take care of removing the movies from the collection

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
