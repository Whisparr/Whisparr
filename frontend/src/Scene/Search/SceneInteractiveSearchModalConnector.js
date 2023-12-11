import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { cancelFetchReleases, clearReleases } from 'Store/Actions/releaseActions';
import SceneInteractiveSearchModal from './SceneInteractiveSearchModal';

function createMapDispatchToProps(dispatch, props) {
  return {
    dispatchCancelFetchReleases() {
      dispatch(cancelFetchReleases());
    },

    dispatchClearReleases() {
      dispatch(clearReleases());
    },

    onModalClose() {
      dispatch(cancelFetchReleases());
      dispatch(clearReleases());
      props.onModalClose();
    }
  };
}

class SceneInteractiveSearchModalConnector extends Component {

  //
  // Lifecycle

  componentWillUnmount() {
    this.props.dispatchCancelFetchReleases();
    this.props.dispatchClearReleases();
  }

  //
  // Render

  render() {
    const {
      dispatchCancelFetchReleases,
      dispatchClearReleases,
      ...otherProps
    } = this.props;

    return (
      <SceneInteractiveSearchModal
        {...otherProps}
      />
    );
  }
}

SceneInteractiveSearchModalConnector.propTypes = {
  ...SceneInteractiveSearchModal.propTypes,
  dispatchCancelFetchReleases: PropTypes.func.isRequired,
  dispatchClearReleases: PropTypes.func.isRequired
};

export default connect(null, createMapDispatchToProps)(SceneInteractiveSearchModalConnector);
