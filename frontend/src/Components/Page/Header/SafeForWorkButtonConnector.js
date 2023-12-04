import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { toggleSafeForWorkMode } from 'Store/Actions/settingsActions';
import SafeForWorkButton from './SafeForWorkButton';

function mapStateToProps(state) {
  return {
    safeForWorkMode: state.settings.safeForWorkMode
  };
}

const mapDispatchToProps = {
  toggleSafeForWorkMode
};

class SafeForWorkButtonConnector extends Component {

  //
  // Listeners

  onSafeForWorkModePress = () => {
    this.props.toggleSafeForWorkMode();
  };

  //
  // Render

  render() {
    return (
      <SafeForWorkButton
        onSafeForWorkModePress={this.onSafeForWorkModePress}
        {...this.props}
      />
    );
  }
}

SafeForWorkButtonConnector.propTypes = {
  toggleSafeForWorkMode: PropTypes.func.isRequired
};

export default connect(mapStateToProps, mapDispatchToProps)(SafeForWorkButtonConnector);
