import React from 'react';
import { createAction } from 'redux-actions';
import Icon from 'Components/Icon';
import { icons, sortDirections } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import createHandleActions from './Creators/createHandleActions';
import createSetClientSideCollectionSortReducer from './Creators/Reducers/createSetClientSideCollectionSortReducer';
import createSetTableOptionReducer from './Creators/Reducers/createSetTableOptionReducer';

//
// Variables

export const section = 'studioScenes';

//
// State

export const defaultState = {
  sortKey: 'fullName',
  sortDirection: sortDirections.ASCENDING,
  secondarySortKey: 'fullName',
  secondarySortDirection: sortDirections.ASCENDING,

  columns: [
    {
      name: 'monitored',
      columnLabel: () => translate('Monitored'),
      isVisible: true,
      isModifiable: false
    },
    {
      name: 'title',
      label: () => translate('Title'),
      isVisible: true,
      isSortable: true
    },
    {
      name: 'credits',
      label: 'Performers',
      isVisible: true
    },
    {
      name: 'path',
      label: () => translate('Path'),
      isVisible: false,
      isSortable: true
    },
    {
      name: 'relativePath',
      label: () => translate('RelativePath'),
      isVisible: false,
      isSortable: true
    },
    {
      name: 'releaseDate',
      label: () => translate('ReleaseDate'),
      isVisible: true,
      isSortable: true
    },
    {
      name: 'runtime',
      label: () => translate('Runtime'),
      isVisible: false,
      isSortable: true
    },
    {
      name: 'languages',
      label: () => translate('Languages'),
      isVisible: false
    },
    {
      name: 'audioInfo',
      label: () => translate('AudioInfo'),
      isVisible: false
    },
    {
      name: 'videoCodec',
      label: () => translate('VideoCodec'),
      isVisible: false
    },
    {
      name: 'videoDynamicRangeType',
      label: () => translate('VideoDynamicRange'),
      isVisible: false
    },
    {
      name: 'audioLanguages',
      label: () => translate('AudioLanguages'),
      isVisible: false
    },
    {
      name: 'subtitleLanguages',
      label: () => translate('SubtitleLanguages'),
      isVisible: false
    },
    {
      name: 'size',
      label: () => translate('Size'),
      isVisible: false,
      isSortable: true
    },
    {
      name: 'releaseGroup',
      label: () => translate('ReleaseGroup'),
      isVisible: false
    },
    {
      name: 'customFormats',
      label: () => translate('Formats'),
      isVisible: false
    },
    {
      name: 'customFormatScore',
      columnLabel: () => translate('CustomFormatScore'),
      label: React.createElement(Icon, {
        name: icons.SCORE,
        title: () => translate('CustomFormatScore')
      }),
      isVisible: false,
      isSortable: true
    },
    {
      name: 'status',
      label: () => translate('Status'),
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
  }
};

export const persistState = [
  'studioScenes.sortKey',
  'studioScenes.sortDirection',
  'studioScenes.columns',
  'studioScenes.tableOptions'
];

//
// Actions Types

export const SET_STUDIO_SCENES_SORT = 'studioScenes/setStudioScenesSort';
export const SET_STUDIO_SCENES_TABLE_OPTION = 'studioScenes/setStudioScenesTableOption';

//
// Action Creators

export const setStudioScenesSort = createAction(SET_STUDIO_SCENES_SORT);
export const setStudioScenesTableOption = createAction(SET_STUDIO_SCENES_TABLE_OPTION);

//
// Reducers

export const reducers = createHandleActions({

  [SET_STUDIO_SCENES_SORT]: createSetClientSideCollectionSortReducer(section),

  [SET_STUDIO_SCENES_TABLE_OPTION]: createSetTableOptionReducer(section)

}, defaultState, section);
