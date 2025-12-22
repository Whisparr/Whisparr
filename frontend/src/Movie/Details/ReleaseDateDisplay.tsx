import React from 'react';
import { useSelector } from 'react-redux';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import getRelativeDate from 'Utilities/Date/getRelativeDate';
import styles from './MovieDetails.css';

interface Props {
  releaseDate?: string;
}

export default function ReleaseDateDisplay({ releaseDate }: Props) {
  const { showRelativeDates, shortDateFormat, timeFormat } = useSelector(
    createUISettingsSelector()
  );

  if (!releaseDate) {
    return null;
  }

  return (
    <span className={styles.year}>
      {getRelativeDate({
        date: releaseDate,
        shortDateFormat,
        showRelativeDates,
        timeFormat,
        timeForToday: false,
      })}
    </span>
  );
}
