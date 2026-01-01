import _ from 'lodash';
import { createSelector } from 'reselect';
import { filterTypePredicates, filterTypes, sortDirections } from 'Helpers/Props';
import findSelectedFilters from 'Utilities/Filter/findSelectedFilters';

function getSortClause(sortKey, sortDirection, sortPredicates) {
  if (sortPredicates && sortPredicates.hasOwnProperty(sortKey)) {
    return function(item) {
      return sortPredicates[sortKey](item, sortDirection);
    };
  }

  return function(item) {
    return item[sortKey];
  };
}

function filter(items, state) {
  const {
    selectedFilterKey,
    filters,
    customFilters,
    filterPredicates
  } = state;

  if (!selectedFilterKey) {
    return items;
  }

  const selectedFilters = findSelectedFilters(selectedFilterKey, filters, customFilters);

  return _.filter(items, (item) => {
    let i = 0;
    let accepted = true;

    while (accepted && i < selectedFilters.length) {
      const {
        key,
        value,
        type = filterTypes.EQUAL
      } = selectedFilters[i];

      if (filterPredicates && filterPredicates.hasOwnProperty(key)) {
        const predicate = filterPredicates[key];

        if (Array.isArray(value)) {
          if (
            type === filterTypes.NOT_CONTAINS ||
            type === filterTypes.NOT_EQUAL
          ) {
            accepted = value.every((v) => predicate(item, v, type));
          } else {
            accepted = value.some((v) => predicate(item, v, type));
          }
        } else {
          accepted = predicate(item, value, type);
        }
      } else if (item.hasOwnProperty(key)) {
        const predicate = filterTypePredicates[type];

        if (Array.isArray(value)) {
          if (
            type === filterTypes.NOT_CONTAINS ||
            type === filterTypes.NOT_EQUAL
          ) {
            accepted = value.every((v) => predicate(item[key], v));
          } else {
            accepted = value.some((v) => predicate(item[key], v));
          }
        } else {
          accepted = predicate(item[key], value);
        }
      } else {
        // Default to false if the filter can't be tested
        accepted = false;
      }

      i++;
    }

    return accepted;
  });
}

function sort(items, state) {
  const {
    sortKey,
    sortDirection,
    sortPredicates,
    secondarySortKey,
    secondarySortDirection
  } = state;

  const clauses = [];
  const orders = [];

  clauses.push(getSortClause(sortKey, sortDirection, sortPredicates));
  orders.push(sortDirection === sortDirections.ASCENDING ? 'asc' : 'desc');

  if (secondarySortKey &&
      secondarySortDirection &&
      (sortKey !== secondarySortKey ||
       sortDirection !== secondarySortDirection)) {
    clauses.push(getSortClause(secondarySortKey, secondarySortDirection, sortPredicates));
    orders.push(secondarySortDirection === sortDirections.ASCENDING ? 'asc' : 'desc');
  }

  return _.orderBy(items, clauses, orders);
}

export function createCustomFiltersSelector(type, alternateType) {
  return createSelector(
    (state) => state.customFilters.items,
    (customFilters) => {
      return customFilters.filter((customFilter) => {
        return customFilter.type === type || customFilter.type === alternateType;
      });
    }
  );
}

function createClientSideCollectionSelector(section, uiSection) {
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

  return createSelector(
    (state) => _.get(state, section),
    (state) => _.get(state, uiSection),
    (state) => state.customFilters.items,
    (sectionState, uiSectionState = {}, customFilters) => {
      const filterTypes = new Set([section, uiSection]);
      const alternateSection = getAlternateFilterType(section);
      const alternateUiSection = getAlternateFilterType(uiSection);

      if (alternateSection) {
        filterTypes.add(alternateSection);
      }

      if (alternateUiSection) {
        filterTypes.add(alternateUiSection);
      }

      const matchingFilters = customFilters.filter((customFilter) =>
        filterTypes.has(customFilter.type)
      );

      const state = Object.assign({}, sectionState, uiSectionState, { customFilters });

      const filtered = filter(state.items, {
        ...state,
        customFilters: matchingFilters
      });
      const sorted = sort(filtered, state);

      return {
        ...sectionState,
        ...uiSectionState,
        customFilters: matchingFilters,
        items: sorted,
        totalItems: state.items.length
      };
    }
  );
}

export default createClientSideCollectionSelector;
