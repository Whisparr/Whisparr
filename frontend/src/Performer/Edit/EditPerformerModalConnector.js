import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { clearPendingChanges } from 'Store/Actions/baseActions';
import EditPerformerModal from './EditPerformerModal';

const mapDispatchToProps = {
  clearPendingChanges
};

class EditPerformerModalConnector extends Component {

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
      <EditPerformerModal
        {...this.props}
        onModalClose={this.onModalClose}
      />
    );
  }
}

EditPerformerModalConnector.propTypes = {
  ...EditPerformerModal.propTypes,
  onModalClose: PropTypes.func.isRequired,
  clearPendingChanges: PropTypes.func.isRequired
};

export default connect(undefined, mapDispatchToProps)(EditPerformerModalConnector);
