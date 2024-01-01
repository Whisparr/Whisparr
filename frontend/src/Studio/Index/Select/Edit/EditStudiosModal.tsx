import React from 'react';
import Modal from 'Components/Modal/Modal';
import EditStudiosModalContent from './EditStudiosModalContent';

interface EditStudiosModalProps {
  isOpen: boolean;
  studioIds: number[];
  onSavePress(payload: object): void;
  onModalClose(): void;
}

function EditStudiosModal(props: EditStudiosModalProps) {
  const { isOpen, studioIds, onSavePress, onModalClose } = props;

  return (
    <Modal isOpen={isOpen} onModalClose={onModalClose}>
      <EditStudiosModalContent
        studioIds={studioIds}
        onSavePress={onSavePress}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

export default EditStudiosModal;
