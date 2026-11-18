import classNames from 'classnames';
import moment from 'moment';
import React, { useCallback, useState } from 'react';
import { useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import CalendarEventQueueDetails from 'Calendar/Events/CalendarEventQueueDetails';
import getStatusStyle from 'Calendar/getStatusStyle';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import EpisodeDetailsModal from 'Episode/EpisodeDetailsModal';
import episodeEntities from 'Episode/episodeEntities';
import useEpisodeFile from 'EpisodeFile/useEpisodeFile';
import { icons, kinds } from 'Helpers/Props';
import useSeries from 'Series/useSeries';
import { createQueueItemSelectorForHook } from 'Store/Selectors/createQueueItemSelector';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import formatTime from 'Utilities/Date/formatTime';
import translate from 'Utilities/String/translate';
import styles from './AgendaEvent.css';

interface AgendaEventProps {
  id: number;
  seriesId: number;
  episodeFileId: number;
  title: string;
  seasonNumber: number;
  releaseDate: string;
  monitored: boolean;
  unverifiedSceneNumbering?: boolean;
  hasFile: boolean;
  grabbed?: boolean;
  showDate: boolean;
}

function AgendaEvent(props: AgendaEventProps) {
  const {
    id,
    seriesId,
    episodeFileId,
    title,
    seasonNumber,
    releaseDate,
    monitored,
    unverifiedSceneNumbering,
    hasFile,
    grabbed,
    showDate,
  } = props;

  const series = useSeries(seriesId)!;
  const episodeFile = useEpisodeFile(episodeFileId);
  const queueItem = useSelector(createQueueItemSelectorForHook(id));
  const { timeFormat, longDateFormat, enableColorImpairedMode } = useSelector(
    createUISettingsSelector()
  );

  const { showEpisodeInformation, showCutoffUnmetIcon } = useSelector(
    (state: AppState) => state.calendar.options
  );

  const [isDetailsModalOpen, setIsDetailsModalOpen] = useState(false);

  const startTime = moment(releaseDate);
  const endTime = moment(releaseDate).add(series.runtime, 'minutes');
  const downloading = !!(queueItem || grabbed);
  const isMonitored = series.monitored && monitored;
  const statusStyle = getStatusStyle(
    hasFile,
    downloading,
    startTime,
    endTime,
    isMonitored
  );

  const handlePress = useCallback(() => {
    setIsDetailsModalOpen(true);
  }, []);

  const handleDetailsModalClose = useCallback(() => {
    setIsDetailsModalOpen(false);
  }, []);

  return (
    <div className={styles.event}>
      <Link className={styles.underlay} onPress={handlePress} />

      <div className={styles.overlay}>
        <div className={styles.date}>
          {showDate ? startTime.format(longDateFormat) : null}
        </div>

        <div
          className={classNames(
            styles.eventWrapper,
            styles[statusStyle],
            enableColorImpairedMode && 'colorImpaired'
          )}
        >
          <div className={styles.time}>
            {formatTime(releaseDate, timeFormat)} -{' '}
            {formatTime(endTime.toISOString(), timeFormat, {
              includeMinuteZero: true,
            })}
          </div>

          <div className={styles.seriesTitle}>{series.title}</div>

          {showEpisodeInformation ? (
            <div className={styles.seasonEpisodeNumber}>
              {seasonNumber}x{releaseDate}
              <div className={styles.episodeSeparator}> - </div>
            </div>
          ) : null}

          <div className={styles.episodeTitle}>
            {showEpisodeInformation ? title : null}
          </div>

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
              kind={kinds.WARNING}
              title={translate('QualityCutoffNotMet')}
            />
          ) : null}
        </div>
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

export default AgendaEvent;
