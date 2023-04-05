import { createAction } from 'redux-actions';
import { filterBuilderTypes, filterBuilderValueTypes, sortDirections } from 'Helpers/Props';
import sortByName from 'Utilities/Array/sortByName';
import translate from 'Utilities/String/translate';
import createHandleActions from './Creators/createHandleActions';
import createSetClientSideCollectionFilterReducer from './Creators/Reducers/createSetClientSideCollectionFilterReducer';
import createSetClientSideCollectionSortReducer from './Creators/Reducers/createSetClientSideCollectionSortReducer';
import createSetTableOptionReducer from './Creators/Reducers/createSetTableOptionReducer';
import { filterPredicates, filters, sortPredicates } from './movieActions';

//
// Variables

export const section = 'movieIndex';

//
// State

export const defaultState = {
  isSaving: false,
  saveError: null,
  isDeleting: false,
  deleteError: null,
  sortKey: 'sortTitle',
  sortDirection: sortDirections.ASCENDING,
  secondarySortKey: 'sortTitle',
  secondarySortDirection: sortDirections.ASCENDING,
  view: 'posters',

  posterOptions: {
    detailedProgressBar: false,
    size: 'large',
    showTitle: false,
    showMonitored: true,
    showQualityProfile: true,
    showCinemaRelease: false,
    showReleaseDate: false,
    showSearchAction: false
  },

  overviewOptions: {
    detailedProgressBar: false,
    size: 'medium',
    showMonitored: true,
    showStudio: true,
    showQualityProfile: true,
    showAdded: false,
    showPath: false,
    showSizeOnDisk: false,
    showSearchAction: false
  },

  tableOptions: {
    showSearchAction: false
  },

  columns: [
    {
      name: 'status',
      columnLabel: translate('ReleaseStatus'),
      isSortable: true,
      isVisible: true,
      isModifiable: false
    },
    {
      name: 'sortTitle',
      label: translate('MovieTitle'),
      isSortable: true,
      isVisible: true,
      isModifiable: false
    },
    {
      name: 'studio',
      label: translate('Studio'),
      isSortable: true,
      isVisible: true
    },
    {
      name: 'qualityProfileId',
      label: translate('QualityProfile'),
      isSortable: true,
      isVisible: true
    },
    {
      name: 'originalLanguage',
      label: translate('OriginalLanguage'),
      isSortable: true,
      isVisible: false
    },
    {
      name: 'added',
      label: translate('Added'),
      isSortable: true,
      isVisible: false
    },
    {
      name: 'year',
      label: translate('Year'),
      isSortable: true,
      isVisible: false
    },
    {
      name: 'runtime',
      label: translate('Runtime'),
      isSortable: true,
      isVisible: false
    },
    {
      name: 'path',
      label: translate('Path'),
      isSortable: true,
      isVisible: false
    },
    {
      name: 'sizeOnDisk',
      label: translate('SizeOnDisk'),
      isSortable: true,
      isVisible: false
    },
    {
      name: 'genres',
      label: translate('Genres'),
      isSortable: false,
      isVisible: false
    },
    {
      name: 'tags',
      label: translate('Tags'),
      isSortable: false,
      isVisible: false
    },
    {
      name: 'actions',
      columnLabel: translate('Actions'),
      isVisible: true,
      isModifiable: false
    }
  ],

  sortPredicates: {
    ...sortPredicates,

    studio: function(item) {
      const studio = item.studio;

      return studio ? studio.toLowerCase() : '';
    },

    originalLanguage: function(item) {
      const { originalLanguage ={} } = item;

      return originalLanguage.name;
    }
  },

  selectedFilterKey: 'all',

  filters,
  filterPredicates,

  filterBuilderProps: [
    {
      name: 'monitored',
      label: translate('Monitored'),
      type: filterBuilderTypes.EXACT,
      valueType: filterBuilderValueTypes.BOOL
    },
    {
      name: 'title',
      label: translate('Title'),
      type: filterBuilderTypes.STRING
    },
    {
      name: 'originalLanguage',
      label: translate('OriginalLanguage'),
      type: filterBuilderTypes.EXACT,
      optionsSelector: function(items) {
        const collectionList = items.reduce((acc, movie) => {
          if (movie.originalLanguage) {
            acc.push({
              id: movie.originalLanguage.name,
              name: movie.originalLanguage.name
            });
          }

          return acc;
        }, []);

        return collectionList.sort(sortByName);
      }
    },
    {
      name: 'studio',
      label: translate('Studio'),
      type: filterBuilderTypes.EXACT,
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
      label: translate('QualityProfile'),
      type: filterBuilderTypes.EXACT,
      valueType: filterBuilderValueTypes.QUALITY_PROFILE
    },
    {
      name: 'added',
      label: translate('Added'),
      type: filterBuilderTypes.DATE,
      valueType: filterBuilderValueTypes.DATE
    },
    {
      name: 'year',
      label: translate('Year'),
      type: filterBuilderTypes.NUMBER
    },
    {
      name: 'runtime',
      label: translate('Runtime'),
      type: filterBuilderTypes.NUMBER
    },
    {
      name: 'path',
      label: translate('Path'),
      type: filterBuilderTypes.STRING
    },
    {
      name: 'sizeOnDisk',
      label: translate('SizeOnDisk'),
      type: filterBuilderTypes.NUMBER,
      valueType: filterBuilderValueTypes.BYTES
    },
    {
      name: 'genres',
      label: translate('Genres'),
      type: filterBuilderTypes.ARRAY,
      optionsSelector: function(items) {
        const genreList = items.reduce((acc, movie) => {
          movie.genres.forEach((genre) => {
            acc.push({
              id: genre,
              name: genre
            });
          });

          return acc;
        }, []);

        return genreList.sort(sortByName);
      }
    },
    {
      name: 'tags',
      label: translate('Tags'),
      type: filterBuilderTypes.ARRAY,
      valueType: filterBuilderValueTypes.TAG
    }
  ]
};

