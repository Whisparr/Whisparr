import PropTypes from 'prop-types';
import React from 'react';
import posterPlaceholder from 'Components/posterPlaceholder';
import MovieImage from './MovieImage';

function MovieHeadshot(props) {
  return (
    <MovieImage
      {...props}
      coverType="headshot"
      placeholder={posterPlaceholder}
    />
  );
}

MovieHeadshot.propTypes = {
  ...MovieImage.propTypes,
  coverType: PropTypes.string,
  placeholder: PropTypes.string,
  overflow: PropTypes.bool,
  size: PropTypes.number.isRequired
};

MovieHeadshot.defaultProps = {
  ...MovieImage.defaultProps,
  size: 250
};

export default MovieHeadshot;
