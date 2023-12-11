import React from 'react';
import Modal from 'Components/Modal/Modal';
import SceneIndexOverviewOptionsModalContent from './SceneIndexOverviewOptionsModalContent';

interface SceneIndexOverviewOptionsModalProps {
  isOpen: boolean;
  onModalClose(...args: unknown[]): void;
}

function SceneIndexOverviewOptionsModal({
  isOpen,
  onModalClose,
  ...otherProps
}: SceneIndexOverviewOptionsModalProps) {
  return (
    <Modal isOpen={isOpen} onModalClose={onModalClose}>
      <SceneIndexOverviewOptionsModalContent
        {...otherProps}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

export default SceneIndexOverviewOptionsModal;
