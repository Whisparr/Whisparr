import classNames from 'classnames';
import React, { useCallback, useState } from 'react';
import Link from 'Components/Link/Link';
import MonitorToggleButton from 'Components/MonitorToggleButton';
import MovieHeadshot from 'Movie/MovieHeadshot';
import Performer from 'Performer/Performer';
import styles from '../MovieCreditPoster.css';

interface Props {
  performer: Performer;
  character?: string;
  posterWidth: number;
  posterHeight: number;
  safeForWorkMode: boolean;
  onTogglePerformerMonitored: (monitored: boolean) => void;
}

function MovieCastPoster({
  performer,
  character,
  posterWidth,
  posterHeight,
  safeForWorkMode,
  onTogglePerformerMonitored,
}: Props) {
  const [hasPosterError, setHasPosterError] = useState(false);

  const onPosterLoad = useCallback(() => {
    setHasPosterError(false);
  }, []);

  const onPosterLoadError = useCallback(() => {
    setHasPosterError(true);
  }, []);

  const elementStyle = {
    width: `${posterWidth}px`,
    height: `${posterHeight}px`,
    borderRadius: '5px',
  } as React.CSSProperties;

  const contentStyle = { width: `${posterWidth}px` } as React.CSSProperties;

  if (!performer?.foreignId) return null;

  const link = `/performer/${performer.foreignId}`;

  return (
    <div className={styles.content} style={contentStyle}>
      <div className={styles.posterContainer}>
        <div className={styles.controls}>
          <MonitorToggleButton
            className={styles.action}
            monitored={performer.monitored}
            size={20}
            onPress={onTogglePerformerMonitored}
          />
        </div>

        <div style={elementStyle}>
          <Link className={styles.link} to={link}>
            <MovieHeadshot
              safeForWorkMode={safeForWorkMode}
              className={styles.poster}
              style={elementStyle}
              images={performer.images}
              size={250}
              lazy={false}
              overflow={true}
              onError={onPosterLoadError}
              onLoad={onPosterLoad}
            />

            {hasPosterError && (
              <div className={styles.overlayTitle}>{performer.fullName}</div>
            )}
          </Link>
        </div>
      </div>

      <div className={classNames(styles.title, 'swiper-no-swiping')}>
        {performer.fullName}
      </div>
      <div className={classNames(styles.title, 'swiper-no-swiping')}>
        {character}
      </div>
    </div>
  );
}

export default MovieCastPoster;
