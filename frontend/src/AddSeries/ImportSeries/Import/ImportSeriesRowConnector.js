import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { setImportSeriesValue } from 'Store/Actions/importSeriesActions';
import createAllSeriesSelector from 'Store/Selectors/createAllSeriesSelector';
import ImportSeriesRow from './ImportSeriesRow';

function createImportSeriesItemSelector() {
  return createSelector(
    (state, { id }) => id,
    (state) => state.importSeries.items,
    (id, items) => {
      return _.find(items, { id }) || {};
    }
  );
}

function createMapStateToProps() {
  return createSelector(
    createImportSeriesItemSelector(),
    createAllSeriesSelector(),
    (item, series) => {
      const selectedSeries = item && item.selectedSeries;
      const isExistingSeries = !!selectedSeries && _.some(series, { tvdbId: selectedSeries.tvdbId });

      return {
        ...item,
        isExistingSeries
      };
    }
  );
}

const mapDispatchToProps = {
  setImportSeriesValue
};

class ImportSeriesRowConnector extends Component {

  //
  // Listeners

  onInputChange = ({ name, value }) => {
    this.props.setImportSeriesValue({
      id: this.props.id,
      [name]: value
    });
  };

  //
  // Render

  render() {
    // Don't show the row until we have the information we require for it.

    const {
      items,
      monitor
    } = this.props;

    if (!items || !monitor) {
      return null;
    }

    return (
      <ImportSeriesRow
        {...this.props}
        onInputChange={this.onInputChange}
        onSeriesSelect={this.onSeriesSelect}
      />
    );
  }
}

ImportSeriesRowConnector.propTypes = {
  rootFolderId: PropTypes.number.isRequired,
  id: PropTypes.string.isRequired,
  monitor: PropTypes.string,
  items: PropTypes.arrayOf(PropTypes.object),
  setImportSeriesValue: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(ImportSeriesRowConnector);
