import PropTypes from 'prop-types';
import React from 'react';
import Label from 'Components/Label';
import { kinds } from 'Helpers/Props';
import styles from './ImportMovieTitle.css';

function ImportMovieTitle(props) {
  const {
    title,
    year,
    network,
    isExistingMovie
  } = props;

  return (
    <div className={styles.titleContainer}>
      <div className={styles.title}>
        {title}
      </div>

      {
        !title.contains(year) &&
        year > 0 ?
          <span className={styles.year}>
            ({year})
          </span> :
          null
      }

      {
        network ?
          <Label>{network}</Label> :
          null
      }

      {
        isExistingMovie ?
          <Label
            kind={kinds.WARNING}
          >
            Existing
          </Label> :
          null
      }
    </div>
  );
}

ImportMovieTitle.propTypes = {
  title: PropTypes.string.isRequired,
  year: PropTypes.number.isRequired,
  network: PropTypes.string,
  isExistingMovie: PropTypes.bool.isRequired
};

export default ImportMovieTitle;
