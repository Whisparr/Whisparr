import React, { useCallback, useState } from 'react';
import { useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import Icon from 'Components/Icon';
import Label from 'Components/Label';
import IconButton from 'Components/Link/IconButton';
import Link from 'Components/Link/Link';
import Popover from 'Components/Tooltip/Popover';
import { icons } from 'Helpers/Props';
import MovieIndexPosterSelect from 'Movie/Index/Select/MovieIndexPosterSelect';
import StudioDetailsLinks from 'Studio/Details/StudioDetailsLinks';
import EditStudioModalConnector from 'Studio/Edit/EditStudioModalConnector';
import StudioLogo from 'Studio/StudioLogo';
import translate from 'Utilities/String/translate';
import createStudioIndexItemSelector from '../createStudioIndexItemSelector';
import selectPosterOptions from './selectPosterOptions';
import styles from './StudioIndexPoster.css';

interface StudioIndexPosterProps {
  studioId: number;
  sortKey: string;
  isSelectMode: boolean;
  posterWidth: number;
  posterHeight: number;
}

function StudioIndexPoster(props: StudioIndexPosterProps) {
  const safeForWorkMode = useSelector(
    (state: AppState) => state.settings.safeForWorkMode
  );

  const { studioId, isSelectMode, posterWidth, posterHeight } = props;

  const { studio } = useSelector(createStudioIndexItemSelector(studioId));

  const { showTitle } = useSelector(selectPosterOptions);
  const [isEditStudioModalOpen, setIsEditStudioModalOpen] = useState(false);

  const { title, images, foreignId, website, tmdbId, tpdbId } = studio;

  const [hasPosterError, setHasPosterError] = useState(false);

  const onPosterLoadError = useCallback(() => {
    setHasPosterError(true);
  }, [setHasPosterError]);

  const onPosterLoad = useCallback(() => {
    setHasPosterError(false);
  }, [setHasPosterError]);

  const onEditStudioPress = useCallback(() => {
    setIsEditStudioModalOpen(true);
  }, [setIsEditStudioModalOpen]);

  const onEditStudioModalClose = useCallback(() => {
    setIsEditStudioModalOpen(false);
  }, [setIsEditStudioModalOpen]);

  const link = `/studio/${foreignId}`;

  const elementStyle = {
    width: `${posterWidth}px`,
    height: `${posterHeight}px`,
  };

  return (
    <div className={styles.content}>
      <div className={styles.posterContainer} title={title}>
        {isSelectMode ? <MovieIndexPosterSelect movieId={studioId} /> : null}

        <Label className={styles.controls}>
          <IconButton
            name={icons.EDIT}
            title={translate('EditStudio')}
            onPress={onEditStudioPress}
          />

          <span className={styles.externalLinks}>
            <Popover
              anchor={<Icon name={icons.EXTERNAL_LINK} size={12} />}
              title={translate('Links')}
              body={
                <StudioDetailsLinks
                  website={website}
                  foreignId={foreignId}
                  tmdbId={tmdbId}
                  tpdbId={tpdbId}
                />
              }
            />
          </span>
        </Label>

        <Link className={styles.link} style={elementStyle} to={link}>
          <StudioLogo
            safeForWorkMode={safeForWorkMode}
            style={elementStyle}
            images={images}
            size={250}
            lazy={true}
            onPosterLoad={onPosterLoad}
            onPosterLoadError={onPosterLoadError}
          />

          {hasPosterError ? (
            <div className={styles.overlayTitle}>{title}</div>
          ) : null}
        </Link>
      </div>

      {showTitle ? (
        <div className={styles.title} title={title}>
          {title}
        </div>
      ) : null}

      <EditStudioModalConnector
        isOpen={isEditStudioModalOpen}
        studioId={studioId}
        onModalClose={onEditStudioModalClose}
      />
    </div>
  );
}

export default StudioIndexPoster;
