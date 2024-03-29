import PropTypes from 'prop-types';
import React from 'react';
import FormInputGroup from 'Components/Form/FormInputGroup';
import VirtualTableRowCell from 'Components/Table/Cells/VirtualTableRowCell';
import VirtualTableSelectCell from 'Components/Table/Cells/VirtualTableSelectCell';
import { inputTypes } from 'Helpers/Props';
import ImportSeriesSelectSeriesConnector from './SelectSeries/ImportSeriesSelectSeriesConnector';
import styles from './ImportSeriesRow.css';

function ImportSeriesRow(props) {
  const {
    id,
    relativePath,
    monitor,
    qualityProfileId,
    selectedSeries,
    isExistingSeries,
    isSelected,
    onSelectedChange,
    onInputChange
  } = props;

  return (
    <>
      <VirtualTableSelectCell
        inputClassName={styles.selectInput}
        id={id}
        isSelected={isSelected}
        isDisabled={!selectedSeries || isExistingSeries}
        onSelectedChange={onSelectedChange}
      />

      <VirtualTableRowCell className={styles.folder}>
        {relativePath}
      </VirtualTableRowCell>

      <VirtualTableRowCell className={styles.monitor}>
        <FormInputGroup
          type={inputTypes.MONITOR_EPISODES_SELECT}
          name="monitor"
          value={monitor}
          onChange={onInputChange}
        />
      </VirtualTableRowCell>

      <VirtualTableRowCell className={styles.qualityProfile}>
        <FormInputGroup
          type={inputTypes.QUALITY_PROFILE_SELECT}
          name="qualityProfileId"
          value={qualityProfileId}
          onChange={onInputChange}
        />
      </VirtualTableRowCell>

      <VirtualTableRowCell className={styles.series}>
        <ImportSeriesSelectSeriesConnector
          id={id}
          isExistingSeries={isExistingSeries}
          onInputChange={onInputChange}
        />
      </VirtualTableRowCell>
    </>
  );
}

ImportSeriesRow.propTypes = {
  id: PropTypes.string.isRequired,
  relativePath: PropTypes.string.isRequired,
  monitor: PropTypes.string.isRequired,
  qualityProfileId: PropTypes.number.isRequired,
  selectedSeries: PropTypes.object,
  isExistingSeries: PropTypes.bool.isRequired,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  isSelected: PropTypes.bool,
  onSelectedChange: PropTypes.func.isRequired,
  onInputChange: PropTypes.func.isRequired
};

ImportSeriesRow.defaultsProps = {
  items: []
};

export default ImportSeriesRow;
