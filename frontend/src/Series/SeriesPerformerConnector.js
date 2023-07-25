import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { fetchImportListSchema, saveImportList, selectImportListSchema, setImportListFieldValue, setImportListValue } from 'Store/Actions/settingsActions';
import createSeriesPerformerListSelector from 'Store/Selectors/createSeriesPerformerListSelector';
import createSeriesSelector from 'Store/Selectors/createSeriesSelector';
import SeriesPerformer from './SeriesPerformer';

function createMapStateToProps() {
  return createSelector(
    createSeriesSelector(),
    createSeriesPerformerListSelector(),
    (state) => state.settings.importLists,
    (series, performerList, importLists) => {
      const {
        monitored,
        qualityProfileId
      } = series;

      return {
        performerList,
        monitored,
        qualityProfileId,
        isSaving: importLists.isSaving
      };
    }
  );
}

const mapDispatchToProps = {
  fetchImportListSchema,
  selectImportListSchema,
  setImportListFieldValue,
  setImportListValue,
  saveImportList
};

class SeriesPerformerConnector extends Component {

  //
  // Listeners

  onMonitorTogglePress = (monitored) => {
    if (this.props.performerList) {
      this.props.setImportListValue({ name: 'enableAutomaticAdd', value: monitored });
      this.props.saveImportList({ id: this.props.performerList.id });
    } else {
      this.props.selectImportListSchema({ implementation: 'TPDbPerformer', presetName: undefined, enabled: true, enableAutomaticAdd: true });
      this.props.setImportListFieldValue({ name: 'performerId', value: this.props.tpdbId.toString() });
      this.props.setImportListValue({ name: 'enableAutomaticAdd', value: true });
      this.props.setImportListValue({ name: 'rootFolderPath', value: '' });
      this.props.setImportListValue({ name: 'name', value: `${this.props.name} - ${this.props.tpdbId}` });
      this.props.setImportListValue({ name: 'qualityProfileId', value: this.props.qualityProfileId });
    }
  };

  //
  // Render

  render() {
    return (
      <SeriesPerformer
        {...this.props}
        onMonitorTogglePress={this.onMonitorTogglePress}
      />
    );
  }
}

SeriesPerformerConnector.propTypes = {
  tpdbId: PropTypes.number.isRequired,
  seriesId: PropTypes.number.isRequired,
  name: PropTypes.string.isRequired,
  performerList: PropTypes.object,
  monitored: PropTypes.bool.isRequired,
  qualityProfileId: PropTypes.number.isRequired,
  isSaving: PropTypes.bool.isRequired,
  fetchImportListSchema: PropTypes.func.isRequired,
  selectImportListSchema: PropTypes.func.isRequired,
  setImportListFieldValue: PropTypes.func.isRequired,
  setImportListValue: PropTypes.func.isRequired,
  saveImportList: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(SeriesPerformerConnector);
