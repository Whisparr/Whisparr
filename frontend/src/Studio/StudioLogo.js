import PropTypes from 'prop-types';
import React from 'react';
import posterPlaceholder from 'Components/posterPlaceholder';
import MovieImage from 'Movie/MovieImage';

function StudioLogo(props) {
  return (
    <MovieImage
      {...props}
      coverType="clearlogo"
      placeholder={posterPlaceholder}
    />
  );
}

StudioLogo.propTypes = {
  ...MovieImage.propTypes,
  coverType: PropTypes.string,
  placeholder: PropTypes.string,
  overflow: PropTypes.bool,
  size: PropTypes.number.isRequired,
  lazy: PropTypes.bool
};

StudioLogo.defaultProps = {
  ...MovieImage.defaultProps,
  size: 250
};

export default StudioLogo;
