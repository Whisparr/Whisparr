import React from 'react';
import Modal from 'Components/Modal/Modal';
import OrganizeScenesModalContent from './OrganizeScenesModalContent';

interface OrganizeScenesModalProps {
  isOpen: boolean;
  sceneIds: number[];
  onModalClose: () => void;
}

function OrganizeScenesModal(props: OrganizeScenesModalProps) {
  const { isOpen, onModalClose, ...otherProps } = props;

  return (
    <Modal isOpen={isOpen} onModalClose={onModalClose}>
      <OrganizeScenesModalContent {...otherProps} onModalClose={onModalClose} />
    </Modal>
  );
}

export default OrganizeScenesModal;