export const persistState = [
  'movieIndex.sortKey',
  'movieIndex.sortDirection',
  'movieIndex.selectedFilterKey',
  'movieIndex.customFilters',
  'movieIndex.view',
  'movieIndex.columns',
  'movieIndex.posterOptions',
  'movieIndex.overviewOptions',
  'movieIndex.tableOptions'
];

//
// Actions Types

export const SET_MOVIE_SORT = 'movieIndex/setMovieSort';
export const SET_MOVIE_FILTER = 'movieIndex/setMovieFilter';
export const SET_MOVIE_VIEW = 'movieIndex/setMovieView';
export const SET_MOVIE_TABLE_OPTION = 'movieIndex/setMovieTableOption';
export const SET_MOVIE_POSTER_OPTION = 'movieIndex/setMoviePosterOption';
export const SET_MOVIE_OVERVIEW_OPTION = 'movieIndex/setMovieOverviewOption';

//
// Action Creators

export const setMovieSort = createAction(SET_MOVIE_SORT);
export const setMovieFilter = createAction(SET_MOVIE_FILTER);
export const setMovieView = createAction(SET_MOVIE_VIEW);
export const setMovieTableOption = createAction(SET_MOVIE_TABLE_OPTION);
export const setMoviePosterOption = createAction(SET_MOVIE_POSTER_OPTION);
export const setMovieOverviewOption = createAction(SET_MOVIE_OVERVIEW_OPTION);

//
// Reducers

export const reducers = createHandleActions({

  [SET_MOVIE_SORT]: createSetClientSideCollectionSortReducer(section),
  [SET_MOVIE_FILTER]: createSetClientSideCollectionFilterReducer(section),

  [SET_MOVIE_VIEW]: function(state, { payload }) {
    return Object.assign({}, state, { view: payload.view });
  },

  [SET_MOVIE_TABLE_OPTION]: createSetTableOptionReducer(section),

  [SET_MOVIE_POSTER_OPTION]: function(state, { payload }) {
    const posterOptions = state.posterOptions;

    return {
      ...state,
      posterOptions: {
        ...posterOptions,
        ...payload
      }
    };
  },

  [SET_MOVIE_OVERVIEW_OPTION]: function(state, { payload }) {
    const overviewOptions = state.overviewOptions;

    return {
      ...state,
      overviewOptions: {
        ...overviewOptions,
        ...payload
      }
    };
  }

}, defaultState, section);
