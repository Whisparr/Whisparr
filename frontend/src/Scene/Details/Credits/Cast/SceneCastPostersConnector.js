import _ from 'lodash';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import SceneCreditPosters from '../SceneCreditPosters';
import SceneCastPoster from './SceneCastPoster';

function createMapStateToProps() {
  return createSelector(
    (state) => state.sceneCredits.items,
    (credits) => {
      const cast = _.reduce(credits, (acc, credit) => {
        if (credit.type === 'cast') {
          acc.push(credit);
        }

        return acc;
      }, []);

      return {
        items: cast
      };
    }
  );
}

class SceneCastPostersConnector extends Component {

  //
  // Render

  render() {

    return (
      <SceneCreditPosters
        {...this.props}
        itemComponent={SceneCastPoster}
      />
    );
  }
}

export default connect(createMapStateToProps)(SceneCastPostersConnector);
