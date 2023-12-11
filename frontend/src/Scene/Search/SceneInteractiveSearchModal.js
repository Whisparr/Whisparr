import PropTypes from 'prop-types';
import React from 'react';
import Modal from 'Components/Modal/Modal';
import { sizes } from 'Helpers/Props';
import SceneInteractiveSearchModalContent from './SceneInteractiveSearchModalContent';

function SceneInteractiveSearchModal(props) {
  const {
    isOpen,
    sceneId,
    onModalClose
  } = props;

  return (
    <Modal
      isOpen={isOpen}
      closeOnBackgroundClick={false}
      onModalClose={onModalClose}
      size={sizes.EXTRA_EXTRA_LARGE}
    >
      <SceneInteractiveSearchModalContent
        sceneId={sceneId}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

SceneInteractiveSearchModal.propTypes = {
  isOpen: PropTypes.bool.isRequired,
  sceneId: PropTypes.number.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default SceneInteractiveSearchModal;
