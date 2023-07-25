import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { deleteEpisodeFile, fetchEpisodeFile } from 'Store/Actions/episodeFileActions';
import { fetchRootFolders } from 'Store/Actions/rootFolderActions';
import { fetchImportListSchema } from 'Store/Actions/settingsActions';
import createEpisodeFileSelector from 'Store/Selectors/createEpisodeFileSelector';
import createEpisodeSelector from 'Store/Selectors/createEpisodeSelector';
import createSeriesSelector from 'Store/Selectors/createSeriesSelector';
import EpisodeSummary from './EpisodeSummary';

function createMapStateToProps() {
  return createSelector(
    createSeriesSelector(),
    createEpisodeSelector(),
    createEpisodeFileSelector(),
    (series, episode, episodeFile = {}) => {
      const {
        id: seriesId,
        qualityProfileId,
        network
      } = series;

      const {
        releaseDate,
        overview,
        actors
      } = episode;

      const {
        mediaInfo,
        path,
        size,
        languages,
        quality,
        qualityCutoffNotMet,
        customFormats
      } = episodeFile;

      return {
        seriesId,
        network,
        qualityProfileId,
        releaseDate,
        overview,
        actors,
        mediaInfo,
        path,
        size,
        languages,
        quality,
        qualityCutoffNotMet,
        customFormats
      };
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    onDeleteEpisodeFile() {
      dispatch(deleteEpisodeFile({
        id: props.episodeFileId,
        episodeEntity: props.episodeEntity
      }));
    },

    dispatchFetchEpisodeFile() {
      dispatch(fetchEpisodeFile({
        id: props.episodeFileId
      }));
    },

    dispatchFetchImportListSchema() {
      dispatch(fetchImportListSchema());
    },

    dispatchFetchRootFolders() {
      dispatch(fetchRootFolders());
    }
  };
}

class EpisodeSummaryConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    const {
      episodeFileId,
      path,
      dispatchFetchEpisodeFile,
      dispatchFetchImportListSchema,
      dispatchFetchRootFolders
    } = this.props;

    if (episodeFileId && !path) {
      dispatchFetchEpisodeFile({ id: episodeFileId });
    }

    dispatchFetchImportListSchema();
    dispatchFetchRootFolders();
  }

  //
  // Render

  render() {
    const {
      dispatchFetchEpisodeFile,
      dispatchFetchImportListSchema,
      dispatchFetchRootFolders,
      ...otherProps
    } = this.props;

    return <EpisodeSummary {...otherProps} />;
  }
}

EpisodeSummaryConnector.propTypes = {
  episodeFileId: PropTypes.number,
  path: PropTypes.string,
  dispatchFetchEpisodeFile: PropTypes.func.isRequired,
  dispatchFetchImportListSchema: PropTypes.func.isRequired,
  dispatchFetchRootFolders: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, createMapDispatchToProps)(EpisodeSummaryConnector);
