import React, { useCallback } from 'react';
import { useDispatch } from 'react-redux';
import Modal from 'Components/Modal/Modal';
import { clearAddSeries } from 'Store/Actions/addSeriesActions';
import AddNewSeriesModalContent, {
  AddNewSeriesModalContentProps,
} from './AddNewSeriesModalContent';

interface AddNewSeriesModalProps extends AddNewSeriesModalContentProps {
  isOpen: boolean;
}

function AddNewSeriesModal({
  isOpen,
  onModalClose,
  ...otherProps
}: AddNewSeriesModalProps) {
  const dispatch = useDispatch();

  const handleModalClose = useCallback(() => {
    dispatch(clearAddSeries());
    onModalClose();
  }, [dispatch, onModalClose]);

  return (
    <Modal isOpen={isOpen} onModalClose={handleModalClose}>
      <AddNewSeriesModalContent
        {...otherProps}
        onModalClose={handleModalClose}
      />
    </Modal>
  );
}

export default AddNewSeriesModal;
