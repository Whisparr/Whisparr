import PropTypes from 'prop-types';
import React, { useCallback } from 'react';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import { icons } from 'Helpers/Props';
import ImportMovieTitle from './ImportMovieTitle';
import styles from './ImportMovieSearchResult.css';

function ImportMovieSearchResult(props) {
  const {
    tmdbId,
    title,
    year,
    network,
    isExistingMovie,
    onPress
  } = props;

  const onPressCallback = useCallback(() => onPress(tmdbId), [tmdbId, onPress]);

  return (
    <div className={styles.container}>
      <Link
        className={styles.movie}
        onPress={onPressCallback}
      >
        <ImportMovieTitle
          title={title}
          year={year}
          network={network}
          isExistingMovie={isExistingMovie}
        />
      </Link>

      <Link
        className={styles.tmdbLink}
        to={`https://www.themoviedb.org/movie/${tmdbId}`}
      >
        <Icon
          className={styles.tmdbLinkIcon}
          name={icons.EXTERNAL_LINK}
          size={16}
        />
      </Link>
    </div>
  );
}

ImportMovieSearchResult.propTypes = {
  tmdbId: PropTypes.number.isRequired,
  title: PropTypes.string.isRequired,
  year: PropTypes.number.isRequired,
  network: PropTypes.string,
  isExistingMovie: PropTypes.bool.isRequired,
  onPress: PropTypes.func.isRequired
};

export default ImportMovieSearchResult;
