import React from 'react';
import Modal from 'Components/Modal/Modal';
import PerformerIndexPosterOptionsModalContent from './PerformerIndexPosterOptionsModalContent';

interface PerformerIndexPosterOptionsModalProps {
  isOpen: boolean;
  onModalClose(...args: unknown[]): unknown;
}

function PerformerIndexPosterOptionsModal({
  isOpen,
  onModalClose,
}: PerformerIndexPosterOptionsModalProps) {
  return (
    <Modal isOpen={isOpen} onModalClose={onModalClose}>
      <PerformerIndexPosterOptionsModalContent onModalClose={onModalClose} />
    </Modal>
  );
}

export default PerformerIndexPosterOptionsModal;
