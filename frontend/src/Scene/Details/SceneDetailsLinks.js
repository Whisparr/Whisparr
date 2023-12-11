import PropTypes from 'prop-types';
import React from 'react';
import Label from 'Components/Label';
import Link from 'Components/Link/Link';
import { kinds, sizes } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import styles from './SceneDetailsLinks.css';

function SceneDetailsLinks(props) {
  const {
    tmdbId,
    imdbId
  } = props;

  return (
    <div className={styles.links}>
      <Link
        className={styles.link}
        to={`https://www.thescenedb.org/scene/${tmdbId}`}
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
        to={`https://trakt.tv/search/tmdb/${tmdbId}?id_type=scene`}
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
            to={`https://scenechat.org/${imdbId}/`}
          >
            <Label
              className={styles.linkLabel}
              kind={kinds.INFO}
              size={sizes.LARGE}
            >
              {translate('SceneChat')}
            </Label>
          </Link>
      }
    </div>
  );
}

SceneDetailsLinks.propTypes = {
  tmdbId: PropTypes.number.isRequired,
  imdbId: PropTypes.string
};

export default SceneDetailsLinks;
