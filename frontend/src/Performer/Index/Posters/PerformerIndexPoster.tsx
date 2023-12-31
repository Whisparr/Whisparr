import React, { useCallback, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import Icon from 'Components/Icon';
import Label from 'Components/Label';
import IconButton from 'Components/Link/IconButton';
import Link from 'Components/Link/Link';
import MonitorToggleButton from 'Components/MonitorToggleButton';
import Popover from 'Components/Tooltip/Popover';
import { icons } from 'Helpers/Props';
import MovieHeadshot from 'Movie/MovieHeadshot';
import PerformerDetailsLinks from 'Performer/Details/PerformerDetailsLinks';
import EditPerformerModalConnector from 'Performer/Edit/EditPerformerModalConnector';
import { togglePerformerMonitored } from 'Store/Actions/performerActions';
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

  const { name, monitored, images, foreignId } = performer;

  const dispatch = useDispatch();

  const onMonitoredPress = useCallback(() => {
    dispatch(togglePerformerMonitored({ performerId, monitored: !monitored }));
  }, [performerId, monitored, dispatch]);

  const safeForWorkMode = useSelector(
    (state: AppState) => state.settings.safeForWorkMode
  );

  const { showName } = useSelector(selectPosterOptions);
  const [isEditPerformerModalOpen, setIsEditPerformerModalOpen] =
    useState(false);

  const [hasPosterError, setHasPosterError] = useState(false);

  const onPosterLoadError = useCallback(() => {
    setHasPosterError(true);
  }, [setHasPosterError]);

  const onPosterLoad = useCallback(() => {
    setHasPosterError(false);
  }, [setHasPosterError]);

  const onEditPerformerPress = useCallback(() => {
    setIsEditPerformerModalOpen(true);
  }, [setIsEditPerformerModalOpen]);

  const onEditPerformerModalClose = useCallback(() => {
    setIsEditPerformerModalOpen(false);
  }, [setIsEditPerformerModalOpen]);

  const link = `/performer/${foreignId}`;

  const elementStyle = {
    width: `${posterWidth}px`,
    height: `${posterHeight}px`,
  };

  return (
    <div className={styles.content}>
      <div className={styles.posterContainer} title={name}>
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
            onPress={onEditPerformerPress}
          />

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

      <EditPerformerModalConnector
        isOpen={isEditPerformerModalOpen}
        performerId={performerId}
        onModalClose={onEditPerformerModalClose}
      />
    </div>
  );
}

export default PerformerIndexPoster;
