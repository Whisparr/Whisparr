import React from 'react';
import Icon from 'Components/Icon';
import RottenTomatoRating from 'Components/RottenTomatoRating';
import TmdbRating from 'Components/TmdbRating';
import { icons } from 'Helpers/Props';
import Language from 'Language/Language';
import { Ratings } from 'Movie/Movie';
import QualityProfile from 'typings/QualityProfile';
import formatDateTime from 'Utilities/Date/formatDateTime';
import getRelativeDate from 'Utilities/Date/getRelativeDate';
import formatBytes from 'Utilities/Number/formatBytes';
import translate from 'Utilities/String/translate';
import styles from './SceneIndexPosterInfo.css';

interface SceneIndexPosterInfoProps {
  studio?: string;
  showQualityProfile: boolean;
  qualityProfile?: QualityProfile;
  added?: string;
  year: number;
  releaseDate?: string;
  path: string;
  ratings: Ratings;
  originalLanguage: Language;
  sizeOnDisk?: number;
  sortKey: string;
  showRelativeDates: boolean;
  showReleaseDate: boolean;
  shortDateFormat: string;
  longDateFormat: string;
  timeFormat: string;
  showTmdbRating: boolean;
  showRottenTomatoesRating: boolean;
}

function SceneIndexPosterInfo(props: SceneIndexPosterInfoProps) {
  const {
    studio,
    showQualityProfile,
    qualityProfile,
    added,
    year,
    releaseDate,
    path,
    ratings,
    originalLanguage,
    sizeOnDisk,
    sortKey,
    showRelativeDates,
    showReleaseDate,
    shortDateFormat,
    longDateFormat,
    timeFormat,
    showTmdbRating,
    showRottenTomatoesRating,
  } = props;

  if (sortKey === 'studio' && studio) {
    return (
      <div className={styles.info} title={translate('Studio')}>
        {studio}
      </div>
    );
  }

  if (
    sortKey === 'qualityProfileId' &&
    !showQualityProfile &&
    !!qualityProfile?.name
  ) {
    return (
      <div className={styles.info} title={translate('QualityProfile')}>
        {qualityProfile.name}
      </div>
    );
  }

  if (sortKey === 'added' && added) {
    const addedDate = getRelativeDate(
      added,
      shortDateFormat,
      showRelativeDates,
      {
        timeFormat,
        timeForToday: false,
      }
    );

    return (
      <div
        className={styles.info}
        title={formatDateTime(added, longDateFormat, timeFormat)}
      >
        {translate('Added')}: {addedDate}
      </div>
    );
  }

  if (sortKey === 'year' && year) {
    return (
      <div className={styles.info} title={translate('Year')}>
        {year}
      </div>
    );
  }

  if (sortKey === 'releaseDate' && releaseDate && !showReleaseDate) {
    const digitalReleaseDate = getRelativeDate(
      releaseDate,
      shortDateFormat,
      showRelativeDates,
      {
        timeFormat,
        timeForToday: false,
      }
    );

    return (
      <div className={styles.info}>
        <Icon name={icons.DISC} /> {digitalReleaseDate}
      </div>
    );
  }

  if (!showTmdbRating && sortKey === 'tmdbRating' && !!ratings.tmdb) {
    return (
      <div className={styles.info}>
        <TmdbRating ratings={ratings} iconSize={12} />
      </div>
    );
  }

  if (
    !showRottenTomatoesRating &&
    sortKey === 'rottenTomatoesRating' &&
    !!ratings.rottenTomatoes
  ) {
    return (
      <div className={styles.info}>
        <RottenTomatoRating ratings={ratings} iconSize={12} />
      </div>
    );
  }

  if (sortKey === 'path') {
    return (
      <div className={styles.info} title={translate('Path')}>
        {path}
      </div>
    );
  }

  if (sortKey === 'sizeOnDisk') {
    return (
      <div className={styles.info} title={translate('SizeOnDisk')}>
        {formatBytes(sizeOnDisk)}
      </div>
    );
  }

  if (sortKey === 'originalLanguage' && originalLanguage) {
    return (
      <div className={styles.info} title={translate('OriginalLanguage')}>
        {originalLanguage.name}
      </div>
    );
  }

  return null;
}

export default SceneIndexPosterInfo;
