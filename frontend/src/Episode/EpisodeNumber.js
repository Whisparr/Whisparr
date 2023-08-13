import PropTypes from 'prop-types';
import React from 'react';
import Icon from 'Components/Icon';
import { icons, kinds } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import styles from './EpisodeNumber.css';

function getWarningMessage(unverifiedSceneNumbering, absoluteEpisodeNumber) {
  const messages = [];

  if (unverifiedSceneNumbering) {
    messages.push(translate('SceneNumberNotVerified'));
  }

  return messages.join('\n');
}

function EpisodeNumber(props) {
  const {
    releaseDate,
    absoluteEpisodeNumber,
    unverifiedSceneNumbering
  } = props;

  const warningMessage = getWarningMessage(unverifiedSceneNumbering, absoluteEpisodeNumber);

  return (
    <span>
      {
        <span>
          {releaseDate}
        </span>
      }

      {
        warningMessage ?
          <Icon
            className={styles.warning}
            name={icons.WARNING}
            kind={kinds.WARNING}
            title={warningMessage}
          /> :
          null
      }

    </span>
  );
}

EpisodeNumber.propTypes = {
  releaseDate: PropTypes.string.isRequired,
  absoluteEpisodeNumber: PropTypes.number,
  useSceneNumbering: PropTypes.bool.isRequired,
  unverifiedSceneNumbering: PropTypes.bool.isRequired,
  alternateTitles: PropTypes.arrayOf(PropTypes.object).isRequired
};

EpisodeNumber.defaultProps = {
  useSceneNumbering: false,
  unverifiedSceneNumbering: false,
  alternateTitles: []
};

export default EpisodeNumber;
