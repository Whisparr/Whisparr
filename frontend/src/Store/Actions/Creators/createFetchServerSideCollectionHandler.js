import _ from 'lodash';
import { batchActions } from 'redux-batched-actions';
import createAjaxRequest from 'Utilities/createAjaxRequest';
import findSelectedFilters from 'Utilities/Filter/findSelectedFilters';
import getSectionState from 'Utilities/State/getSectionState';
import { set, updateServerSideCollection } from '../baseActions';

function createFetchServerSideCollectionHandler(section, url, fetchDataAugmenter) {
  const [baseSection] = section.split('.');

  function getAlternateFilterType(value) {
    if (typeof value !== 'string') {
      return null;
    }

    if (value.endsWith('Pages')) {
      return `${value.replace(/Pages$/, '')}Index`;
    }

    if (value.endsWith('Index')) {
      return `${value.replace(/Index$/, '')}Pages`;
    }

    return null;
  }

  return function(getState, payload, dispatch) {
    dispatch(set({ section, isFetching: true }));

    const sectionState = getSectionState(getState(), section, true);
    const page = payload.page || sectionState.page || 1;

    const data = Object.assign({ page },
      _.pick(sectionState, [
        'pageSize',
        'sortDirection',
        'sortKey'
      ]));

    if (fetchDataAugmenter) {
      fetchDataAugmenter(getState, payload, data);
    }

    const {
      selectedFilterKey,
      filters
    } = sectionState;

    const filterTypes = new Set([section, baseSection]);
    const alternateSection = getAlternateFilterType(section);
    const alternateBaseSection = getAlternateFilterType(baseSection);

    if (alternateSection) {
      filterTypes.add(alternateSection);
    }

    if (alternateBaseSection) {
      filterTypes.add(alternateBaseSection);
    }

    const customFilters = getState().customFilters.items.filter((customFilter) =>
      filterTypes.has(customFilter.type)
    );

    const selectedFilters = findSelectedFilters(selectedFilterKey, filters, customFilters);

    selectedFilters.forEach((filter) => {
      data[filter.key] = filter.value;
    });

    if (selectedFilters.length) {
      data.filters = JSON.stringify(selectedFilters);
    }

    const promise = createAjaxRequest({
      url,
      data,
      traditional: true
    }).request;

    promise.done((response) => {
      dispatch(batchActions([
        updateServerSideCollection({ section, data: response }),

        set({
          section,
          isFetching: false,
          isPopulated: true,
          error: null
        })
      ]));
    });

    promise.fail((xhr) => {
      dispatch(set({
        section,
        isFetching: false,
        isPopulated: false,
        error: xhr
      }));
    });
  };
}

export default createFetchServerSideCollectionHandler;
