import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { togglePerformerMonitored } from 'Store/Actions/performerActions';

function createMapStateToProps() {
  return createSelector(
    (state, { performer }) => performer,
    (state) => state.settings.safeForWorkMode,
    (performer, safeForWorkMode) => {
      return {
        performer,
        safeForWorkMode
      };
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    dispatchTogglePerformerMonitored(payload) {
      dispatch(togglePerformerMonitored(payload));
    }
  };
}

class MovieCreditPosterConnector extends Component {

  //
  // Listeners

  onTogglePerformerMonitored = (monitored) => {
    this.props.dispatchTogglePerformerMonitored({
      performerId: this.props.performer.id,
      monitored
    });
  };

  //
  // Render

  render() {
    const {
      performer,
      component: ItemComponent
    } = this.props;

    return (
      <ItemComponent
        {...this.props}
        performer={performer}
        onTogglePerformerMonitored={this.onTogglePerformerMonitored}
      />
    );
  }
}

MovieCreditPosterConnector.propTypes = {
  performer: PropTypes.object.isRequired,
  performerForeignId: PropTypes.string.isRequired,
  component: PropTypes.elementType.isRequired,
  dispatchTogglePerformerMonitored: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, createMapDispatchToProps)(MovieCreditPosterConnector);
