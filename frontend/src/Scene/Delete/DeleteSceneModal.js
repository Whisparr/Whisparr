import PropTypes from 'prop-types';
import React from 'react';
import Modal from 'Components/Modal/Modal';
import { sizes } from 'Helpers/Props';
import DeleteSceneModalContentConnector from './DeleteSceneModalContentConnector';

function DeleteSceneModal(props) {
  const {
    isOpen,
    onModalClose,
    previousScene,
    ...otherProps
  } = props;

  return (
    <Modal
      isOpen={isOpen}
      size={sizes.MEDIUM}
      onModalClose={onModalClose}
    >
      <DeleteSceneModalContentConnector
        {...otherProps}
        onModalClose={onModalClose}
        previousScene={previousScene}
      />
    </Modal>
  );
}

DeleteSceneModal.propTypes = {
  ...DeleteSceneModalContentConnector.propTypes,
  isOpen: PropTypes.bool.isRequired,
  onModalClose: PropTypes.func.isRequired,
  previousScene: PropTypes.string
};

export default DeleteSceneModal;
