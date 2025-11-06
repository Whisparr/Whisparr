import React from 'react';
import { createAction } from 'redux-actions';
import IconButton from 'Components/Link/IconButton';
import { icons, sortDirections } from 'Helpers/Props';
import createSetTableOptionReducer from 'Store/Actions/Creators/Reducers/createSetTableOptionReducer';
import translate from 'Utilities/String/translate';
import createHandleActions from './Creators/createHandleActions';
import createSetClientSideCollectionSortReducer from './Creators/Reducers/createSetClientSideCollectionSortReducer';

//
// Variables

export const section = 'unmappedMovieFiles';

//
// State

export const defaultState = {
  isFetching: false,
  isPopulated: false,
  sortKey: 'path',
  sortDirection: sortDirections.ASCENDING,
  error: null,
  isDeleting: false,
  deleteError: null,
  isSaving: false,
  saveError: null,
  items: [],

  sortPredicates: {
    quality: function(item, direction) {
      return item.quality ? item.qualityWeight : 0;
    }
  },

  columns: [
    {
      name: 'select',
      columnLabel: 'Select',
      isSortable: false,
      isVisible: true,
      isModifiable: false,
      isHidden: true
    },
    {
      name: 'path',
      label: () => translate('Path'),
      isSortable: true,
      isVisible: true
    },
    {
      name: 'size',
      label: () => translate('Size'),
      isSortable: true,
      isVisible: true
    },
    {
      name: 'dateAdded',
      label: () => translate('DateAdded'),
      isSortable: true,
      isVisible: true
    },
    {
      name: 'quality',
      label: () => translate('Quality'),
      isSortable: true,
      isVisible: true
    },
    {
      name: 'actions',
      columnLabel: () => translate('Actions'),
      label: React.createElement(IconButton, { name: icons.ADVANCED_SETTINGS }),
      isVisible: true,
      isModifiable: false
    }
  ]
};

export const persistState = [
  'unmappedMovieFiles.columns',
  'unmappedMovieFiles.sortKey',
  'unmappedMovieFiles.sortDirection'
];

//
// Actions Types

export const SET_UNMAPPED_MOVIE_FILES_SORT = 'unmappedMovieFiles/setUnmappedMovieFilesSort';
export const SET_UNMAPPED_MOVIE_FILES_TABLE_OPTION = 'unmappedMovieFiles/setUnmappedMovieMovieFilesTableOption';

//
// Action Creators

export const setUnmappedMovieFilesSort = createAction(SET_UNMAPPED_MOVIE_FILES_SORT);
export const setUnmappedMovieFilesTableOption = createAction(SET_UNMAPPED_MOVIE_FILES_TABLE_OPTION);

//
// Helpers

//
// Action Handlers

//
// Reducers

export const reducers = createHandleActions({
  [SET_UNMAPPED_MOVIE_FILES_SORT]: createSetClientSideCollectionSortReducer(section),
  [SET_UNMAPPED_MOVIE_FILES_TABLE_OPTION]: createSetTableOptionReducer(section)

}, defaultState, section);
