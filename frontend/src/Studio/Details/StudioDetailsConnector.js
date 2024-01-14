import { push } from 'connected-react-router';
import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import * as commandNames from 'Commands/commandNames';
import { executeCommand } from 'Store/Actions/commandActions';
import { cancelFetchReleases, clearReleases } from 'Store/Actions/releaseActions';
import { toggleStudioMonitored } from 'Store/Actions/studioActions';
import createAllStudiosSelector from 'Store/Selectors/createAllStudiosSelector';
import createCommandsSelector from 'Store/Selectors/createCommandsSelector';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import { findCommand, isCommandExecuting } from 'Utilities/Command';
import StudioDetails from './StudioDetails';

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

    const filteredMovies = items.filter((movie) => movie.studioForeignId === foreignId);
    const years = _.uniq(filteredMovies.map((movie) => movie.year)).sort();
    const hasMovies = !!filteredMovies.filter((movie) => movie.itemType === 'movie').length;
    const hasScenes = !!filteredMovies.filter((movie) => movie.itemType === 'scene').length;

    return {
      isMoviesFetching: isFetching,
      isMoviesPopulated: isPopulated,
      moviesError: error,
      hasMovies,
      hasScenes,
      years,
      sizeOnDisk: _.sumBy(filteredMovies, 'sizeOnDisk')
    };
  }
);

function createMapStateToProps() {
  return createSelector(
    (state, { foreignId }) => foreignId,
    selectMovies,
    createAllStudiosSelector(),
    createCommandsSelector(),
    createDimensionsSelector(),
    (state) => state.app.isSidebarVisible,
    (state) => state.settings.safeForWorkMode,
    (foreignId, movies, allStudios, commands, dimensions, isSidebarVisible, safeForWorkMode) => {
      const sortedStudios = _.orderBy(allStudios, 'sortTitle');
      const studioIndex = _.findIndex(sortedStudios, { foreignId });
      const studio = sortedStudios[studioIndex];

      if (!studio) {
        return {};
      }

      const {
        isMoviesFetching,
        isMoviesPopulated,
        moviesError,
        hasMovies,
        hasScenes,
        years,
        sizeOnDisk
      } = movies;

      const previousStudio = sortedStudios[studioIndex - 1] || _.last(sortedStudios);
      const nextStudio = sortedStudios[studioIndex + 1] || _.first(sortedStudios);
      const isMovieRefreshing = isCommandExecuting(findCommand(commands, { name: commandNames.REFRESH_MOVIE, movieIds: [studio.id] }));
      const movieRefreshingCommand = findCommand(commands, { name: commandNames.REFRESH_MOVIE });
      const allMoviesRefreshing = (
        isCommandExecuting(movieRefreshingCommand) &&
        !movieRefreshingCommand.body.movieId
      );
      const isRefreshing = isMovieRefreshing || allMoviesRefreshing;
      const isSearching = isCommandExecuting(findCommand(commands, { name: commandNames.STUDIO_SEARCH, studioIds: [studio.id] }));

      const isFetching = isMoviesFetching;
      const isPopulated = isMoviesPopulated;

      return {
        ...studio,
        years,
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
        previousStudio,
        nextStudio,
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
    dispatchToggleStudioMonitored(payload) {
      dispatch(toggleStudioMonitored(payload));
    },
    dispatchExecuteCommand(payload) {
      dispatch(executeCommand(payload));
    },
    onGoToStudio(foreignId) {
      dispatch(push(`${window.Whisparr.urlBase}/studio/${foreignId}`));
    }
  };
}

class StudioDetailsConnector extends Component {

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
    this.props.dispatchToggleStudioMonitored({
      studioId: this.props.id,
      monitored
    });
  };

  onRefreshPress = () => {
    this.props.dispatchExecuteCommand({
      name: commandNames.REFRESH_MOVIE,
      movieIds: [this.props.id]
    });
  };

  onSearchPress = () => {
    this.props.dispatchExecuteCommand({
      name: commandNames.STUDIO_SEARCH,
      studioIds: [this.props.id]
    });
  };

  //
  // Render

  render() {
    return (
      <StudioDetails
        {...this.props}
        onMonitorTogglePress={this.onMonitorTogglePress}
        onRefreshPress={this.onRefreshPress}
        onSearchPress={this.onSearchPress}
      />
    );
  }
}

StudioDetailsConnector.propTypes = {
  id: PropTypes.number.isRequired,
  foreignId: PropTypes.string.isRequired,
  isMovieRefreshing: PropTypes.bool.isRequired,
  allMoviesRefreshing: PropTypes.bool.isRequired,
  isRefreshing: PropTypes.bool.isRequired,
  isSmallScreen: PropTypes.bool.isRequired,
  dispatchClearReleases: PropTypes.func.isRequired,
  dispatchCancelFetchReleases: PropTypes.func.isRequired,
  dispatchToggleStudioMonitored: PropTypes.func.isRequired,
  dispatchExecuteCommand: PropTypes.func.isRequired,
  onGoToStudio: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, createMapDispatchToProps)(StudioDetailsConnector);
