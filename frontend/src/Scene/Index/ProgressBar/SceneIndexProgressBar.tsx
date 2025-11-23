import React from 'react';
import { useSelector } from 'react-redux';
import ProgressBar from 'Components/ProgressBar';
import { sizes } from 'Helpers/Props';
import { MovieFile } from 'MovieFile/MovieFile';
import createSceneQueueItemsDetailsSelector, {
  SceneQueueDetails,
} from 'Scene/Index/createSceneQueueDetailsSelector';
import getStatusStyle from 'Utilities/Movie/getStatusStyle';
import translate from 'Utilities/String/translate';
import styles from './SceneIndexProgressBar.css';

interface SceneIndexProgressBarProps {
  sceneId: number;
  sceneFile: MovieFile;
  monitored: boolean;
  status: string;
  hasFile: boolean;
  isAvailable: boolean;
  width: number;
  detailedProgressBar: boolean;
  bottomRadius?: boolean;
  isStandAlone?: boolean;
}

function SceneIndexProgressBar(props: SceneIndexProgressBarProps) {
  const {
    sceneId,
    sceneFile,
    monitored,
    status,
    hasFile,
    isAvailable,
    width,
    detailedProgressBar,
    bottomRadius,
    isStandAlone,
  } = props;

  const queueDetails: SceneQueueDetails = useSelector(
    createSceneQueueItemsDetailsSelector(sceneId)
  );

  const progress = 100;
  const queueStatusText =
    queueDetails.count > 0 ? translate('Downloading') : null;
  let sceneStatus = status === 'released' && hasFile ? 'downloaded' : status;

  if (sceneStatus === 'deleted') {
    sceneStatus = translate('Missing');

    if (hasFile) {
      sceneStatus = sceneFile?.quality?.quality.name ?? translate('Downloaded');
    }
  } else if (hasFile) {
    sceneStatus = sceneFile?.quality?.quality.name ?? translate('Downloaded');
  } else if (isAvailable && !hasFile) {
    sceneStatus = translate('Missing');
  } else {
    sceneStatus = translate('NotAvailable');
  }

  const attachedClassName = bottomRadius
    ? styles.progressRadius
    : styles.progress;
  const containerClassName = isStandAlone ? undefined : attachedClassName;

  return (
    <ProgressBar
      className={styles.progressBar}
      containerClassName={containerClassName}
      progress={progress}
      kind={getStatusStyle(
        status,
        monitored,
        hasFile,
        isAvailable,
        'kinds',
        queueDetails.count > 0
      )}
      size={detailedProgressBar ? sizes.MEDIUM : sizes.SMALL}
      showText={detailedProgressBar}
      width={width}
      text={queueStatusText ? queueStatusText : sceneStatus}
    />
  );
}

export default SceneIndexProgressBar;
