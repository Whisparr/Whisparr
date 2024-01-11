import PropTypes from 'prop-types';
import React from 'react';
import Button from 'Components/Link/Button';
import { kinds } from 'Helpers/Props';
import styles from 'Scene/NoScene.css';
import translate from 'Utilities/String/translate';

function NoPerformer(props) {
  const { totalItems } = props;

  if (totalItems > 0) {
    return (
      <div>
        <div className={styles.message}>
          {translate('AllPerformersHiddenDueToFilter')}
        </div>
      </div>
    );
  }

  return (
    <div>
      <div className={styles.message}>
        {translate('NoPerformersExist')}
      </div>

      <div className={styles.buttonContainer}>
        <Button
          to="/add/import"
          kind={kinds.PRIMARY}
        >
          {translate('ImportExistingScenes')}
        </Button>
      </div>

      <div className={styles.buttonContainer}>
        <Button
          to="/add/new/scene"
          kind={kinds.PRIMARY}
        >
          {translate('AddNewScene')}
        </Button>
      </div>
    </div>
  );
}

NoPerformer.propTypes = {
  totalItems: PropTypes.number.isRequired
};

export default NoPerformer;
