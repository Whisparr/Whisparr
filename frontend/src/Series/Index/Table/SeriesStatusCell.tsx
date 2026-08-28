import React, { useCallback } from 'react';
import { useDispatch } from 'react-redux';
import Icon from 'Components/Icon';
import MonitorToggleButton from 'Components/MonitorToggleButton';
import StatusIndicator from 'Components/StatusIndicator';
import VirtualTableRowCell from 'Components/Table/Cells/TableRowCell';
import { icons } from 'Helpers/Props';
import { SeriesStatus } from 'Series/Series';
import { getSeriesStatusDetails } from 'Series/SeriesStatus';
import { toggleSeriesMonitored } from 'Store/Actions/seriesActions';
import translate from 'Utilities/String/translate';
import styles from './SeriesStatusCell.css';

interface SeriesStatusCellProps {
  className: string;
  seriesId: number;
  monitored: boolean;
  status: SeriesStatus;
  isSelectMode: boolean;
  isSaving: boolean;
  component?: React.ElementType;
}

function SeriesStatusCell(props: SeriesStatusCellProps) {
  const {
    className,
    seriesId,
    monitored,
    status,
    isSelectMode,
    isSaving,
    component: Component = VirtualTableRowCell,
    ...otherProps
  } = props;

  const statusDetails = getSeriesStatusDetails(status);
  const dispatch = useDispatch();

  const onMonitoredPress = useCallback(() => {
    dispatch(toggleSeriesMonitored({ seriesId, monitored: !monitored }));
  }, [seriesId, monitored, dispatch]);

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
        <StatusIndicator
          className={styles.statusIcon}
          label={
            monitored
              ? translate('SeriesIsMonitored')
              : translate('SeriesIsUnmonitored')
          }
          title={
            monitored
              ? translate('SiteIsMonitored')
              : translate('SiteIsUnmonitored')
          }
        >
          <Icon name={monitored ? icons.MONITORED : icons.UNMONITORED} />
        </StatusIndicator>
      )}

      <StatusIndicator
        className={styles.statusIcon}
        label={statusDetails.message}
        title={`${statusDetails.title}: ${statusDetails.message}`}
      >
        <Icon name={statusDetails.icon} />
      </StatusIndicator>
    </Component>
  );
}

export default SeriesStatusCell;
