import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import * as commandNames from 'Commands/commandNames';
import withCurrentPage from 'Components/withCurrentPage';
import { executeCommand } from 'Store/Actions/commandActions';
import { deleteMovieFile, deleteMovieFiles, fetchMovieFiles } from 'Store/Actions/movieFileActions';
import { setUnmappedMovieFilesTableOption } from 'Store/Actions/unmappedMovieFileActions';
import createClientSideCollectionSelector from 'Store/Selectors/createClientSideCollectionSelector';
import createCommandExecutingSelector from 'Store/Selectors/createCommandExecutingSelector';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import { registerPagePopulator, unregisterPagePopulator } from 'Utilities/pagePopulator';
import UnmappedFilesTable from './UnmappedFilesTable';

function createMapStateToProps() {
  return createSelector(
    createClientSideCollectionSelector('movieFiles'),
    createClientSideCollectionSelector('movieFiles', 'unmappedMovieFiles'),
    createCommandExecutingSelector(commandNames.RESCAN_SCENES),
    createCommandExecutingSelector(commandNames.CLEAN_UNMAPPED_FILES),
    createDimensionsSelector(),
    (
      movieFiles,
      movieFileColumns,
      isScanningFolders,
      isCleaningUnmappedFiles,
      dimensionsState
    ) => {
      // movieFiles could pick up mapped entries via signalR so filter again here
      const {
        items,
        ...otherProps
      } = movieFiles;

      const unmappedFiles = _.filter(items, { movieId: 0 });

      return {
        ...otherProps,
        items: unmappedFiles,
        columns: movieFileColumns.columns,
        isScanningFolders,
        isCleaningUnmappedFiles,
        isSmallScreen: dimensionsState.isSmallScreen
      };
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    onTableOptionChange(payload) {
      dispatch(setUnmappedMovieFilesTableOption(payload));
    },

    fetchUnmappedFiles() {
      dispatch(fetchMovieFiles({ unmapped: true }));
    },

    deleteUnmappedFile(id) {
      dispatch(deleteMovieFile({ id }));
    },

    deleteUnmappedFiles(movieFileIds) {
      dispatch(deleteMovieFiles({ movieFileIds }));
    },

    onAddScenesPress() {
      dispatch(executeCommand({
        name: commandNames.RESCAN_SCENES,
        filter: 'matched'
      }));
    },

    onCleanUnmappedFilesPress() {
      dispatch(executeCommand({
        name: commandNames.CLEAN_UNMAPPED_FILES
      }));
    }
  };
}

class UnmappedFilesTableConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    registerPagePopulator(this.repopulate, ['movieFileUpdated']);

    this.repopulate();
  }

  componentWillUnmount() {
    unregisterPagePopulator(this.repopulate);
  }

  //
  // Control

  repopulate = () => {
    this.props.fetchUnmappedFiles();
  };

  //
  // Render

  render() {
    return (
      <UnmappedFilesTable
        {...this.props}
      />
    );
  }
}

UnmappedFilesTableConnector.propTypes = {
  isSmallScreen: PropTypes.bool.isRequired,
  onTableOptionChange: PropTypes.func.isRequired,
  fetchUnmappedFiles: PropTypes.func.isRequired,
  deleteUnmappedFile: PropTypes.func.isRequired,
  deleteUnmappedFiles: PropTypes.func.isRequired
};

export default withCurrentPage(
  connect(createMapStateToProps, createMapDispatchToProps)(UnmappedFilesTableConnector)
);
