import classNames from 'classnames';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import SpinnerIconButton from 'Components/Link/SpinnerIconButton';
import { icons } from 'Helpers/Props';
import styles from './MonitorToggleButton.css';

function getTooltip(monitored, type, isDisabled, tooltip) {
  if (tooltip) {
    return tooltip;
  }

  if (isDisabled) {
    return 'Cannot toggle monitored state when movie is unmonitored';
  }

  if (monitored) {
    const monitoredLabels = {
      movieMonitor: 'Monitored Movies, click to unmonitor',
      sceneMonitor: 'Monitored Scenes, click to unmonitor'
    };

    return monitoredLabels[type] ?? 'Monitored, click to unmonitor';
  }

  const unMonitoredLabels = {
    movieMonitor: 'Unmonitored Movies, click to monitor',
    sceneMonitor: 'Unmonitored Scenes, click to monitor'
  };

  return unMonitoredLabels[type] ?? 'Unmonitored, click to monitor';
}

class MonitorToggleButton extends Component {

  //
  // Listeners

  onPress = (event) => {
    const shiftKey = event.nativeEvent.shiftKey;
    if (this.props.type === 'movieMonitor') {
      this.props.onPress({ monitored: this.props.monitored, moviesMonitored: !this.props.moviesMonitored }, { shiftKey });
    } else if (this.props.type === 'sceneMonitor') {
      this.props.onPress({ monitored: !this.props.monitored, moviesMonitored: this.props.moviesMonitored }, { shiftKey });
    } else {
      this.props.onPress(!this.props.monitored, { shiftKey });
    }
  };

  //
  // Render

  render() {
    const {
      className,
      monitored,
      moviesMonitored,
      type,
      isDisabled,
      tooltip,
      isSaving,
      size,
      ...otherProps
    } = this.props;

    let monitorType = undefined;
    switch (type) {
      case 'movieMonitor':
        monitorType = 'movie';
        break;
      case 'sceneMonitor':
        monitorType = 'scene';
        break;
      default:
        monitorType = undefined;
    }

    const monitoredValue = monitorType === 'movie' ? moviesMonitored : monitored;

    let iconName = icons.UNMONITORED;

    if (monitorType) {
      const iconSet = monitorType === 'movie' ?
        { monitored: icons.FILM, unmonitored: icons.FILMUNMONITOR } :
        { monitored: icons.SCENE, unmonitored: icons.SCENEUNMONITOR };

      iconName = monitoredValue ? iconSet.monitored : iconSet.unmonitored;
    } else if (monitoredValue) {
      iconName = icons.MONITORED;
    }

    return (
      <SpinnerIconButton
        className={classNames(
          className,
          isDisabled && styles.isDisabled
        )}
        name={iconName}
        size={size}
        title={getTooltip(monitoredValue, type, isDisabled, tooltip)}
        isDisabled={isDisabled}
        isSpinning={isSaving}
        {...otherProps}
        onPress={this.onPress}
      />
    );
  }
}

MonitorToggleButton.propTypes = {
  className: PropTypes.string.isRequired,
  monitored: PropTypes.bool,
  moviesMonitored: PropTypes.bool,
  type: PropTypes.string,
  size: PropTypes.number,
  isDisabled: PropTypes.bool.isRequired,
  tooltip: PropTypes.string,
  isSaving: PropTypes.bool.isRequired,
  onPress: PropTypes.func.isRequired
};

MonitorToggleButton.defaultProps = {
  className: styles.toggleButton,
  isDisabled: false,
  isSaving: false,
  monitored: false
};

export default MonitorToggleButton;
