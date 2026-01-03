import PropTypes from 'prop-types';
import React from 'react';
import posterPlaceholder from 'Components/posterPlaceholder';
import MovieImage from 'Movie/MovieImage';

function SceneHeadshot(props) {
  return (
    <MovieImage
      {...props}
      coverType="headshot"
      placeholder={posterPlaceholder}
    />
  );
}

SceneHeadshot.propTypes = {
  size: PropTypes.number.isRequired
};

SceneHeadshot.defaultProps = {
  size: 250
};

export default SceneHeadshot;
