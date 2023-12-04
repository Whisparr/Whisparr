import PropTypes from 'prop-types';
import React from 'react';
import Label from 'Components/Label';
import Link from 'Components/Link/Link';
import { kinds, sizes } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import styles from './MovieDetailsLinks.css';

function MovieDetailsLinks(props) {
  const {
    tmdbId,
    imdbId
  } = props;

  return (
    <div className={styles.links}>
      <Link
        className={styles.link}
        to={`https://www.themoviedb.org/movie/${tmdbId}`}
      >
        <Label
          className={styles.linkLabel}
          kind={kinds.INFO}
          size={sizes.LARGE}
        >
          {translate('TMDb')}
        </Label>
      </Link>

      <Link
        className={styles.link}
        to={`https://trakt.tv/search/tmdb/${tmdbId}?id_type=movie`}
      >
        <Label
          className={styles.linkLabel}
          kind={kinds.INFO}
          size={sizes.LARGE}
        >
          {translate('Trakt')}
        </Label>
      </Link>

      <Link
        className={styles.link}
        to={`https://letterboxd.com/tmdb/${tmdbId}`}
      >
        <Label
          className={styles.linkLabel}
          kind={kinds.INFO}
          size={sizes.LARGE}
        >
          {translate('Letterboxd')}
        </Label>
      </Link>

      {
        !!imdbId &&
          <Link
            className={styles.link}
            to={`https://imdb.com/title/${imdbId}/`}
          >
            <Label
              className={styles.linkLabel}
              kind={kinds.INFO}
              size={sizes.LARGE}
            >
              {translate('IMDb')}
            </Label>
          </Link>
      }

      {
        !!imdbId &&
          <Link
            className={styles.link}
            to={`https://moviechat.org/${imdbId}/`}
          >
            <Label
              className={styles.linkLabel}
              kind={kinds.INFO}
              size={sizes.LARGE}
            >
              {translate('MovieChat')}
            </Label>
          </Link>
      }
    </div>
  );
}

MovieDetailsLinks.propTypes = {
  tmdbId: PropTypes.number.isRequired,
  imdbId: PropTypes.string
};

export default MovieDetailsLinks;
