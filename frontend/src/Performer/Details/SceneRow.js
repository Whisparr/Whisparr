import PropTypes from 'prop-types';
import React, { Component } from 'react';
import MonitorToggleButton from 'Components/MonitorToggleButton';
import RelativeDateCellConnector from 'Components/Table/Cells/RelativeDateCellConnector';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableRow from 'Components/Table/TableRow';
import Tooltip from 'Components/Tooltip/Tooltip';
import { tooltipPositions } from 'Helpers/Props';
import MovieIndexProgressBar from 'Movie/Index/ProgressBar/MovieIndexProgressBar';
import MovieFormats from 'Movie/MovieFormats';
import MovieSearchCellConnector from 'Movie/MovieSearchCellConnector';
import MovieTitleLink from 'Movie/MovieTitleLink';
import MediaInfoConnector from 'MovieFile/MediaInfoConnector';
import * as mediaInfoTypes from 'MovieFile/mediaInfoTypes';
import MovieFileLanguageConnector from 'MovieFile/MovieFileLanguageConnector';
import formatRuntime from 'Utilities/Date/formatRuntime';
import formatBytes from 'Utilities/Number/formatBytes';
import formatCustomFormatScore from 'Utilities/Number/formatCustomFormatScore';
import styles from './SceneRow.css';

