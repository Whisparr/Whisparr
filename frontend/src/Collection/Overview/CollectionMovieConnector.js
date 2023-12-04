import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { toggleMovieMonitored } from 'Store/Actions/movieActions';
import createCollectionExistingMovieSelector from 'Store/Selectors/createCollectionExistingMovieSelector';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import CollectionMovie from './CollectionMovie';

function createMapStateToProps() {
  return createSelector(
    createDimensionsSelector(),
    createCollectionExistingMovieSelector(),
    (state) => state.settings.safeForWorkMode,
    (dimensions, existingMovie, safeForWorkMode) => {
      return {
        isSmallScreen: dimensions.isSmallScreen,
        isExistingMovie: !!existingMovie,
        safeForWorkMode,
        ...existingMovie
      };
    }
  );
}

const mapDispatchToProps = {
  toggleMovieMonitored
};

class CollectionMovieConnector extends Component {

  //
  // Listeners

  onMonitorTogglePress = (monitored) => {
    this.props.toggleMovieMonitored({
      movieId: this.props.id,
      monitored
    });
  };

  //
  // Render

  render() {
    return (
      <CollectionMovie
        {...this.props}
        onMonitorTogglePress={this.onMonitorTogglePress}
      />
    );
  }
}

CollectionMovieConnector.propTypes = {
  id: PropTypes.number,
  monitored: PropTypes.bool,
  toggleMovieMonitored: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(CollectionMovieConnector);
