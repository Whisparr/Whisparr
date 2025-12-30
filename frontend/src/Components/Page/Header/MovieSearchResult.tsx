import React from 'react';
import { useSelector } from 'react-redux';
import Icon from 'Components/Icon';
import Label from 'Components/Label';
import { icons, kinds, sizes } from 'Helpers/Props';
import MoviePoster from 'Movie/MoviePoster';
import ScenePoster from 'Scene/ScenePoster';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import formatRuntime from 'Utilities/Date/formatRuntime';
import getRelativeDate from 'Utilities/Date/getRelativeDate';
import firstCharToUpper from 'Utilities/String/firstCharToUpper';
import { SuggestedMovie } from './MovieSearchInput';
import styles from './MovieSearchResult.css';

interface Match {
  key: string;
  refIndex: number;
}

interface MovieSearchResultProps extends SuggestedMovie {
  match: Match;
}

function MovieSearchResult(props: MovieSearchResultProps) {
  const { title, year, images, itemType, studioTitle, runtime, releaseDate } =
    props;

  const { showRelativeDates, shortDateFormat, timeFormat } = useSelector(
    createUISettingsSelector()
  );

  let releaseText = '';

  if (releaseDate) {
    releaseText = getRelativeDate({
      date: releaseDate,
      shortDateFormat,
      showRelativeDates,
      timeFormat,
      timeForToday: false,
    });
  } else if (year > 0) {
    releaseText = `${year}`;
  }

  return (
    <div className={styles.result}>
      {itemType === 'scene' ? (
        <div className={styles.sceneContainer}>
          <ScenePoster
            className={styles.scene}
            images={images}
            size={180}
            lazy={false}
            overflow={true}
            safeForWorkMode={false}
          />
        </div>
      ) : (
        <div className={styles.posterContainer}>
          <MoviePoster
            className={styles.poster}
            images={images}
            size={250}
            lazy={false}
            overflow={true}
          />
        </div>
      )}
      <div className={styles.titles}>
        <div className={styles.title}>{title}</div>

        <div>{releaseText ? `${releaseText}` : ''}</div>

        <div className={styles.metaRow}>
          <div className={styles.itemType}>
            <Label size={sizes.LARGE} kind={kinds.INVERSE} outline={true}>
              {firstCharToUpper(itemType)}
            </Label>
          </div>

          {studioTitle ? (
            <Label size={sizes.LARGE} kind={kinds.INVERSE} outline={true}>
              <Icon name={icons.STUDIO} className={styles.studioIcon} />
              {studioTitle}
            </Label>
          ) : null}

          {runtime ? (
            <div className={styles.runtime}>{formatRuntime(runtime)}</div>
          ) : null}
        </div>
      </div>
    </div>
  );
}

export default MovieSearchResult;
