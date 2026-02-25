import { createSelector, createSelectorCreator, defaultMemoize } from 'reselect';
import hasDifferentItemsOrOrder from 'Utilities/Object/hasDifferentItemsOrOrder';
import createClientSideCollectionSelector from './createClientSideCollectionSelector';

function createUnoptimizedSelector(uiSection, seriesType) {
  const preFilter = seriesType ? (item) => item.seriesType === seriesType : undefined;

  return createSelector(
    createClientSideCollectionSelector('series', uiSection, preFilter),
    (series) => {
      const items = series.items.map((s) => {
        const {
          id,
          sortTitle
        } = s;

        return {
          id,
          sortTitle
        };
      });

      return {
        ...series,
        items
      };
    }
  );
}

function seriesListEqual(a, b) {
  return hasDifferentItemsOrOrder(a, b);
}

const createSeriesEqualSelector = createSelectorCreator(
  defaultMemoize,
  seriesListEqual
);

function createSeriesClientSideCollectionItemsSelector(uiSection, seriesType) {
  return createSeriesEqualSelector(
    createUnoptimizedSelector(uiSection, seriesType),
    (series) => series
  );
}

export default createSeriesClientSideCollectionItemsSelector;
