import classNames from 'classnames';
import moment from 'moment';
import React, { useCallback, useState } from 'react';
import { useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import getStatusStyle from 'Calendar/getStatusStyle';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import EpisodeDetailsModal from 'Episode/EpisodeDetailsModal';
import episodeEntities from 'Episode/episodeEntities';
import useEpisodeFile from 'EpisodeFile/useEpisodeFile';
import { icons, kinds } from 'Helpers/Props';
import Series from 'Series/Series';
import { createQueueItemSelectorForHook } from 'Store/Selectors/createQueueItemSelector';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import translate from 'Utilities/String/translate';
import CalendarEventQueueDetails from './CalendarEventQueueDetails';
import styles from './CalendarEvent.css';

interface CalendarEventProps {
  id: number;
  episodeId: number;
  series: Series;
  episodeFileId?: number;
  title: string;
  releaseDate: string;
  monitored: boolean;
  unverifiedSceneNumbering?: boolean;
  hasFile: boolean;
  grabbed?: boolean;
  onEventModalOpenToggle: (isOpen: boolean) => void;
}

function CalendarEvent(props: CalendarEventProps) {
  const {
    id,
    series,
    episodeFileId,
    title,
    releaseDate,
    monitored,
    unverifiedSceneNumbering,
    hasFile,
    grabbed,
    onEventModalOpenToggle,
  } = props;

  const episodeFile = useEpisodeFile(episodeFileId);
  const queueItem = useSelector(createQueueItemSelectorForHook(id));

  const { enableColorImpairedMode } = useSelector(createUISettingsSelector());

  const { showEpisodeInformation, showCutoffUnmetIcon, fullColorEvents } =
    useSelector((state: AppState) => state.calendar.options);

  const [isDetailsModalOpen, setIsDetailsModalOpen] = useState(false);

  const handleDetailsModalClose = useCallback(() => {
    setIsDetailsModalOpen(true);
    onEventModalOpenToggle(true);
  }, [onEventModalOpenToggle]);

  const handlePress = useCallback(() => {
    setIsDetailsModalOpen(false);
    onEventModalOpenToggle(false);
  }, [onEventModalOpenToggle]);

  if (!series) {
    return null;
  }

  const startTime = moment(releaseDate);
  const endTime = moment(releaseDate).add(series.runtime, 'minutes');
  const isDownloading = !!(queueItem || grabbed);
  const isMonitored = series.monitored && monitored;
  const statusStyle = getStatusStyle(
    hasFile,
    isDownloading,
    startTime,
    endTime,
    isMonitored
  );

  return (
    <div
      className={classNames(
        styles.event,
        styles[statusStyle],
        enableColorImpairedMode && 'colorImpaired',
        fullColorEvents && 'fullColor'
      )}
    >
      <Link className={styles.underlay} onPress={handlePress} />

      <div className={styles.overlay}>
        <div className={styles.info}>
          <div className={styles.seriesTitle}>{series.title}</div>

          <div
            className={classNames(
              styles.statusContainer,
              fullColorEvents && 'fullColor'
            )}
          >
            {unverifiedSceneNumbering ? (
              <Icon
                className={styles.statusIcon}
                name={icons.WARNING}
                title={translate('SceneNumberNotVerified')}
              />
            ) : null}

            {queueItem ? (
              <span className={styles.statusIcon}>
                <CalendarEventQueueDetails {...queueItem} />
              </span>
            ) : null}

            {!queueItem && grabbed ? (
              <Icon
                className={styles.statusIcon}
                name={icons.DOWNLOADING}
                title={translate('EpisodeIsDownloading')}
              />
            ) : null}

            {showCutoffUnmetIcon &&
            !!episodeFile &&
            episodeFile.qualityCutoffNotMet ? (
              <Icon
                className={styles.statusIcon}
                name={icons.EPISODE_FILE}
                kind={fullColorEvents ? kinds.DEFAULT : kinds.WARNING}
                title={translate('QualityCutoffNotMet')}
              />
            ) : null}
          </div>
        </div>

        {showEpisodeInformation ? (
          <div className={styles.episodeInfo}>
            <div className={styles.episodeTitle}>{title}</div>

            <div>{releaseDate}</div>
          </div>
        ) : null}
      </div>

      <EpisodeDetailsModal
        isOpen={isDetailsModalOpen}
        episodeId={id}
        episodeEntity={episodeEntities.CALENDAR}
        seriesId={series.id}
        episodeTitle={title}
        showOpenSeriesButton={true}
        onModalClose={handleDetailsModalClose}
      />
    </div>
  );
}

export default CalendarEvent;
