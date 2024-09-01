import PropTypes from 'prop-types';
import React from 'react';
import Button from 'Components/Link/Button';
import { kinds } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import styles from './NoScene.css';

function NoScene(props) {
  const { totalItems } = props;

  if (totalItems > 0) {
    return (
      <div>
        <div className={styles.message}>
          {translate('AllScenesHiddenDueToFilter')}
        </div>
      </div>
    );
  }

  return (
    <div>
      <div className={styles.message}>
        {translate('NoScenesExist')}
      </div>

      <div className={styles.buttonContainer}>
        <Button
          to="/add/import/scenes"
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

NoScene.propTypes = {
  totalItems: PropTypes.number.isRequired
};

export default NoScene;