class SceneRow extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isDetailsModalOpen: false
    };
  }

  //
  // Listeners

  onManualSearchPress = () => {
    this.setState({ isDetailsModalOpen: true });
  };

  onDetailsModalClose = () => {
    this.setState({ isDetailsModalOpen: false });
  };

  onMonitorMoviePress = (monitored, options) => {
    this.props.onMonitorMoviePress(this.props.id, monitored, options);
  };

  //
  // Render

  render() {
    const {
      id,
      foreignId,
      movieFileId,
      isAvailable,
      hasFile,
      movieFile,
      monitored,
      credits,
      runtime,
      movieRuntimeFormat,
      releaseDate,
      title,
      isSaving,
      movieFilePath,
      movieFileRelativePath,
      movieFileSize,
      releaseGroup,
      customFormats,
      customFormatScore,
      columns
    } = this.props;

    return (
      <TableRow>
        {
          columns.map((column) => {
            const {
              name,
              isVisible
            } = column;

            if (!isVisible) {
              return null;
            }

            if (name === 'monitored') {
              return (
                <TableRowCell
                  key={name}
                  className={styles.monitored}
                >
                  <MonitorToggleButton
                    monitored={monitored}
                    isSaving={isSaving}
                    onPress={this.onMonitorMoviePress}
                  />
                </TableRowCell>
              );
            }

            if (name === 'title') {
              return (
                <TableRowCell
                  key={name}
                  className={styles.title}
                >
                  <MovieTitleLink
                    foreignId={foreignId}
                    title={title}
                  />
                </TableRowCell>
              );
            }

            if (name === 'credits') {
              const joinedPerformers = credits
                .slice(0, 4)
                .sort((a, b) => {
                  return a.performer.name > b.performer.name ? 1 : -1;
                })
                .map((credit) => credit.performer.name)
                .join(', ');

              return (
                <TableRowCell key={name} className={styles.performers}>
                  <span title={joinedPerformers}>{joinedPerformers}</span>
                </TableRowCell>
              );
            }

            if (name === 'path') {
              return (
                <TableRowCell key={name}>
                  {
                    movieFilePath
                  }
                </TableRowCell>
              );
            }

            if (name === 'relativePath') {
              return (
                <TableRowCell key={name}>
                  {
                    movieFileRelativePath
                  }
                </TableRowCell>
              );
            }

            if (name === 'releaseDate') {
              return (
                <RelativeDateCellConnector
                  key={name}
                  date={releaseDate}
                />
              );
            }

            if (name === 'runtime') {
              return (
                <TableRowCell
                  key={name}
                  className={styles.runtime}
                >
                  { formatRuntime(runtime, movieRuntimeFormat) }
                </TableRowCell>
              );
            }

            if (name === 'customFormats') {
              return (
                <TableRowCell key={name}>
                  <MovieFormats
                    formats={customFormats}
                  />
                </TableRowCell>
              );
            }

            if (name === 'customFormatScore') {
              return (
                <TableRowCell
                  key={name}
                  className={styles.customFormatScore}
                >
                  <Tooltip
                    anchor={formatCustomFormatScore(
                      customFormatScore,
                      customFormats.length
                    )}
                    tooltip={<MovieFormats formats={customFormats} />}
                    position={tooltipPositions.BOTTOM}
                  />
                </TableRowCell>
              );
            }

            if (name === 'languages') {
              return (
                <TableRowCell
                  key={name}
                  className={styles.languages}
                >
                  <MovieFileLanguageConnector
                    movieFileId={movieFileId}
                  />
                </TableRowCell>
              );
            }

            if (name === 'audioInfo') {
              return (
                <TableRowCell
                  key={name}
                  className={styles.audio}
                >
                  <MediaInfoConnector
                    type={mediaInfoTypes.AUDIO}
                    movieFileId={movieFileId}
                  />
                </TableRowCell>
              );
            }

            if (name === 'audioLanguages') {
              return (
                <TableRowCell
                  key={name}
                  className={styles.audioLanguages}
                >
                  <MediaInfoConnector
                    type={mediaInfoTypes.AUDIO_LANGUAGES}
                    movieFileId={movieFileId}
                  />
                </TableRowCell>
              );
            }

            if (name === 'subtitleLanguages') {
              return (
                <TableRowCell
                  key={name}
                  className={styles.subtitles}
                >
                  <MediaInfoConnector
                    type={mediaInfoTypes.SUBTITLES}
                    movieFileId={movieFileId}
                  />
                </TableRowCell>
              );
            }

            if (name === 'videoCodec') {
              return (
                <TableRowCell
                  key={name}
                  className={styles.video}
                >
                  <MediaInfoConnector
                    type={mediaInfoTypes.VIDEO}
                    movieFileId={movieFileId}
                  />
                </TableRowCell>
              );
            }

            if (name === 'videoDynamicRangeType') {
              return (
                <TableRowCell
                  key={name}
                  className={styles.videoDynamicRangeType}
                >
                  <MediaInfoConnector
                    type={mediaInfoTypes.VIDEO_DYNAMIC_RANGE_TYPE}
                    movieFileId={movieFileId}
                  />
                </TableRowCell>
              );
            }

            if (name === 'size') {
              return (
                <TableRowCell
                  key={name}
                  className={styles.size}
                >
                  {!!movieFileSize && formatBytes(movieFileSize)}
                </TableRowCell>
              );
            }

            if (name === 'releaseGroup') {
              return (
                <TableRowCell
                  key={name}
                  className={styles.releaseGroup}
                >
                  {releaseGroup}
                </TableRowCell>
              );
            }

            if (name === 'status') {
              return (
                <TableRowCell
                  key={name}
                  className={styles.status}
                >
                  <MovieIndexProgressBar
                    movieId={id}
                    isAvailable={isAvailable}
                    hasFile={hasFile}
                    movieFile={movieFile}
                    monitored={monitored}
                    detailedProgressBar={true}
                    bottomRadius={false}
                    isStandAlone={true}
                  />
                </TableRowCell>
              );
            }

            if (name === 'actions') {
              return (
                <MovieSearchCellConnector
                  key={name}
                  movieId={id}
                  movieTitle={title}
                />
              );
            }

            return null;
          })
        }
      </TableRow>
    );
  }
}

SceneRow.propTypes = {
  id: PropTypes.number.isRequired,
  foreignId: PropTypes.string.isRequired,
  movieFileId: PropTypes.number,
  isAvailable: PropTypes.bool.isRequired,
  hasFile: PropTypes.bool.isRequired,
  movieFile: PropTypes.object,
  monitored: PropTypes.bool.isRequired,
  releaseDate: PropTypes.string,
  runtime: PropTypes.number,
  movieRuntimeFormat: PropTypes.string,
  title: PropTypes.string.isRequired,
  credits: PropTypes.arrayOf(PropTypes.object),
  joinedPerformers: PropTypes.string,
  isSaving: PropTypes.bool,
  movieFilePath: PropTypes.string,
  movieFileRelativePath: PropTypes.string,
  movieFileSize: PropTypes.number,
  releaseGroup: PropTypes.string,
  customFormats: PropTypes.arrayOf(PropTypes.object),
  customFormatScore: PropTypes.number.isRequired,
  mediaInfo: PropTypes.object,
  columns: PropTypes.arrayOf(PropTypes.object).isRequired,
  onMonitorMoviePress: PropTypes.func.isRequired
};

SceneRow.defaultProps = {
  customFormats: []
};

export default SceneRow;
