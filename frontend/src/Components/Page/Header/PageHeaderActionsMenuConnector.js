import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { restart, shutdown } from 'Store/Actions/systemActions';
import PageHeaderActionsMenu from './PageHeaderActionsMenu';

const mapDispatchToProps = {
  restart,
  shutdown
};

class PageHeaderActionsMenuConnector extends Component {

  //
  // Listeners

  onRestartPress = () => {
    this.props.restart();
  };

  onShutdownPress = () => {
    this.props.shutdown();
  };

  //
  // Render

  render() {
    return (
      <PageHeaderActionsMenu
        {...this.props}
        onRestartPress={this.onRestartPress}
        onShutdownPress={this.onShutdownPress}
      />
    );
  }
}

PageHeaderActionsMenuConnector.propTypes = {
  onKeyboardShortcutsPress: PropTypes.func.isRequired,
  restart: PropTypes.func.isRequired,
  shutdown: PropTypes.func.isRequired
};

export default connect(null, mapDispatchToProps)(PageHeaderActionsMenuConnector);
