import React from 'react';
import { useSelector } from 'react-redux';
import Icon from 'Components/Icon';
import InlineMarkdown from 'Components/Markdown/InlineMarkdown';
import { icons } from 'Helpers/Props';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import getRelativeDate from 'Utilities/Date/getRelativeDate';
import translate from 'Utilities/String/translate';
import Movie from '../Movie';
import styles from './MovieReleaseDates.css';

type MovieReleaseDatesProps = Pick<
  Movie,
  'foreignId' | 'releaseDate' | 'itemType'
>;

function MovieReleaseDates({
  foreignId,
  itemType,
  releaseDate,
}: MovieReleaseDatesProps) {
  const { showRelativeDates, shortDateFormat, timeFormat } = useSelector(
    createUISettingsSelector()
  );

  let urlFragment = 'https://stashdb.org/scenes/';
  if (itemType === 'movie') {
    urlFragment = foreignId.startsWith('tpdb:')
      ? 'https://theporndb.net/movies/'
      : 'https://www.themoviedb.org/movie/';
  }

  if (!releaseDate) {
    return (
      <div>
        <div className={styles.dateIcon}>
          <Icon name={icons.MISSING} />
        </div>

        <InlineMarkdown
          data={translate('NoMovieReleaseDatesAvailable', {
            url: `${urlFragment}${foreignId}`,
          })}
        />
      </div>
    );
  }

  return (
    <div>
      {releaseDate ? (
        <div title={translate('ReleaseDate')}>
          <div className={styles.dateIcon}>
            <Icon name={icons.CALENDAR} />
          </div>

          {getRelativeDate({
            date: releaseDate,
            shortDateFormat,
            showRelativeDates,
            timeFormat,
            timeForToday: false,
          })}
        </div>
      ) : null}
    </div>
  );
}

export default MovieReleaseDates;
