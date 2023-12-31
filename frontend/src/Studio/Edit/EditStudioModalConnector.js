import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { clearPendingChanges } from 'Store/Actions/baseActions';
import EditStudioModal from './EditStudioModal';

const mapDispatchToProps = {
  clearPendingChanges
};

class EditStudioModalConnector extends Component {

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
      <EditStudioModal
        {...this.props}
        onModalClose={this.onModalClose}
      />
    );
  }
}

EditStudioModalConnector.propTypes = {
  ...EditStudioModal.propTypes,
  onModalClose: PropTypes.func.isRequired,
  clearPendingChanges: PropTypes.func.isRequired
};

export default connect(undefined, mapDispatchToProps)(EditStudioModalConnector);
