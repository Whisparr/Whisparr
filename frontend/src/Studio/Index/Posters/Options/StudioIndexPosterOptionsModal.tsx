import React from 'react';
import Modal from 'Components/Modal/Modal';
import StudioIndexPosterOptionsModalContent from './StudioIndexPosterOptionsModalContent';

interface StudioIndexPosterOptionsModalProps {
  isOpen: boolean;
  onModalClose(...args: unknown[]): unknown;
}

function StudioIndexPosterOptionsModal({
  isOpen,
  onModalClose,
}: StudioIndexPosterOptionsModalProps) {
  return (
    <Modal isOpen={isOpen} onModalClose={onModalClose}>
      <StudioIndexPosterOptionsModalContent onModalClose={onModalClose} />
    </Modal>
  );
}

export default StudioIndexPosterOptionsModal;
