import PropTypes from 'prop-types';
import React from 'react';
import EpisodeNumber from './EpisodeNumber';

function SeasonEpisodeNumber(props) {
  const {
    airDate,
    ...otherProps
  } = props;

  return (
    <EpisodeNumber
      showSeasonNumber={true}
      {...otherProps}
    />
  );
}

SeasonEpisodeNumber.propTypes = {
  airDate: PropTypes.string
};

export default SeasonEpisodeNumber;
