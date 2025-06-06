import PropTypes from 'prop-types';
import React from 'react';
import Modal from 'Components/Modal/Modal';
import { sizes } from 'Helpers/Props';
import DeletePerformerModalContentConnector from './DeletePerformerModalContentConnector';

function DeletePerformerModal(props) {
  const {
    isOpen,
    onModalClose,
    performerIds,
    onDeletePress,
    ...otherProps
  } = props;

  return (
    <Modal
      isOpen={isOpen}
      size={sizes.MEDIUM}
      onModalClose={onModalClose}
    >
      <DeletePerformerModalContentConnector
        {...otherProps}
        performerIds={performerIds}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

DeletePerformerModal.propTypes = {
  ...DeletePerformerModalContentConnector.propTypes,
  performerIds: PropTypes.arrayOf(PropTypes.number).isRequired,
  isOpen: PropTypes.bool.isRequired,
  onDeletePress: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default DeletePerformerModal;
