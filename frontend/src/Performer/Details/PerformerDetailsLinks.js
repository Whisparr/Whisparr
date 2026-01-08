import PropTypes from 'prop-types';
import React from 'react';
import Label from 'Components/Label';
import Link from 'Components/Link/Link';
import { kinds, sizes } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import styles from './PerformerDetailsLinks.css';

function PerformerDetailsLinks(props) {
  const { tmdbId, tpdbId, foreignId } = props;

  return (
    <div className={styles.links}>
      <Link
        className={styles.link}
        to={`https://stashdb.org/performers/${foreignId}`}
      >
        <Label
          className={styles.linkLabel}
          kind={kinds.INFO}
          size={sizes.LARGE}
        >
          {translate('StashDB')}
        </Label>
      </Link>

      {!!tmdbId && (
        <Link
          className={styles.link}
          to={`https://www.themoviedb.org/person/${tmdbId}`}
        >
          <Label
            className={styles.linkLabel}
            kind={kinds.INFO}
            size={sizes.LARGE}
          >
            {translate('TMDb')}
          </Label>
        </Link>
      )}

      {!!tpdbId && (
        <Link
          className={styles.link}
          to={`https://theporndb.net/performers/${tpdbId}`}
        >
          <Label
            className={styles.linkLabel}
            kind={kinds.INFO}
            size={sizes.LARGE}
          >
            {translate('TPDb')}
          </Label>
        </Link>
      )}
    </div>
  );
}

PerformerDetailsLinks.propTypes = {
  foreignId: PropTypes.string.isRequired,
  tpdbId: PropTypes.string,
  tmdbId: PropTypes.number
};

export default PerformerDetailsLinks;
