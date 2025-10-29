import React from 'react';
import Modal from 'Components/Modal/Modal';
import Movie from 'Movie/Movie';
import SelectMovieModalContent from './SelectMovieModalContent';

interface SelectMovieModalProps {
  isOpen: boolean;
  modalTitle: string;
  relativePath?: string;
  onMovieSelect(movie: Movie): void;
  onModalClose(): void;
}

function SelectMovieModal(props: SelectMovieModalProps) {
  const { isOpen, modalTitle, relativePath, onMovieSelect, onModalClose } =
    props;

  return (
    <Modal isOpen={isOpen} onModalClose={onModalClose}>
      <SelectMovieModalContent
        modalTitle={modalTitle}
        relativePath={relativePath || ''}
        onMovieSelect={onMovieSelect}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

export default SelectMovieModal;
