import React from 'react';
import Modal from 'Components/Modal/Modal';
import EditScenesModalContent from './EditScenesModalContent';

interface EditScenesModalProps {
  isOpen: boolean;
  sceneIds: number[];
  onSavePress(payload: object): void;
  onModalClose(): void;
}

function EditScenesModal(props: EditScenesModalProps) {
  const { isOpen, sceneIds, onSavePress, onModalClose } = props;

  return (
    <Modal isOpen={isOpen} onModalClose={onModalClose}>
      <EditScenesModalContent
        sceneIds={sceneIds}
        onSavePress={onSavePress}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

export default EditScenesModal;
