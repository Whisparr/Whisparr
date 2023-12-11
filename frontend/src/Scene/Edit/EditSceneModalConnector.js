import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { clearPendingChanges } from 'Store/Actions/baseActions';
import EditSceneModal from './EditSceneModal';

const mapDispatchToProps = {
  clearPendingChanges
};

class EditSceneModalConnector extends Component {

  //
  // Listeners

  onModalClose = () => {
    this.props.clearPendingChanges({ section: 'scenes' });
    this.props.onModalClose();
  };

  //
  // Render

  render() {
    return (
      <EditSceneModal
        {...this.props}
        onModalClose={this.onModalClose}
      />
    );
  }
}

EditSceneModalConnector.propTypes = {
  ...EditSceneModal.propTypes,
  onModalClose: PropTypes.func.isRequired,
  clearPendingChanges: PropTypes.func.isRequired
};

export default connect(undefined, mapDispatchToProps)(EditSceneModalConnector);
