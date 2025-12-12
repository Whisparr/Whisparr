import PropTypes from 'prop-types';
import React from 'react';
import Modal from 'Components/Modal/Modal';
import { sizes } from 'Helpers/Props';
import SelectPerformerSitesModalContent from './SelectPerformerSitesModalContent';

function SelectPerformerSitesModal(props) {
  const {
    isOpen,
    onModalClose,
    ...otherProps
  } = props;

  return (
    <Modal
      isOpen={isOpen}
      size={sizes.LARGE}
      onModalClose={onModalClose}
    >
      <SelectPerformerSitesModalContent
        {...otherProps}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

SelectPerformerSitesModal.propTypes = {
  isOpen: PropTypes.bool.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default SelectPerformerSitesModal;