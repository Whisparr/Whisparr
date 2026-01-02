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
  onTogglePerformerMonitored: (
    monitored: boolean,
    moviesMonitored: boolean
  ) => void;
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
  const name = performer.fullName ?? performer.name;
  const isPerformer = !!performer?.foreignId;
  const isPerfotmerLoaded = !!performer?.fullName;
  const link = isPerformer ? `/performer/${performer.foreignId}` : '';
  const title = isPerformer
    ? `${performer.fullName}`
    : 'Create a Link on StashDB to Link this Performer';

  return (
    <div className={styles.content} style={contentStyle}>
      <div className={styles.posterContainer}>
        {isPerfotmerLoaded && (
          <div className={styles.controls}>
            <MonitorToggleButton
              className={styles.action}
              monitored={performer.monitored}
              moviesMonitored={performer.moviesMonitored}
              type="sceneMonitor"
              size={20}
              onPress={onTogglePerformerMonitored}
            />
            <MonitorToggleButton
              className={styles.movieAction}
              monitored={performer.monitored}
              moviesMonitored={performer.moviesMonitored}
              type="movieMonitor"
              size={20}
              onPress={onTogglePerformerMonitored}
            />
          </div>
        )}

        <div style={elementStyle}>
          <Link title={title} className={styles.link} to={link}>
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
              <div className={styles.overlayTitle}>{name}</div>
            )}
          </Link>
        </div>
      </div>

      <div className={classNames(styles.title, 'swiper-no-swiping')}>
        {name}
      </div>
      <div className={classNames(styles.title, 'swiper-no-swiping')}>
        {character}
      </div>
    </div>
  );
}

export default MovieCastPoster;
