import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { selectImportListSchema, setImportListFieldValue, setImportListValue } from 'Store/Actions/settingsActions';
import createMovieCreditListSelector from 'Store/Selectors/createMovieCreditListSelector';

function createMapStateToProps() {
  return createSelector(
    createMovieCreditListSelector(),
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

class MovieCreditPosterConnector extends Component {

  //
  // Listeners

  onImportListSelect = () => {
    this.props.selectImportListSchema({ implementation: 'TMDbPersonImport', implementationName: 'TMDb Person', presetName: undefined });
    this.props.setImportListFieldValue({ name: 'personId', value: this.props.performer.foreignId.toString() });
    this.props.setImportListValue({ name: 'name', value: `${this.props.performer.name} - ${this.props.performer.foreignId}` });
  };

  //
  // Render

  render() {
    const {
      performer,
      component: ItemComponent
    } = this.props;

    return (
      <ItemComponent
        {...this.props}
        performer={performer}
        onImportListSelect={this.onImportListSelect}
      />
    );
  }
}

MovieCreditPosterConnector.propTypes = {
  performer: PropTypes.object.isRequired,
  component: PropTypes.elementType.isRequired,
  selectImportListSchema: PropTypes.func.isRequired,
  setImportListFieldValue: PropTypes.func.isRequired,
  setImportListValue: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(MovieCreditPosterConnector);
