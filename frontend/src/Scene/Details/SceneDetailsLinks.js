import PropTypes from 'prop-types';
import React from 'react';
import Label from 'Components/Label';
import Link from 'Components/Link/Link';
import { kinds, sizes } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import styles from './SceneDetailsLinks.css';

function SceneDetailsLinks(props) {
  const {
    foreignId
  } = props;

  return (
    <div className={styles.links}>
      <Link
        className={styles.link}
        to={`https://www.stashdb.org/scenes/${foreignId}`}
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

SceneDetailsLinks.propTypes = {
  foreignId: PropTypes.string.isRequired
};

export default SceneDetailsLinks;
