import React, { Fragment } from 'react';
import Icon from 'Components/Icon';
import Popover from 'Components/Tooltip/Popover';
import { icons, kinds, tooltipPositions } from 'Helpers/Props';
import { AlternateTitle, SeriesType } from 'Series/Series';
import padNumber from 'Utilities/Number/padNumber';
import filterAlternateTitles from 'Utilities/Series/filterAlternateTitles';
import translate from 'Utilities/String/translate';
import SceneInfo from './SceneInfo';
import styles from './EpisodeNumber.css';

function getWarningMessage(unverifiedSceneNumbering: boolean) {
  const messages = [];

  if (unverifiedSceneNumbering) {
    messages.push(translate('SceneNumberNotVerified'));
  }

  return messages.join('\n');
}

export interface EpisodeNumberProps {
  seasonNumber: number;
  episodeNumber: number;
  releaseDate?: string;
  absoluteEpisodeNumber?: number;
  sceneSeasonNumber?: number;
  sceneEpisodeNumber?: number;
  sceneAbsoluteEpisodeNumber?: number;
  useSceneNumbering?: boolean;
  unverifiedSceneNumbering?: boolean;
  alternateTitles?: AlternateTitle[];
  seriesType?: SeriesType;
  showSeasonNumber?: boolean;
}

function EpisodeNumber(props: EpisodeNumberProps) {
  const {
    seasonNumber,
    episodeNumber,
    releaseDate,
    absoluteEpisodeNumber,
    sceneSeasonNumber,
    sceneEpisodeNumber,
    sceneAbsoluteEpisodeNumber,
    useSceneNumbering = false,
    unverifiedSceneNumbering = false,
    alternateTitles: seriesAlternateTitles = [],
    seriesType,
    showSeasonNumber = false,
  } = props;

  const alternateTitles = filterAlternateTitles(
    seriesAlternateTitles,
    null,
    useSceneNumbering,
    seasonNumber,
    sceneSeasonNumber
  );

  const hasSceneInformation =
    sceneSeasonNumber !== undefined ||
    sceneEpisodeNumber !== undefined ||
    false ||
    !!alternateTitles.length;

  const warningMessage = getWarningMessage(unverifiedSceneNumbering);

  if (releaseDate) {
    return (
      <span>
        <span>{releaseDate}</span>

        {warningMessage ? (
          <Icon
            className={styles.warning}
            name={icons.WARNING}
            kind={kinds.WARNING}
            title={warningMessage}
          />
        ) : null}
      </span>
    );
  }

  return (
    <span>
      {hasSceneInformation ? (
        <Popover
          anchor={
            <span>
              {showSeasonNumber && seasonNumber != null && (
                <Fragment>{seasonNumber}x</Fragment>
              )}

              {showSeasonNumber ? padNumber(episodeNumber, 2) : episodeNumber}

              {false && (
                <span className={styles.absoluteEpisodeNumber}>
                  ({absoluteEpisodeNumber})
                </span>
              )}
            </span>
          }
          title={translate('SceneInformation')}
          body={
            <SceneInfo
              seasonNumber={seasonNumber}
              episodeNumber={episodeNumber}
              sceneSeasonNumber={sceneSeasonNumber}
              sceneEpisodeNumber={sceneEpisodeNumber}
              sceneAbsoluteEpisodeNumber={sceneAbsoluteEpisodeNumber}
              alternateTitles={alternateTitles}
              seriesType={seriesType}
            />
          }
          position={tooltipPositions.RIGHT}
        />
      ) : (
        <span>
          {showSeasonNumber && seasonNumber != null && (
            <Fragment>{seasonNumber}x</Fragment>
          )}

          {showSeasonNumber ? padNumber(episodeNumber, 2) : episodeNumber}

          {false && (
            <span className={styles.absoluteEpisodeNumber}>
              ({absoluteEpisodeNumber})
            </span>
          )}
        </span>
      )}

      {warningMessage ? (
        <Icon
          className={styles.warning}
          name={icons.WARNING}
          kind={kinds.WARNING}
          title={warningMessage}
        />
      ) : null}
    </span>
  );
}

export default EpisodeNumber;
