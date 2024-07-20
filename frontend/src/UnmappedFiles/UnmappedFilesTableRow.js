import PropTypes from 'prop-types';
import React, { Component } from 'react';
import IconButton from 'Components/Link/IconButton';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import RelativeDateCell from 'Components/Table/Cells/RelativeDateCell';
import VirtualTableRowCell from 'Components/Table/Cells/VirtualTableRowCell';
import VirtualTableSelectCell from 'Components/Table/Cells/VirtualTableSelectCell';
import { icons, kinds } from 'Helpers/Props';
import InteractiveImportModal from 'InteractiveImport/InteractiveImportModal';
import MovieQuality from 'Movie/MovieQuality';
import FileDetailsModal from 'MovieFile/FileDetailsModal';
import formatBytes from 'Utilities/Number/formatBytes';
import translate from 'Utilities/String/translate';
import styles from './UnmappedFilesTableRow.css';

class UnmappedFilesTableRow extends Component {
  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isDetailsModalOpen: false,
      isInteractiveImportModalOpen: false,
      isConfirmDeleteModalOpen: false,
      sortColumn: null,
      sortDirection: 'asc' // or 'desc'
    };
  }

  //
  // Listeners

  onDetailsPress = () => {
    this.setState({ isDetailsModalOpen: true });
  };

  onDetailsModalClose = () => {
    this.setState({ isDetailsModalOpen: false });
  };

  onInteractiveImportPress = () => {
    this.setState({ isInteractiveImportModalOpen: true });
  };

  onInteractiveImportModalClose = () => {
    this.setState({ isInteractiveImportModalOpen: false });
  };

  onDeleteFilePress = () => {
    this.setState({ isConfirmDeleteModalOpen: true });
  };

  onConfirmDelete = () => {
    this.setState({ isConfirmDeleteModalOpen: false });
    this.props.deleteUnmappedFile(this.props.id);
  };

  onConfirmDeleteModalClose = () => {
    this.setState({ isConfirmDeleteModalOpen: false });
  };

  onSortColumn = (column) => {
    this.setState((prevState) => {
      const isSameColumn = prevState.sortColumn === column;
      const direction =
        isSameColumn && prevState.sortDirection === 'asc' ? 'desc' : 'asc';
      return {
        sortColumn: column,
        sortDirection: direction
      };
    });
  };

  renderSortIndicator = (column) => {
    const { sortColumn, sortDirection } = this.state;
    if (sortColumn !== column) {
      return null;
    }
    return sortDirection === 'asc' ? ' ↑' : ' ↓';
  };

  //
  // Render

  render() {
    const {
      id,
      originalFilePath,
      size,
      dateAdded,
      mediaInfo,
      quality,
      columns,
      isSelected,
      onSelectedChange
    } = this.props;

    const folder = originalFilePath.substring(
      0,
      Math.max(
        originalFilePath.lastIndexOf('/'),
        originalFilePath.lastIndexOf('\\')
      )
    );

    const {
      isInteractiveImportModalOpen,
      isDetailsModalOpen,
      isConfirmDeleteModalOpen
    } = this.state;

    return (
      <>
        {columns.map((column) => {
          const { name, isVisible } = column;

          if (!isVisible) {
            return null;
          }

          if (name === 'select') {
            return (
              <VirtualTableSelectCell
                inputClassName={styles.checkInput}
                id={id}
                key={name}
                isSelected={isSelected}
                isDisabled={false}
                onSelectedChange={onSelectedChange}
              />
            );
          }

          if (name === 'path') {
            console.log('originalFilePath:', originalFilePath);
            return (
              <VirtualTableRowCell
                key={name}
                className={styles[name]}
                title={originalFilePath?.toString() ?? ''}
              >
                {originalFilePath}
              </VirtualTableRowCell>
            );
          }

          if (name === 'size') {
            return (
              <VirtualTableRowCell key={name} className={styles[name]}>
                {formatBytes(size)}
              </VirtualTableRowCell>
            );
          }

          if (name === 'dateAdded') {
            return (
              <RelativeDateCell
                key={name}
                className={styles[name]}
                date={dateAdded}
                component={VirtualTableRowCell}
              />
            );
          }

          if (name === 'quality') {
            return (
              <VirtualTableRowCell key={name} className={styles[name]}>
                <MovieQuality quality={quality} />
              </VirtualTableRowCell>
            );
          }

          if (name === 'actions') {
            return (
              <VirtualTableRowCell key={name} className={styles[name]}>
                <IconButton name={icons.INFO} onPress={this.onDetailsPress} />

                <IconButton
                  name={icons.INTERACTIVE}
                  onPress={this.onInteractiveImportPress}
                />

                <IconButton
                  name={icons.DELETE}
                  onPress={this.onDeleteFilePress}
                />
              </VirtualTableRowCell>
            );
          }

          return null;
        })}

        <InteractiveImportModal
          isOpen={isInteractiveImportModalOpen}
          folder={folder}
          showFilterExistingFiles={true}
          filterExistingFiles={true}
          showImportMode={false}
          showReplaceExistingFiles={false}
          replaceExistingFiles={false}
          onModalClose={this.onInteractiveImportModalClose}
        />

        <FileDetailsModal
          isOpen={isDetailsModalOpen}
          onModalClose={this.onDetailsModalClose}
          mediaInfo={mediaInfo}
        />

        <ConfirmModal
          isOpen={isConfirmDeleteModalOpen}
          kind={kinds.DANGER}
          title={translate('DeleteSelectedMovieFiles')}
          message={translate('DeleteSelectedMovieFilesHelpText', [
            originalFilePath
          ])}
          confirmLabel={translate('Delete')}
          onConfirm={this.onConfirmDelete}
          onCancel={this.onConfirmDeleteModalClose}
        />
      </>
    );
  }
}

UnmappedFilesTableRow.propTypes = {
  id: PropTypes.number.isRequired,
  originalFilePath: PropTypes.string.isRequired,
  size: PropTypes.number.isRequired,
  quality: PropTypes.object.isRequired,
  dateAdded: PropTypes.string.isRequired,
  mediaInfo: PropTypes.object,
  columns: PropTypes.arrayOf(PropTypes.object).isRequired,
  isSelected: PropTypes.bool,
  onSelectedChange: PropTypes.func.isRequired,
  deleteUnmappedFile: PropTypes.func.isRequired
};

export default UnmappedFilesTableRow;
