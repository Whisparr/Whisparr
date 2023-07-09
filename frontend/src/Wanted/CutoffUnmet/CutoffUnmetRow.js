import PropTypes from 'prop-types';
import React from 'react';
import RelativeDateCellConnector from 'Components/Table/Cells/RelativeDateCellConnector';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableSelectCell from 'Components/Table/Cells/TableSelectCell';
import TableRow from 'Components/Table/TableRow';
import episodeEntities from 'Episode/episodeEntities';
import EpisodeNumber from 'Episode/EpisodeNumber';
import EpisodeSearchCellConnector from 'Episode/EpisodeSearchCellConnector';
import EpisodeStatusConnector from 'Episode/EpisodeStatusConnector';
import EpisodeTitleLink from 'Episode/EpisodeTitleLink';
import EpisodeFileLanguageConnector from 'EpisodeFile/EpisodeFileLanguageConnector';
import SeriesTitleLink from 'Series/SeriesTitleLink';
import styles from './CutoffUnmetRow.css';

function CutoffUnmetRow(props) {
  const {
    id,
    episodeFileId,
    series,
    seasonNumber,
    absoluteEpisodeNumber,
    unverifiedSceneNumbering,
    actors,
    releaseDate,
    title,
    isSelected,
    columns,
    onSelectedChange
  } = props;

  return (
    <TableRow>
      <TableSelectCell
        id={id}
        isSelected={isSelected}
        onSelectedChange={onSelectedChange}
      />

      {
        columns.map((column) => {
          const {
            name,
            isVisible
          } = column;

          if (!isVisible) {
            return null;
          }

          if (name === 'series.sortTitle') {
            return (
              <TableRowCell key={name}>
                <SeriesTitleLink
                  titleSlug={series.titleSlug}
                  title={series.title}
                />
              </TableRowCell>
            );
          }

          if (name === 'episode') {
            return (
              <TableRowCell
                key={name}
                className={styles.episode}
              >
                <EpisodeNumber
                  seasonNumber={seasonNumber}
                  absoluteEpisodeNumber={absoluteEpisodeNumber}
                  alternateTitles={series.alternateTitles}
                  unverifiedSceneNumbering={unverifiedSceneNumbering}
                />
              </TableRowCell>
            );
          }

          if (name === 'episodes.title') {
            return (
              <TableRowCell key={name}>
                <EpisodeTitleLink
                  episodeId={id}
                  seriesId={series.id}
                  episodeEntity={episodeEntities.WANTED_CUTOFF_UNMET}
                  episodeTitle={title}
                  showOpenSeriesButton={true}
                />
              </TableRowCell>
            );
          }

          if (name === 'actors') {
            const joinedPerformers = actors.map((a) => a.character).slice(0, 4).join(', ');

            return (
              <TableRowCell
                key={name}
                className={styles.actors}
              >
                <span title={joinedPerformers}>
                  {joinedPerformers}
                </span>
              </TableRowCell>
            );
          }

          if (name === 'episodes.airDateUtc') {
            return (
              <RelativeDateCellConnector
                key={name}
                date={releaseDate}
              />
            );
          }

          if (name === 'languages') {
            return (
              <TableRowCell
                key={name}
                className={styles.languages}
              >
                <EpisodeFileLanguageConnector
                  episodeFileId={episodeFileId}
                />
              </TableRowCell>
            );
          }

          if (name === 'status') {
            return (
              <TableRowCell
                key={name}
                className={styles.status}
              >
                <EpisodeStatusConnector
                  episodeId={id}
                  episodeFileId={episodeFileId}
                  episodeEntity={episodeEntities.WANTED_CUTOFF_UNMET}
                />
              </TableRowCell>
            );
          }

          if (name === 'actions') {
            return (
              <EpisodeSearchCellConnector
                key={name}
                episodeId={id}
                seriesId={series.id}
                episodeTitle={title}
                episodeEntity={episodeEntities.WANTED_CUTOFF_UNMET}
                showOpenSeriesButton={true}
              />
            );
          }

          return null;
        })
      }
    </TableRow>
  );
}

CutoffUnmetRow.propTypes = {
  id: PropTypes.number.isRequired,
  episodeFileId: PropTypes.number,
  series: PropTypes.object.isRequired,
  seasonNumber: PropTypes.number.isRequired,
  absoluteEpisodeNumber: PropTypes.number,
  sceneSeasonNumber: PropTypes.number,
  sceneEpisodeNumber: PropTypes.number,
  sceneAbsoluteEpisodeNumber: PropTypes.number,
  actors: PropTypes.arrayOf(PropTypes.object),
  unverifiedSceneNumbering: PropTypes.bool.isRequired,
  releaseDate: PropTypes.string.isRequired,
  title: PropTypes.string.isRequired,
  isSelected: PropTypes.bool,
  columns: PropTypes.arrayOf(PropTypes.object).isRequired,
  onSelectedChange: PropTypes.func.isRequired
};

CutoffUnmetRow.defaultProps = {
  actors: []
};

export default CutoffUnmetRow;
