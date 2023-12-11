import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { toggleCollectionMonitored } from 'Store/Actions/sceneCollectionActions';
import SceneCollectionLabel from './SceneCollectionLabel';

function createMapStateToProps() {
  return createSelector(
    (state, { tmdbId }) => tmdbId,
    (state) => state.sceneCollections.items,
    (tmdbId, collections) => {
      const collection = collections.find((scene) => scene.tmdbId === tmdbId);
      return {
        ...collection
      };
    }
  );
}

const mapDispatchToProps = {
  toggleCollectionMonitored
};

class SceneCollectionLabelConnector extends Component {

  //
  // Listeners

  onMonitorTogglePress = (monitored) => {
    this.props.toggleCollectionMonitored({
      collectionId: this.props.id,
      monitored
    });
  };

  //
  // Render

  render() {
    return (
      <SceneCollectionLabel
        {...this.props}
        onMonitorTogglePress={this.onMonitorTogglePress}
      />
    );
  }
}

SceneCollectionLabelConnector.propTypes = {
  tmdbId: PropTypes.number.isRequired,
  id: PropTypes.number.isRequired,
  monitored: PropTypes.bool.isRequired,
  toggleCollectionMonitored: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(SceneCollectionLabelConnector);
