import React from 'react';
import Modal from 'Components/Modal/Modal';
import DeleteSceneModalContent from './DeleteSceneModalContent';

interface DeleteSceneModalProps {
  isOpen: boolean;
  sceneIds: number[];
  onModalClose(): void;
}

function DeleteSceneModal(props: DeleteSceneModalProps) {
  const { isOpen, sceneIds, onModalClose } = props;

  return (
    <Modal isOpen={isOpen} onModalClose={onModalClose}>
      <DeleteSceneModalContent
        sceneIds={sceneIds}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

export default DeleteSceneModal;
