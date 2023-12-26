import { createAction } from 'redux-actions';
import { filterBuilderTypes, filterBuilderValueTypes, sortDirections } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import createHandleActions from './Creators/createHandleActions';
import createSetClientSideCollectionFilterReducer from './Creators/Reducers/createSetClientSideCollectionFilterReducer';
import createSetClientSideCollectionSortReducer from './Creators/Reducers/createSetClientSideCollectionSortReducer';
import { filterPredicates, filters } from './movieActions';

//
// Variables

export const section = 'performerIndex';

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
    showName: false
  },

  sortPredicates: {
    gender: function(item) {
      const gender = item.gender;

      return gender ? gender.toLowerCase() : '';
    }
  },

  selectedFilterKey: 'all',

  filters,
  filterPredicates,

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
  'performerIndex.sortKey',
  'performerIndex.sortDirection',
  'performerIndex.selectedFilterKey',
  'performerIndex.customFilters',
  'performerIndex.posterOptions'
];

//
// Actions Types

export const SET_PERFORMER_SORT = 'performerIndex/setPerformerSort';
export const SET_PERFORMER_FILTER = 'performerIndex/setPerformerFilter';
export const SET_PERFORMER_POSTER_OPTION = 'performerIndex/setPerformerPosterOption';

//
// Action Creators

export const setPerformerSort = createAction(SET_PERFORMER_SORT);
export const setPerformerFilter = createAction(SET_PERFORMER_FILTER);
export const setPerformerPosterOption = createAction(SET_PERFORMER_POSTER_OPTION);

//
// Reducers

export const reducers = createHandleActions({

  [SET_PERFORMER_SORT]: createSetClientSideCollectionSortReducer(section),
  [SET_PERFORMER_FILTER]: createSetClientSideCollectionFilterReducer(section),

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
