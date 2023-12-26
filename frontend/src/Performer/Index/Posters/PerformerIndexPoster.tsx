import React, { useCallback, useState } from 'react';
import { useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import Icon from 'Components/Icon';
import Label from 'Components/Label';
import Link from 'Components/Link/Link';
import Popover from 'Components/Tooltip/Popover';
import { icons } from 'Helpers/Props';
import MovieHeadshot from 'Movie/MovieHeadshot';
import PerformerDetailsLinks from 'Performer/Details/PerformerDetailsLinks';
import translate from 'Utilities/String/translate';
import createPerformerIndexItemSelector from '../createPerformerIndexItemSelector';
import selectPosterOptions from './selectPosterOptions';
import styles from './PerformerIndexPoster.css';

interface PerformerIndexPosterProps {
  performerId: number;
  sortKey: string;
  isSelectMode: boolean;
  posterWidth: number;
  posterHeight: number;
}

function PerformerIndexPoster(props: PerformerIndexPosterProps) {
  const { performerId, posterWidth, posterHeight } = props;

  const { performer } = useSelector(
    createPerformerIndexItemSelector(performerId)
  );

  const safeForWorkMode = useSelector(
    (state: AppState) => state.settings.safeForWorkMode
  );

  const { showName } = useSelector(selectPosterOptions);

  const { name, images, foreignId } = performer;

  const [hasPosterError, setHasPosterError] = useState(false);

  const onPosterLoadError = useCallback(() => {
    setHasPosterError(true);
  }, [setHasPosterError]);

  const onPosterLoad = useCallback(() => {
    setHasPosterError(false);
  }, [setHasPosterError]);

  const link = `/performer/${foreignId}`;

  const elementStyle = {
    width: `${posterWidth}px`,
    height: `${posterHeight}px`,
  };

  return (
    <div className={styles.content}>
      <div className={styles.posterContainer} title={name}>
        <Label className={styles.controls}>
          <span className={styles.externalLinks}>
            <Popover
              anchor={<Icon name={icons.EXTERNAL_LINK} size={12} />}
              title={translate('Links')}
              body={<PerformerDetailsLinks foreignId={foreignId} />}
            />
          </span>
        </Label>

        <Link className={styles.link} style={elementStyle} to={link}>
          <MovieHeadshot
            blur={safeForWorkMode}
            style={elementStyle}
            images={images}
            size={250}
            lazy={false}
            overflow={true}
            onError={onPosterLoadError}
            onLoad={onPosterLoad}
          />

          {hasPosterError ? (
            <div className={styles.overlayTitle}>{name}</div>
          ) : null}
        </Link>
      </div>

      {showName ? (
        <div className={styles.title} title={name}>
          {name}
        </div>
      ) : null}
    </div>
  );
}

export default PerformerIndexPoster;
