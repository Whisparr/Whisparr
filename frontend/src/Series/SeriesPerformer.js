import PropTypes from 'prop-types';
import React, { Component } from 'react';
import MonitorToggleButton from 'Components/MonitorToggleButton';
import EditImportListModalConnector from 'Settings/ImportLists/ImportLists/EditImportListModalConnector';
import styles from './SeriesPerformer.css';

class SeriesPerformer extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      hasPosterError: false,
      isEditImportListModalOpen: false
    };
  }

  onAddImportListPress = (monitored) => {
    if (this.props.performerList) {
      this.props.onMonitorTogglePress(monitored);
    } else {
      this.props.onMonitorTogglePress(monitored);
      this.setState({ isEditImportListModalOpen: true });
    }
  };

  onEditImportListModalClose = () => {
    this.setState({ isEditImportListModalOpen: false });
  };

  render() {
    const {
      name,
      performerList,
      isSaving
    } = this.props;

    const monitored = performerList !== undefined && performerList.enableAutomaticAdd;
    const importListId = performerList ? performerList.id : 0;

    return (
      <div>
        <MonitorToggleButton
          className={styles.monitorToggleButton}
          monitored={monitored}
          isSaving={isSaving}
          size={15}
          onPress={this.onAddImportListPress}
        />
        {name}
        <EditImportListModalConnector
          id={importListId}
          isOpen={this.state.isEditImportListModalOpen}
          onModalClose={this.onEditImportListModalClose}
          onDeleteImportListPress={this.onDeleteImportListPress}
        />
      </div>
    );
  }
}

SeriesPerformer.propTypes = {
  tpdbId: PropTypes.number.isRequired,
  name: PropTypes.string.isRequired,
  performerList: PropTypes.object,
  isSaving: PropTypes.bool.isRequired,
  onMonitorTogglePress: PropTypes.func.isRequired
};

export default SeriesPerformer;
