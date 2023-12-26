import _ from 'lodash';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createMovieSelector from 'Store/Selectors/createMovieSelector';
import MovieCreditPosters from '../MovieCreditPosters';
import MovieCastPoster from './MovieCastPoster';

function createMapStateToProps() {
  return createSelector(
    createMovieSelector(),
    (movie) => {
      const cast = _.reduce(movie.credits, (acc, credit) => {
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

class MovieCastPostersConnector extends Component {

  //
  // Render

  render() {

    return (
      <MovieCreditPosters
        {...this.props}
        itemComponent={MovieCastPoster}
      />
    );
  }
}

export default connect(createMapStateToProps)(MovieCastPostersConnector);
