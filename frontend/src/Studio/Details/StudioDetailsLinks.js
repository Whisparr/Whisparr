import PropTypes from 'prop-types';
import React from 'react';
import Label from 'Components/Label';
import Link from 'Components/Link/Link';
import { kinds, sizes } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import styles from './StudioDetailsLinks.css';

function StudioDetailsLinks(props) {
  const {
    foreignId,
    website
  } = props;

  return (
    <div className={styles.links}>
      <Link
        className={styles.link}
        to={website}
      >
        <Label
          className={styles.linkLabel}
          kind={kinds.INFO}
          size={sizes.LARGE}
        >
          {translate('Homepage')}
        </Label>
      </Link>
      <Link
        className={styles.link}
        to={`https://stashdb.org/studios/${foreignId}`}
      >
        <Label
          className={styles.linkLabel}
          kind={kinds.INFO}
          size={sizes.LARGE}
        >
          {translate('StashDB')}
        </Label>
      </Link>
    </div>
  );
}

StudioDetailsLinks.propTypes = {
  foreignId: PropTypes.string.isRequired,
  website: PropTypes.string
};

export default StudioDetailsLinks;
