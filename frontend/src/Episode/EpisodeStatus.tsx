import React from 'react';
import { useSelector } from 'react-redux';
import QueueDetails from 'Activity/Queue/QueueDetails';
import Icon from 'Components/Icon';
import ProgressBar from 'Components/ProgressBar';
import StatusIndicator from 'Components/StatusIndicator';
import Episode from 'Episode/Episode';
import useEpisode, { EpisodeEntity } from 'Episode/useEpisode';
import useEpisodeFile from 'EpisodeFile/useEpisodeFile';
import { icons, kinds, sizes } from 'Helpers/Props';
import { createQueueItemSelectorForHook } from 'Store/Selectors/createQueueItemSelector';
import isBefore from 'Utilities/Date/isBefore';
import translate from 'Utilities/String/translate';
import EpisodeQuality from './EpisodeQuality';
import styles from './EpisodeStatus.css';

interface EpisodeStatusProps {
  episodeId: number;
  episodeEntity?: EpisodeEntity;
  episodeFileId: number | undefined;
}

function EpisodeStatus({
  episodeId,
  episodeEntity = 'episodes',
  episodeFileId,
}: EpisodeStatusProps) {
  const {
    releaseDate,
    monitored,
    grabbed = false,
  } = useEpisode(episodeId, episodeEntity) as Episode;

  const queueItem = useSelector(createQueueItemSelectorForHook(episodeId));
  const episodeFile = useEpisodeFile(episodeFileId);

  const hasEpisodeFile = !!episodeFile;
  const isQueued = !!queueItem;
  const hasAired = isBefore(releaseDate);

  if (isQueued) {
    const { sizeleft, size } = queueItem;

    const progress = size ? 100 - (sizeleft / size) * 100 : 0;

    return (
      <StatusIndicator
        className={styles.center}
        label={translate('EpisodeIsDownloading')}
      >
        <QueueDetails
          {...queueItem}
          progressBar={
            <ProgressBar
              progress={progress}
              kind={kinds.PURPLE}
              size={sizes.MEDIUM}
            />
          }
        />
      </StatusIndicator>
    );
  }

  if (grabbed) {
    const label = translate('EpisodeIsDownloading');

    return (
      <StatusIndicator className={styles.center} label={label} title={label}>
        <Icon name={icons.DOWNLOADING} />
      </StatusIndicator>
    );
  }

  if (hasEpisodeFile) {
    const quality = episodeFile.quality;
    const isCutoffNotMet = episodeFile.qualityCutoffNotMet;
    const label = translate('EpisodeDownloaded');

    return (
      <StatusIndicator className={styles.center} label={label}>
        <EpisodeQuality
          quality={quality}
          size={episodeFile.size}
          isCutoffNotMet={isCutoffNotMet}
          title={label}
        />
      </StatusIndicator>
    );
  }

  if (!releaseDate) {
    const label = translate('Tba');

    return (
      <StatusIndicator className={styles.center} label={label} title={label}>
        <Icon name={icons.TBA} />
      </StatusIndicator>
    );
  }

  if (!monitored) {
    const label = translate('EpisodeIsNotMonitored');

    return (
      <StatusIndicator className={styles.center} label={label} title={label}>
        <Icon name={icons.UNMONITORED} kind={kinds.DISABLED} />
      </StatusIndicator>
    );
  }

  if (hasAired) {
    const label = translate('EpisodeMissingFromDisk');

    return (
      <StatusIndicator className={styles.center} label={label} title={label}>
        <Icon name={icons.MISSING} />
      </StatusIndicator>
    );
  }

  const label = translate('EpisodeHasNotAired');

  return (
    <StatusIndicator className={styles.center} label={label} title={label}>
      <Icon name={icons.NOT_AIRED} />
    </StatusIndicator>
  );
}

export default EpisodeStatus;
