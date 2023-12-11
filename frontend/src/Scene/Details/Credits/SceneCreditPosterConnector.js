import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { selectImportListSchema, setImportListFieldValue, setImportListValue } from 'Store/Actions/settingsActions';
import createSceneCreditListSelector from 'Store/Selectors/createSceneCreditListSelector';

function createMapStateToProps() {
  return createSelector(
    createSceneCreditListSelector(),
    (importList) => {
      return {
        importList
      };
    }
  );
}

const mapDispatchToProps = {
  selectImportListSchema,
  setImportListFieldValue,
  setImportListValue
};

class SceneCreditPosterConnector extends Component {

  //
  // Listeners

  onImportListSelect = () => {
    this.props.selectImportListSchema({ implementation: 'TMDbPersonImport', implementationName: 'TMDb Person', presetName: undefined });
    this.props.setImportListFieldValue({ name: 'personId', value: this.props.tmdbId.toString() });
    this.props.setImportListValue({ name: 'name', value: `${this.props.personName} - ${this.props.tmdbId}` });
  };

  //
  // Render

  render() {
    const {
      tmdbId,
      component: ItemComponent,
      personName
    } = this.props;

    return (
      <ItemComponent
        {...this.props}
        tmdbId={tmdbId}
        personName={personName}
        onImportListSelect={this.onImportListSelect}
      />
    );
  }
}

SceneCreditPosterConnector.propTypes = {
  tmdbId: PropTypes.number.isRequired,
  personName: PropTypes.string.isRequired,
  component: PropTypes.elementType.isRequired,
  selectImportListSchema: PropTypes.func.isRequired,
  setImportListFieldValue: PropTypes.func.isRequired,
  setImportListValue: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(SceneCreditPosterConnector);
