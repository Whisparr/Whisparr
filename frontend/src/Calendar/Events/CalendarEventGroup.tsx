import classNames from 'classnames';
import moment from 'moment';
import React, { useCallback, useMemo, useState } from 'react';
import { useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';
import getStatusStyle from 'Calendar/getStatusStyle';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import { icons } from 'Helpers/Props';
import useSeries from 'Series/useSeries';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import { CalendarItem } from 'typings/Calendar';
import translate from 'Utilities/String/translate';
import CalendarEvent from './CalendarEvent';
import styles from './CalendarEventGroup.css';

function createIsDownloadingSelector(episodeIds: number[]) {
  return createSelector(
    (state: AppState) => state.queue.details,
    (details) => {
      return details.items.some((item) => {
        return !!(item.episodeId && episodeIds.includes(item.episodeId));
      });
    }
  );
}

interface CalendarEventGroupProps {
  episodeIds: number[];
  seriesId: number;
  events: CalendarItem[];
  onEventModalOpenToggle: (isOpen: boolean) => void;
}

function CalendarEventGroup({
  episodeIds,
  seriesId,
  events,
  onEventModalOpenToggle,
}: CalendarEventGroupProps) {
  const isDownloading = useSelector(createIsDownloadingSelector(episodeIds));
  const series = useSeries(seriesId)!;

  const { enableColorImpairedMode } = useSelector(createUISettingsSelector());

  const { showEpisodeInformation, fullColorEvents } = useSelector(
    (state: AppState) => state.calendar.options
  );

  const [isExpanded, setIsExpanded] = useState(false);

  const firstEpisode = events[0];
  const lastEpisode = events[events.length - 1];
  const releaseDate = firstEpisode.releaseDate;
  const startTime = moment(releaseDate);
  const endTime = moment(lastEpisode.releaseDate).add(
    series.runtime,
    'minutes'
  );
  const seasonNumber = firstEpisode.seasonNumber;

  const { allDownloaded, anyQueued, anyMonitored } = useMemo(() => {
    let files = 0;
    let queued = 0;
    let monitored = 0;

    events.forEach((event) => {
      if (event.episodeFileId) {
        files++;
      }

      if (event.queued) {
        queued++;
      }

      if (series.monitored && event.monitored) {
        monitored++;
      }
    });

    return {
      allDownloaded: files === events.length,
      anyQueued: queued > 0,
      anyMonitored: monitored > 0,
    };
  }, [series, events]);

  const anyDownloading = isDownloading || anyQueued;

  const statusStyle = getStatusStyle(
    allDownloaded,
    anyDownloading,
    startTime,
    endTime,
    anyMonitored
  );

  const handleExpandPress = useCallback(() => {
    setIsExpanded((state) => !state);
  }, []);

  if (isExpanded) {
    return (
      <div>
        {events.map((event) => {
          return (
            <CalendarEvent
              key={event.id}
              episodeId={event.id}
              {...event}
              series={series}
              onEventModalOpenToggle={onEventModalOpenToggle}
            />
          );
        })}

        <Link
          className={styles.collapseContainer}
          component="div"
          onPress={handleExpandPress}
        >
          <Icon name={icons.COLLAPSE} />
        </Link>
      </div>
    );
  }

  return (
    <div
      className={classNames(
        styles.eventGroup,
        styles[statusStyle],
        enableColorImpairedMode && 'colorImpaired',
        fullColorEvents && 'fullColor'
      )}
    >
      <div className={styles.info}>
        <div className={styles.seriesTitle}>{series.title}</div>

        {anyDownloading ? (
          <Icon
            containerClassName={styles.statusIcon}
            name={icons.DOWNLOADING}
            title={translate('AnEpisodeIsDownloading')}
          />
        ) : null}
      </div>

      <div className={styles.airingInfo}>
        {showEpisodeInformation ? (
          <div className={styles.episodeInfo}>{seasonNumber}</div>
        ) : (
          <Link
            className={styles.expandContainerInline}
            component="div"
            onPress={handleExpandPress}
          >
            <Icon name={icons.EXPAND} />
          </Link>
        )}
      </div>

      {showEpisodeInformation ? (
        <Link
          className={styles.expandContainer}
          component="div"
          onPress={handleExpandPress}
        >
          &nbsp;
          <Icon name={icons.EXPAND} />
          &nbsp;
        </Link>
      ) : null}
    </div>
  );
}

export default CalendarEventGroup;
