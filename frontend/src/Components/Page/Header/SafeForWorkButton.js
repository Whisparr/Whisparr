import PropTypes from 'prop-types';
import React from 'react';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import { icons } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import styles from './SafeForWorkButton.css';

function SafeForWorkButton(props) {
  const {
    safeForWorkMode,
    onSafeForWorkModePress
  } = props;

  return (
    <Link
      className={styles.button}
      title={safeForWorkMode ? translate('HiddenClickToShow') : translate('ShownClickToHide')}
      onPress={onSafeForWorkModePress}
    >
      <Icon
        name={safeForWorkMode ? icons.SFW : icons.NSFW}
        size={21}
      />
    </Link>
  );
}

SafeForWorkButton.propTypes = {
  safeForWorkMode: PropTypes.bool,
  onSafeForWorkModePress: PropTypes.func.isRequired
};

export default SafeForWorkButton;
