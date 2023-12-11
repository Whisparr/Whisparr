import _ from 'lodash';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import SceneCreditPosters from '../SceneCreditPosters';
import SceneCrewPoster from './SceneCrewPoster';

function createMapStateToProps() {
  return createSelector(
    (state) => state.sceneCredits.items,
    (credits) => {
      const crew = _.reduce(credits, (acc, credit) => {
        if (credit.type === 'crew') {
          acc.push(credit);
        }

        return acc;
      }, []);

      return {
        items: _.uniqBy(crew, 'personName')
      };
    }
  );
}

class SceneCrewPostersConnector extends Component {

  //
  // Render

  render() {

    return (
      <SceneCreditPosters
        {...this.props}
        itemComponent={SceneCrewPoster}
      />
    );
  }
}

export default connect(createMapStateToProps)(SceneCrewPostersConnector);
