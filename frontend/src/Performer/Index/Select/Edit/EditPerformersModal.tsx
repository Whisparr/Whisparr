import React from 'react';
import Modal from 'Components/Modal/Modal';
import EditPerformersModalContent from './EditPerformersModalContent';

interface EditPerformersModalProps {
  isOpen: boolean;
  performerIds: number[];
  onSavePress(payload: object): void;
  onModalClose(): void;
}

function EditPerformersModal(props: EditPerformersModalProps) {
  const { isOpen, performerIds, onSavePress, onModalClose } = props;

  return (
    <Modal isOpen={isOpen} onModalClose={onModalClose}>
      <EditPerformersModalContent
        performerIds={performerIds}
        onSavePress={onSavePress}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

export default EditPerformersModal;
