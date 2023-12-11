import PropTypes from 'prop-types';
import React, { Component } from 'react';
import MonitorToggleButton from 'Components/MonitorToggleButton';
import styles from './SceneCollectionLabel.css';

class SceneCollectionLabel extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      hasPosterError: false
    };
  }

  render() {
    const {
      title,
      monitored,
      onMonitorTogglePress
    } = this.props;

    return (
      <div>
        <MonitorToggleButton
          className={styles.monitorToggleButton}
          monitored={monitored}
          size={15}
          onPress={onMonitorTogglePress}
        />
        {title}
      </div>
    );
  }
}

SceneCollectionLabel.propTypes = {
  title: PropTypes.string.isRequired,
  monitored: PropTypes.bool.isRequired,
  onMonitorTogglePress: PropTypes.func.isRequired
};

export default SceneCollectionLabel;
