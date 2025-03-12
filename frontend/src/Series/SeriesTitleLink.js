import PropTypes from 'prop-types';
import React from 'react';
import Link from 'Components/Link/Link';
import Tooltip from '../Components/Tooltip/Tooltip';

function SeriesTitleLink({ titleSlug, title, tvdbId }) {
  const link = `/site/${titleSlug}`;

  return (
    <Tooltip
      anchor={
        <Link to={link}>
          {title}
        </Link>
      }
      tooltip={tvdbId ? `TPDB ID: ${tvdbId}` : 'No TVDB ID available'}
      kind="inverse"
      position="top"
    />
  );
}

SeriesTitleLink.propTypes = {
  titleSlug: PropTypes.string.isRequired,
  title: PropTypes.string.isRequired,
  tvdbId: PropTypes.number
};

export default SeriesTitleLink;
