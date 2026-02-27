import React from 'react';
import translate from 'Utilities/String/translate';
import FilterBuilderRowValue from './FilterBuilderRowValue';

const seriesTypesList = [
  {
    id: 'standard',
    get name() {
      return translate('Scenes');
    }
  },
  {
    id: 'jav',
    get name() {
      return translate('JAV');
    }
  }
];

function SeriesTypesFilterBuilderRowValue(props) {
  return (
    <FilterBuilderRowValue
      tagList={seriesTypesList}
      {...props}
    />
  );
}

export default SeriesTypesFilterBuilderRowValue;
