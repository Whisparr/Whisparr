import React, { useMemo } from 'react';
import * as seriesTypes from 'Utilities/Series/seriesTypes';
import translate from 'Utilities/String/translate';
import EnhancedSelectInput, {
  EnhancedSelectInputProps,
  EnhancedSelectInputValue,
} from './EnhancedSelectInput';
import SeriesTypeSelectInputOption from './SeriesTypeSelectInputOption';
import SeriesTypeSelectInputSelectedValue from './SeriesTypeSelectInputSelectedValue';

interface SeriesTypeSelectInputProps
  extends EnhancedSelectInputProps<EnhancedSelectInputValue<string>, string> {
  includeNoChange: boolean;
  includeNoChangeDisabled?: boolean;
  includeMixed: boolean;
}

export interface ISeriesTypeOption {
  key: string;
  value: string;
  format?: string;
  isDisabled?: boolean;
}

const seriesTypeOptions: ISeriesTypeOption[] = [
  {
    key: seriesTypes.STANDARD,
    value: 'Scenes',
    format: 'Season and episode numbers (S01E05)',
  },
  {
    key: seriesTypes.JAV,
    value: 'JAV',
    format: 'JAV ID format (ABC-123)',
  },
];

function SeriesTypeSelectInput(props: SeriesTypeSelectInputProps) {
  const {
    includeNoChange,
    includeNoChangeDisabled = true,
    includeMixed,
  } = props;

  const values = useMemo(() => {
    const result = [...seriesTypeOptions];

    if (includeNoChange) {
      result.unshift({
        key: 'noChange',
        value: translate('NoChange'),
        isDisabled: includeNoChangeDisabled,
      });
    }

    if (includeMixed) {
      result.unshift({
        key: 'mixed',
        value: `(${translate('Mixed')})`,
        isDisabled: true,
      });
    }

    return result;
  }, [includeNoChange, includeNoChangeDisabled, includeMixed]);

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
