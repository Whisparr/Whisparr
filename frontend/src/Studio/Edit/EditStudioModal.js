import PropTypes from 'prop-types';
import React from 'react';
import Modal from 'Components/Modal/Modal';
import EditStudioModalContentConnector from './EditStudioModalContentConnector';

function EditStudioModal({ isOpen, onModalClose, ...otherProps }) {
  return (
    <Modal
      isOpen={isOpen}
      onModalClose={onModalClose}
    >
      <EditStudioModalContentConnector
        {...otherProps}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

EditStudioModal.propTypes = {
  ...EditStudioModalContentConnector.propTypes,
  isOpen: PropTypes.bool.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default EditStudioModal;
