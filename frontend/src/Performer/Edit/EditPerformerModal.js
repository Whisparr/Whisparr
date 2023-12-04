import PropTypes from 'prop-types';
import React from 'react';
import Modal from 'Components/Modal/Modal';
import EditPerformerModalContentConnector from './EditPerformerModalContentConnector';

function EditPerformerModal({ isOpen, onModalClose, ...otherProps }) {
  return (
    <Modal
      isOpen={isOpen}
      onModalClose={onModalClose}
    >
      <EditPerformerModalContentConnector
        {...otherProps}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

EditPerformerModal.propTypes = {
  ...EditPerformerModalContentConnector.propTypes,
  isOpen: PropTypes.bool.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default EditPerformerModal;
