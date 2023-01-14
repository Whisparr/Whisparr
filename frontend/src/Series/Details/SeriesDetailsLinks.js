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
        to={`http://www.thetvdb.com/?tab=series&id=${tvdbId}`}
      >
        <Label
          className={styles.linkLabel}
          kind={kinds.INFO}
          size={sizes.LARGE}
        >
          The TVDB
        </Label>
      </Link>

      <Link
        className={styles.link}
        to={`http://trakt.tv/search/tvdb/${tvdbId}?id_type=show`}
      >
        <Label
          className={styles.linkLabel}
          kind={kinds.INFO}
          size={sizes.LARGE}
        >
          Trakt
        </Label>
      </Link>
    </div>
  );
}

SeriesDetailsLinks.propTypes = {
  tvdbId: PropTypes.number.isRequired
};

export default SeriesDetailsLinks;
