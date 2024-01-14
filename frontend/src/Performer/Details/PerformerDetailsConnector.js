import { push } from 'connected-react-router';
import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import * as commandNames from 'Commands/commandNames';
import { executeCommand } from 'Store/Actions/commandActions';
import { togglePerformerMonitored } from 'Store/Actions/performerActions';
import { cancelFetchReleases, clearReleases } from 'Store/Actions/releaseActions';
import createAllPerformersSelector from 'Store/Selectors/createAllPerformersSelector';
import createCommandsSelector from 'Store/Selectors/createCommandsSelector';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import { findCommand, isCommandExecuting } from 'Utilities/Command';
import PerformerDetails from './PerformerDetails';

const selectMovies = createSelector(
  (state, { foreignId }) => foreignId,
  (state) => state.movies,
  (foreignId, movies) => {
    const {
      items,
      isFetching,
      isPopulated,
      error
    } = movies;

    const filteredMovies = items.filter((movie) => movie.credits.some((credit) => credit.performer.foreignId === foreignId));
    const studios = _.orderBy(_.uniqBy(filteredMovies.map((movie) => ({ title: movie.studioTitle, foreignId: movie.studioForeignId })), 'foreignId'), 'title');
    const hasMovies = !!filteredMovies.filter((movie) => movie.itemType === 'movie').length;
    const hasScenes = !!filteredMovies.filter((movie) => movie.itemType === 'scene').length;

    return {
      isMoviesFetching: isFetching,
      isMoviesPopulated: isPopulated,
      moviesError: error,
      hasMovies,
      hasScenes,
      studios,
      sizeOnDisk: _.sumBy(filteredMovies, 'sizeOnDisk')
    };
  }
);

function createMapStateToProps() {
  return createSelector(
    (state, { foreignId }) => foreignId,
    selectMovies,
    createAllPerformersSelector(),
    createCommandsSelector(),
    createDimensionsSelector(),
    (state) => state.app.isSidebarVisible,
    (state) => state.settings.safeForWorkMode,
    (foreignId, movies, allPerformers, commands, dimensions, isSidebarVisible, safeForWorkMode) => {
      const sortedPerformers = _.orderBy(allPerformers, 'fullName');
      const performerIndex = _.findIndex(sortedPerformers, { foreignId });
      const performer = sortedPerformers[performerIndex];

      if (!performer) {
        return {};
      }

      const {
        isMoviesFetching,
        isMoviesPopulated,
        moviesError,
        hasMovies,
        hasScenes,
        studios,
        sizeOnDisk
      } = movies;

      const previousPerformer = sortedPerformers[performerIndex - 1] || _.last(sortedPerformers);
      const nextPerformer = sortedPerformers[performerIndex + 1] || _.first(sortedPerformers);
      const isMovieRefreshing = isCommandExecuting(findCommand(commands, { name: commandNames.REFRESH_MOVIE, movieIds: [performer.id] }));
      const movieRefreshingCommand = findCommand(commands, { name: commandNames.REFRESH_MOVIE });
      const allMoviesRefreshing = (
        isCommandExecuting(movieRefreshingCommand) &&
        !movieRefreshingCommand.body.movieId
      );
      const isRefreshing = isMovieRefreshing || allMoviesRefreshing;
      const isSearching = isCommandExecuting(findCommand(commands, { name: commandNames.PERFORMER_SEARCH, performerIds: [performer.id] }));

      const isFetching = isMoviesFetching;
      const isPopulated = isMoviesPopulated;

      return {
        ...performer,
        studios,
        sizeOnDisk,
        hasMovies,
        hasScenes,
        moviesError,
        isMovieRefreshing,
        allMoviesRefreshing,
        isRefreshing,
        isSearching,
        isFetching,
        isPopulated,
        previousPerformer,
        nextPerformer,
        isSmallScreen: dimensions.isSmallScreen,
        isSidebarVisible,
        safeForWorkMode
      };
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    dispatchClearReleases() {
      dispatch(clearReleases());
    },
    dispatchCancelFetchReleases() {
      dispatch(cancelFetchReleases());
    },
    dispatchTogglePerformerMonitored(payload) {
      dispatch(togglePerformerMonitored(payload));
    },
    dispatchExecuteCommand(payload) {
      dispatch(executeCommand(payload));
    },
    onGoToPerformer(foreignId) {
      dispatch(push(`${window.Whisparr.urlBase}/performer/${foreignId}`));
    }
  };
}

class PerformerDetailsConnector extends Component {

  //
  // Lifecycle

  componentDidUpdate(prevProps) {
    const {
      id
    } = this.props;

    // If the id has changed we need to clear the episodes/episode
    // files and fetch from the server.

    if (prevProps.id !== id) {
      this.unpopulate();
    }
  }

  componentWillUnmount() {
    this.unpopulate();
  }

  //
  // Control

  unpopulate = () => {
    this.props.dispatchCancelFetchReleases();
    this.props.dispatchClearReleases();
  };

  //
  // Listeners

  onMonitorTogglePress = (monitored) => {
    this.props.dispatchTogglePerformerMonitored({
      performerId: this.props.id,
      monitored
    });
  };

  onRefreshPress = () => {
    this.props.dispatchExecuteCommand({
      name: commandNames.REFRESH_PERFORMER,
      performerIds: [this.props.id]
    });
  };

  onSearchPress = () => {
    this.props.dispatchExecuteCommand({
      name: commandNames.PERFORMER_SEARCH,
      performerIds: [this.props.id]
    });
  };

  //
  // Render

  render() {
    return (
      <PerformerDetails
        {...this.props}
        onMonitorTogglePress={this.onMonitorTogglePress}
        onRefreshPress={this.onRefreshPress}
        onSearchPress={this.onSearchPress}
      />
    );
  }
}

PerformerDetailsConnector.propTypes = {
  id: PropTypes.number.isRequired,
  foreignId: PropTypes.string.isRequired,
  isMovieRefreshing: PropTypes.bool.isRequired,
  allMoviesRefreshing: PropTypes.bool.isRequired,
  isRefreshing: PropTypes.bool.isRequired,
  isSmallScreen: PropTypes.bool.isRequired,
  dispatchClearReleases: PropTypes.func.isRequired,
  dispatchCancelFetchReleases: PropTypes.func.isRequired,
  dispatchTogglePerformerMonitored: PropTypes.func.isRequired,
  dispatchExecuteCommand: PropTypes.func.isRequired,
  onGoToPerformer: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, createMapDispatchToProps)(PerformerDetailsConnector);
