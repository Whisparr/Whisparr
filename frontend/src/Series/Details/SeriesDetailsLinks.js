import PropTypes from 'prop-types';
import React from 'react';
import Label from 'Components/Label';
import Link from 'Components/Link/Link';
import { kinds, sizes } from 'Helpers/Props';
import styles from './SeriesDetailsLinks.css';

function SeriesDetailsLinks(props) {
  const {
    tvdbId
  } = props;

  return (
    <div className={styles.links}>
      <Link
        className={styles.link}
        to={`https://theporndb.net/sites/${tvdbId}`}
      >
        <Label
          className={styles.linkLabel}
          kind={kinds.INFO}
          size={sizes.LARGE}
        >
          TPDB
        </Label>
      </Link>
    </div>
  );
}

SeriesDetailsLinks.propTypes = {
  tvdbId: PropTypes.number.isRequired
};

export default SeriesDetailsLinks;
