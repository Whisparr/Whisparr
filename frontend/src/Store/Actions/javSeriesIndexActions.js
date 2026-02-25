import moment from 'moment';
import { createAction } from 'redux-actions';
import { sortDirections } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import createHandleActions from './Creators/createHandleActions';
import createSetClientSideCollectionFilterReducer from './Creators/Reducers/createSetClientSideCollectionFilterReducer';
import createSetClientSideCollectionSortReducer from './Creators/Reducers/createSetClientSideCollectionSortReducer';
import createSetTableOptionReducer from './Creators/Reducers/createSetTableOptionReducer';
import { filterBuilderProps, filterPredicates, filters, sortPredicates } from './seriesActions';

//
// Variables

export const section = 'javSeriesIndex';

//
// State

export const defaultState = {
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
    showTags: false,
    showSearchAction: false
  },

  overviewOptions: {
    detailedProgressBar: false,
    size: 'medium',
    showMonitored: true,
    showNetwork: true,
    showQualityProfile: true,
    showPreviousAiring: false,
    showAdded: false,
    showSeasonCount: true,
    showPath: false,
    showSizeOnDisk: false,
    showTags: false,
    showSearchAction: false
  },

  tableOptions: {
    showBanners: false,
    showSearchAction: false
  },

  columns: [
    {
      name: 'status',
      columnLabel: () => translate('Status'),
      isSortable: true,
      isVisible: true,
      isModifiable: false
    },
    {
      name: 'sortTitle',
      label: () => translate('SiteTitle'),
      isSortable: true,
      isVisible: true,
      isModifiable: false
    },
    {
      name: 'network',
      label: () => translate('Network'),
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
      name: 'nextAiring',
      label: () => translate('NextAiring'),
      isSortable: true,
      isVisible: true
    },
    {
      name: 'previousAiring',
      label: () => translate('PreviousAiring'),
      isSortable: true,
      isVisible: false
    },
    {
      name: 'originalLanguage',
      label: () => translate('OriginalLanguage'),
      isSortable: true,
      isVisible: false
    },
    {
      name: 'added',
      label: () => translate('Added'),
      isSortable: true,
      isVisible: false
    },
    {
      name: 'seasonCount',
      label: () => translate('Seasons'),
      isSortable: true,
      isVisible: true
    },
    {
      name: 'seasonFolder',
      label: () => translate('SeasonFolder'),
      isSortable: true,
      isVisible: false
    },
    {
      name: 'episodeProgress',
      label: () => translate('Episodes'),
      isSortable: true,
      isVisible: true
    },
    {
      name: 'episodeCount',
      label: () => translate('EpisodeCount'),
      isSortable: true,
      isVisible: false
    },
    {
      name: 'latestSeason',
      label: () => translate('LatestSeason'),
      isSortable: true,
      isVisible: false
    },
    {
      name: 'year',
      label: () => translate('Year'),
      isSortable: true,
      isVisible: false
    },
    {
      name: 'path',
      label: () => translate('Path'),
      isSortable: true,
      isVisible: false
    },
    {
      name: 'sizeOnDisk',
      label: () => translate('SizeOnDisk'),
      isSortable: true,
      isVisible: false
    },
    {
      name: 'genres',
      label: () => translate('Genres'),
      isSortable: false,
      isVisible: false
    },
    {
      name: 'certification',
      label: () => translate('Certification'),
      isSortable: false,
      isVisible: false
    },
    {
      name: 'releaseGroups',
      label: () => translate('ReleaseGroups'),
      isSortable: false,
      isVisible: false
    },
    {
      name: 'tags',
      label: () => translate('Tags'),
      isSortable: true,
      isVisible: false
    },
    {
      name: 'useSceneNumbering',
      label: () => translate('SceneNumbering'),
      isSortable: true,
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
    ...sortPredicates,

    network: function(item) {
      const network = item.network;

      return network ? network.toLowerCase() : '';
    },

    nextAiring: function(item, direction) {
      const nextAiring = item.nextAiring;

      if (nextAiring) {
        return moment(nextAiring).unix();
      }

      if (direction === sortDirections.DESCENDING) {
        return 0;
      }

      return Number.MAX_VALUE;
    },

    previousAiring: function(item, direction) {
      const previousAiring = item.previousAiring;

      if (previousAiring) {
        return moment(previousAiring).unix();
      }

      if (direction === sortDirections.DESCENDING) {
        return -Number.MAX_VALUE;
      }

      return Number.MAX_VALUE;
    },

    episodeProgress: function(item) {
      const { statistics = {} } = item;

      const {
        episodeCount = 0,
        episodeFileCount
      } = statistics;

      const progress = episodeCount ? episodeFileCount / episodeCount * 100 : 100;

      return progress + episodeCount / 1000000;
    },

    episodeCount: function(item) {
      const { statistics = {} } = item;

      return statistics.totalEpisodeCount || 0;
    },

    seasonCount: function(item) {
      const { statistics = {} } = item;

      return statistics.seasonCount;
    },

    originalLanguage: function(item) {
      const { originalLanguage = {} } = item;

      return originalLanguage.name;
    }
  },

  selectedFilterKey: 'all',

  filters,

  filterPredicates,

  filterBuilderProps
};

