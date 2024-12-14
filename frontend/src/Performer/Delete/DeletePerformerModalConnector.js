import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { clearPendingChanges } from 'Store/Actions/baseActions';
import DeletePerformerModal from './DeletePerformerModal';

const mapDispatchToProps = {
  clearPendingChanges
};

class DeletePerformerModalConnector extends Component {

  //
  // Listeners

  onModalClose = () => {
    this.props.clearPendingChanges({ section: 'performers' });
    this.props.onModalClose();
  };

  //
  // Render

  render() {
    return (
      <DeletePerformerModal
        {...this.props}
        onModalClose={this.onModalClose}
      />
    );
  }
}

DeletePerformerModalConnector.propTypes = {
  ...DeletePerformerModal.propTypes,
  onModalClose: PropTypes.func.isRequired,
  clearPendingChanges: PropTypes.func.isRequired
};

export default connect(undefined, mapDispatchToProps)(DeletePerformerModalConnector);
