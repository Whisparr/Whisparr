import PropTypes from 'prop-types';
import React, { Fragment } from 'react';
import Icon from 'Components/Icon';
import Popover from 'Components/Tooltip/Popover';
import { icons, kinds, tooltipPositions } from 'Helpers/Props';
import padNumber from 'Utilities/Number/padNumber';
import filterAlternateTitles from 'Utilities/Series/filterAlternateTitles';
import SceneInfo from './SceneInfo';
import styles from './EpisodeNumber.css';

function getWarningMessage(unverifiedSceneNumbering, absoluteEpisodeNumber) {
  const messages = [];

  if (unverifiedSceneNumbering) {
    messages.push('Scene number hasn\'t been verified yet');
  }

  return messages.join('\n');
}

function EpisodeNumber(props) {
  const {
    seasonNumber,
    episodeNumber,
    absoluteEpisodeNumber,
    sceneSeasonNumber,
    sceneEpisodeNumber,
    sceneAbsoluteEpisodeNumber,
    useSceneNumbering,
    unverifiedSceneNumbering,
    alternateTitles: seriesAlternateTitles,
    showSeasonNumber
  } = props;

  const alternateTitles = filterAlternateTitles(seriesAlternateTitles, null, useSceneNumbering, seasonNumber, sceneSeasonNumber);

  const hasSceneInformation = sceneSeasonNumber !== undefined ||
    sceneEpisodeNumber !== undefined ||
    (sceneAbsoluteEpisodeNumber !== undefined) ||
    !!alternateTitles.length;

  const warningMessage = getWarningMessage(unverifiedSceneNumbering, absoluteEpisodeNumber);

  return (
    <span>
      {
        hasSceneInformation ?
          <Popover
            anchor={
              <span>
                {
                  showSeasonNumber && seasonNumber != null &&
                    <Fragment>
                      {seasonNumber}x
                    </Fragment>
                }

                {showSeasonNumber ? padNumber(episodeNumber, 2) : episodeNumber}
              </span>
            }
            title="Scene Information"
            body={
              <SceneInfo
                seasonNumber={seasonNumber}
                episodeNumber={episodeNumber}
                sceneSeasonNumber={sceneSeasonNumber}
                sceneEpisodeNumber={sceneEpisodeNumber}
                sceneAbsoluteEpisodeNumber={sceneAbsoluteEpisodeNumber}
                alternateTitles={alternateTitles}
              />
            }
            position={tooltipPositions.RIGHT}
          /> :
          <span>
            {
              showSeasonNumber && seasonNumber != null &&
                <Fragment>
                  {seasonNumber}x
                </Fragment>
            }

            {showSeasonNumber ? padNumber(episodeNumber, 2) : episodeNumber}
          </span>
      }

      {
        warningMessage ?
          <Icon
            className={styles.warning}
            name={icons.WARNING}
            kind={kinds.WARNING}
            title={warningMessage}
          /> :
          null
      }

    </span>
  );
}

EpisodeNumber.propTypes = {
  seasonNumber: PropTypes.number.isRequired,
  episodeNumber: PropTypes.number.isRequired,
  absoluteEpisodeNumber: PropTypes.number,
  sceneSeasonNumber: PropTypes.number,
  sceneEpisodeNumber: PropTypes.number,
  sceneAbsoluteEpisodeNumber: PropTypes.number,
  useSceneNumbering: PropTypes.bool.isRequired,
  unverifiedSceneNumbering: PropTypes.bool.isRequired,
  alternateTitles: PropTypes.arrayOf(PropTypes.object).isRequired,
  showSeasonNumber: PropTypes.bool.isRequired
};

EpisodeNumber.defaultProps = {
  useSceneNumbering: false,
  unverifiedSceneNumbering: false,
  alternateTitles: [],
  showSeasonNumber: false
};

export default EpisodeNumber;
