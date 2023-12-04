import _ from 'lodash';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createMovieSelector from 'Store/Selectors/createMovieSelector';
import MovieCreditPosters from '../MovieCreditPosters';
import MovieCrewPoster from './MovieCrewPoster';

function createMapStateToProps() {
  return createSelector(
    createMovieSelector(),
    (movie) => {
      const crew = _.reduce(movie.credits, (acc, credit) => {
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

class MovieCrewPostersConnector extends Component {

  //
  // Render

  render() {

    return (
      <MovieCreditPosters
        {...this.props}
        itemComponent={MovieCrewPoster}
      />
    );
  }
}

export default connect(createMapStateToProps)(MovieCrewPostersConnector);