export const persistState = [
  'javSeriesIndex.sortKey',
  'javSeriesIndex.sortDirection',
  'javSeriesIndex.selectedFilterKey',
  'javSeriesIndex.customFilters',
  'javSeriesIndex.view',
  'javSeriesIndex.columns',
  'javSeriesIndex.posterOptions',
  'javSeriesIndex.overviewOptions',
  'javSeriesIndex.tableOptions'
];

//
// Actions Types

export const SET_JAV_SERIES_SORT = 'javSeriesIndex/setJavSeriesSort';
export const SET_JAV_SERIES_FILTER = 'javSeriesIndex/setJavSeriesFilter';
export const SET_JAV_SERIES_VIEW = 'javSeriesIndex/setJavSeriesView';
export const SET_JAV_SERIES_TABLE_OPTION = 'javSeriesIndex/setJavSeriesTableOption';
export const SET_JAV_SERIES_POSTER_OPTION = 'javSeriesIndex/setJavSeriesPosterOption';
export const SET_JAV_SERIES_OVERVIEW_OPTION = 'javSeriesIndex/setJavSeriesOverviewOption';

//
// Action Creators

export const setJavSeriesSort = createAction(SET_JAV_SERIES_SORT);
export const setJavSeriesFilter = createAction(SET_JAV_SERIES_FILTER);
export const setJavSeriesView = createAction(SET_JAV_SERIES_VIEW);
export const setJavSeriesTableOption = createAction(SET_JAV_SERIES_TABLE_OPTION);
export const setJavSeriesPosterOption = createAction(SET_JAV_SERIES_POSTER_OPTION);
export const setJavSeriesOverviewOption = createAction(SET_JAV_SERIES_OVERVIEW_OPTION);

//
// Reducers

export const reducers = createHandleActions({

  [SET_JAV_SERIES_SORT]: createSetClientSideCollectionSortReducer(section),
  [SET_JAV_SERIES_FILTER]: createSetClientSideCollectionFilterReducer(section),

  [SET_JAV_SERIES_VIEW]: function(state, { payload }) {
    return Object.assign({}, state, { view: payload.view });
  },

  [SET_JAV_SERIES_TABLE_OPTION]: createSetTableOptionReducer(section),

  [SET_JAV_SERIES_POSTER_OPTION]: function(state, { payload }) {
    const posterOptions = state.posterOptions;

    return {
      ...state,
      posterOptions: {
        ...posterOptions,
        ...payload
      }
    };
  },

  [SET_JAV_SERIES_OVERVIEW_OPTION]: function(state, { payload }) {
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
