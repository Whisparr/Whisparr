import React from 'react';
import ProgressBar from 'Components/ProgressBar';
import { kinds, sizes } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import styles from './MovieIndexProgressBar.css';

interface MovieIndexProgressBarProps {
  movieId: number;
  monitored: boolean;
  status: string;
  width: number;
  detailedProgressBar: boolean;
  bottomRadius?: boolean;
  isStandAlone?: boolean;
}

function MovieIndexProgressBar(props: MovieIndexProgressBarProps) {
  const { status, width, detailedProgressBar, bottomRadius, isStandAlone } =
    props;

  const progress = 100;
  const queueStatusText = null;
  let movieStatus = status === 'released' ? 'downloaded' : status;

  if (movieStatus === 'deleted') {
    movieStatus = 'Missing';
  } else {
    movieStatus = 'NotAvailable';
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
      kind={kinds.SUCCESS}
      size={detailedProgressBar ? sizes.MEDIUM : sizes.SMALL}
      showText={detailedProgressBar}
      width={width}
      text={queueStatusText ? queueStatusText : translate(movieStatus)}
    />
  );
}

export default MovieIndexProgressBar;
