import React from 'react';
import Modal from 'Components/Modal/Modal';
import SceneIndexPosterOptionsModalContent from './SceneIndexPosterOptionsModalContent';

interface SceneIndexPosterOptionsModalProps {
  isOpen: boolean;
  onModalClose(...args: unknown[]): unknown;
}

function SceneIndexPosterOptionsModal({
  isOpen,
  onModalClose,
}: SceneIndexPosterOptionsModalProps) {
  return (
    <Modal isOpen={isOpen} onModalClose={onModalClose}>
      <SceneIndexPosterOptionsModalContent onModalClose={onModalClose} />
    </Modal>
  );
}

export default SceneIndexPosterOptionsModal;
