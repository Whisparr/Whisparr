import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { clearPendingChanges } from 'Store/Actions/baseActions';
import DeleteStudioModal from './DeleteStudioModal';

const mapDispatchToProps = {
  clearPendingChanges
};

class DeleteStudioModalConnector extends Component {

  //
  // Listeners

  onModalClose = () => {
    this.props.clearPendingChanges({ section: 'studios' });
    this.props.onModalClose();
  };

  //
  // Render

  render() {
    return (
      <DeleteStudioModal
        {...this.props}
        onModalClose={this.onModalClose}
      />
    );
  }
}

DeleteStudioModalConnector.propTypes = {
  ...DeleteStudioModal.propTypes,
  onModalClose: PropTypes.func.isRequired,
  clearPendingChanges: PropTypes.func.isRequired
};

export default connect(undefined, mapDispatchToProps)(DeleteStudioModalConnector);
