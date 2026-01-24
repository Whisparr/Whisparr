import React from 'react';
import translate from 'Utilities/String/translate';
import * as seriesTypes from 'Utilities/Series/seriesTypes';
import EnhancedSelectInput from './EnhancedSelectInput';
import SeriesTypeSelectInputOption from './SeriesTypeSelectInputOption';
import SeriesTypeSelectInputSelectedValue from './SeriesTypeSelectInputSelectedValue';

interface SeriesTypeSelectInputProps {
  includeNoChange: boolean;
  includeNoChangeDisabled?: boolean;
  includeMixed: boolean;
}

interface ISeriesTypeOption {
  key: string;
  value: string;
  format?: string;
  isDisabled?: boolean;
}

const seriesTypeOptions: ISeriesTypeOption[] = [
  {
    key: seriesTypes.STANDARD,
    value: 'Standard',
    format: 'Season and episode numbers (S01E05)',
  },
  {
    key: seriesTypes.JAV,
    value: 'JAV',
    format: 'JAV ID format (ABC-123)',
  },
];

function SeriesTypeSelectInput(props: SeriesTypeSelectInputProps) {
  const values = [...seriesTypeOptions];

  const {
    includeNoChange,
    includeNoChangeDisabled = true,
    includeMixed,
  } = props;

  if (includeNoChange) {
    values.unshift({
      key: 'noChange',
      value: translate('NoChange'),
      isDisabled: includeNoChangeDisabled,
    });
  }

  if (includeMixed) {
    values.unshift({
      key: 'mixed',
      value: `(${translate('Mixed')})`,
      isDisabled: true,
    });
  }

  return (
    <EnhancedSelectInput
      {...props}
      values={values}
      optionComponent={SeriesTypeSelectInputOption}
      selectedValueComponent={SeriesTypeSelectInputSelectedValue}
    />
  );
}

SeriesTypeSelectInput.defaultProps = {
  includeNoChange: false,
  includeMixed: false,
};

export default SeriesTypeSelectInput;
