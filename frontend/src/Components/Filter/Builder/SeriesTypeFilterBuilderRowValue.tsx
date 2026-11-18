import React from 'react';
import translate from 'Utilities/String/translate';
import FilterBuilderRowValue, {
  FilterBuilderRowValueProps,
} from './FilterBuilderRowValue';

const seriesTypeList = [
  {
    id: 'standard',
    get name() {
      return translate('Scenes');
    },
  },
  {
    id: 'jav',
    get name() {
      return translate('Jav');
    },
  },
];

type SeriesTypeFilterBuilderRowValueProps<T> = Omit<
  FilterBuilderRowValueProps<T, string>,
  'tagList'
>;

function SeriesTypeFilterBuilderRowValue<T>(
  props: SeriesTypeFilterBuilderRowValueProps<T>
) {
  return <FilterBuilderRowValue tagList={seriesTypeList} {...props} />;
}

export default SeriesTypeFilterBuilderRowValue;
