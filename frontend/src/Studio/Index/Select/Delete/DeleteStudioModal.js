import PropTypes from 'prop-types';
import React from 'react';
import Modal from 'Components/Modal/Modal';
import { sizes } from 'Helpers/Props';
import DeleteStudioModalContentConnector from './DeleteStudioModalContentConnector';

function DeleteStudioModal(props) {
  const {
    isOpen,
    onModalClose,
    studioIds,
    onDeletePress,
    ...otherProps
  } = props;
  return (
    <Modal
      isOpen={isOpen}
      size={sizes.MEDIUM}
      onModalClose={onModalClose}
    >
      <DeleteStudioModalContentConnector
        {...otherProps}
        studioIds={studioIds}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

DeleteStudioModal.propTypes = {
  ...DeleteStudioModalContentConnector.propTypes,
  studioIds: PropTypes.arrayOf(PropTypes.number).isRequired,
  isOpen: PropTypes.bool.isRequired,
  onDeletePress: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default DeleteStudioModal;
