import React, { useCallback, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import Icon from 'Components/Icon';
import Label from 'Components/Label';
import Link from 'Components/Link/Link';
import MonitorToggleButton from 'Components/MonitorToggleButton';
import Popover from 'Components/Tooltip/Popover';
import { icons } from 'Helpers/Props';
import { toggleStudioMonitored } from 'Store/Actions/studioActions';
import StudioDetailsLinks from 'Studio/Details/StudioDetailsLinks';
import StudioLogo from 'Studio/StudioLogo';
import translate from 'Utilities/String/translate';
import createStudioIndexItemSelector from '../createStudioIndexItemSelector';
import selectPosterOptions from './selectPosterOptions';
import styles from './StudioIndexPoster.css';
import EditStudioModalConnector from 'Studio/Edit/EditStudioModalConnector';
import IconButton from 'Components/Link/IconButton';

interface StudioIndexPosterProps {
  studioId: number;
  sortKey: string;
  isSelectMode: boolean;
  posterWidth: number;
  posterHeight: number;
}

function StudioIndexPoster(props: StudioIndexPosterProps) {
  const { studioId, posterWidth, posterHeight } = props;

  const { studio } = useSelector(createStudioIndexItemSelector(studioId));

  const safeForWorkMode = useSelector(
    (state: AppState) => state.settings.safeForWorkMode
  );

  const { showName } = useSelector(selectPosterOptions);
  const [isEditStudioModalOpen, setIsEditStudioModalOpen] = useState(false);

  const { title, monitored, images, foreignId } = studio;

  const dispatch = useDispatch();

  const onMonitoredPress = useCallback(() => {
    dispatch(toggleStudioMonitored({ studioId, monitored: !monitored }));
  }, [studioId, monitored, dispatch]);

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
    'object-fit': 'contain',
    'padding-right': '15px',
    'padding-left': '5px',
  };

  return (
    <div className={styles.content}>
      <div className={styles.posterContainer} title={title}>
        <MonitorToggleButton
          className={styles.monitorToggleButton}
          monitored={monitored}
          size={20}
          onPress={onMonitoredPress}
        />

        <Label className={styles.controls}>
          <IconButton
            name={icons.EDIT}
            title={translate('EditPerformer')}
            onPress={onEditStudioPress}
          />

          <span className={styles.externalLinks}>
            <Popover
              anchor={<Icon name={icons.EXTERNAL_LINK} size={12} />}
              title={translate('Links')}
              body={<StudioDetailsLinks foreignId={foreignId} />}
            />
          </span>
        </Label>

        <Link className={styles.link} style={elementStyle} to={link}>
          <StudioLogo
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
            <div className={styles.overlayTitle}>{title}</div>
          ) : null}
        </Link>
      </div>

      {showName ? (
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
