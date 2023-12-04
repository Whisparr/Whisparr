import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { clearSceneHistory, fetchSceneHistory, sceneHistoryMarkAsFailed } from 'Store/Actions/sceneHistoryActions';
import SceneHistoryModalContent from './SceneHistoryModalContent';

function createMapStateToProps() {
  return createSelector(
    (state) => state.sceneHistory,
    (sceneHistory) => {
      return sceneHistory;
    }
  );
}

const mapDispatchToProps = {
  fetchSceneHistory,
  clearSceneHistory,
  sceneHistoryMarkAsFailed
};

class SceneHistoryModalContentConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    const {
      sceneId
    } = this.props;

    this.props.fetchSceneHistory({
      sceneId
    });
  }

  componentWillUnmount() {
    this.props.clearSceneHistory();
  }

  //
  // Listeners

  onMarkAsFailedPress = (historyId) => {
    const {
      sceneId
    } = this.props;

    this.props.sceneHistoryMarkAsFailed({
      historyId,
      sceneId
    });
  };

  //
  // Render

  render() {
    return (
      <SceneHistoryModalContent
        {...this.props}
        onMarkAsFailedPress={this.onMarkAsFailedPress}
      />
    );
  }
}

SceneHistoryModalContentConnector.propTypes = {
  sceneId: PropTypes.number.isRequired,
  fetchSceneHistory: PropTypes.func.isRequired,
  clearSceneHistory: PropTypes.func.isRequired,
  sceneHistoryMarkAsFailed: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(SceneHistoryModalContentConnector);
