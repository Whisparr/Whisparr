import React from 'react';
import { useSelector } from 'react-redux';
import Icon from 'Components/Icon';
import { icons } from 'Helpers/Props';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import getRelativeDate from 'Utilities/Date/getRelativeDate';
import translate from 'Utilities/String/translate';
import styles from './SceneReleaseDates.css';

interface SceneReleaseDatesProps {
  releaseDate: string;
}

function SceneReleaseDates(props: SceneReleaseDatesProps) {
  const { releaseDate } = props;

  const { showRelativeDates, shortDateFormat, timeFormat } = useSelector(
    createUISettingsSelector()
  );

  return (
    <div>
      {releaseDate ? (
        <div title={translate('DigitalRelease')}>
          <div className={styles.dateIcon}>
            <Icon name={icons.DISC} />
          </div>
          {getRelativeDate(releaseDate, shortDateFormat, showRelativeDates, {
            timeFormat,
            timeForToday: false,
          })}
        </div>
      ) : null}
    </div>
  );
}

export default SceneReleaseDates;
