import React, { useCallback } from 'react';
import { useDispatch } from 'react-redux';
import Icon from 'Components/Icon';
import MonitorToggleButton from 'Components/MonitorToggleButton';
import VirtualTableRowCell from 'Components/Table/Cells/TableRowCell';
import { icons } from 'Helpers/Props';
import { getSceneStatusDetails } from 'Scene/SceneStatus';
import { toggleMovieMonitored } from 'Store/Actions/movieActions';
import translate from 'Utilities/String/translate';
import styles from './SceneStatusCell.css';

interface SceneStatusCellProps {
  className: string;
  movieId: number;
  monitored: boolean;
  status: string;
  isSelectMode: boolean;
  isSaving: boolean;
  component?: React.ElementType;
}

function SceneStatusCell(props: SceneStatusCellProps) {
  const {
    className,
    movieId,
    monitored,
    status,
    isSelectMode,
    isSaving,
    component: Component = VirtualTableRowCell,
    ...otherProps
  } = props;

  const statusDetails = getSceneStatusDetails(status);

  const dispatch = useDispatch();

  const onMonitoredPress = useCallback(() => {
    dispatch(toggleMovieMonitored({ movieId, monitored: !monitored }));
  }, [movieId, monitored, dispatch]);

  return (
    <Component className={className} {...otherProps}>
      {isSelectMode ? (
        <MonitorToggleButton
          className={styles.statusIcon}
          monitored={monitored}
          isSaving={isSaving}
          onPress={onMonitoredPress}
        />
      ) : (
        <Icon
          className={styles.statusIcon}
          name={monitored ? icons.MONITORED : icons.UNMONITORED}
          title={
            monitored
              ? translate('SceneIsMonitored')
              : translate('SceneIsUnmonitored')
          }
        />
      )}

      <Icon
        className={styles.statusIcon}
        name={statusDetails.icon}
        title={`${statusDetails.title}: ${statusDetails.message}`}
      />
    </Component>
  );
}

export default SceneStatusCell;
