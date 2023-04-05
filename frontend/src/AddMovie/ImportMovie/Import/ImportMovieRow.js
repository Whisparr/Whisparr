import PropTypes from 'prop-types';
import React from 'react';
import FormInputGroup from 'Components/Form/FormInputGroup';
import VirtualTableRowCell from 'Components/Table/Cells/VirtualTableRowCell';
import VirtualTableSelectCell from 'Components/Table/Cells/VirtualTableSelectCell';
import { inputTypes } from 'Helpers/Props';
import ImportMovieSelectMovieConnector from './SelectMovie/ImportMovieSelectMovieConnector';
import styles from './ImportMovieRow.css';

function ImportMovieRow(props) {
  const {
    id,
    relativePath,
    monitor,
    qualityProfileId,
    selectedMovie,
    isExistingMovie,
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
        isDisabled={!selectedMovie || isExistingMovie}
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

      <VirtualTableRowCell className={styles.movie}>
        <ImportMovieSelectMovieConnector
          id={id}
          isExistingMovie={isExistingMovie}
          onInputChange={onInputChange}
        />
      </VirtualTableRowCell>
    </>
  );
}

ImportMovieRow.propTypes = {
  id: PropTypes.string.isRequired,
  relativePath: PropTypes.string.isRequired,
  monitor: PropTypes.string.isRequired,
  qualityProfileId: PropTypes.number.isRequired,
  selectedMovie: PropTypes.object,
  isExistingMovie: PropTypes.bool.isRequired,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  isSelected: PropTypes.bool,
  onSelectedChange: PropTypes.func.isRequired,
  onInputChange: PropTypes.func.isRequired
};

ImportMovieRow.defaultsProps = {
  items: []
};

export default ImportMovieRow;
