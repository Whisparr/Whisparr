import PropTypes from 'prop-types';
import React, { PureComponent } from 'react';
import Link from 'Components/Link/Link';

class MovieStudioLink extends PureComponent {

  render() {
    const {
      foreignId,
      studioTitle
    } = this.props;

    const link = `/studio/${foreignId}`;

    return (
      <Link
        to={link}
        title={studioTitle}
      >
        {studioTitle}
      </Link>
    );
  }
}

MovieStudioLink.propTypes = {
  foreignId: PropTypes.string.isRequired,
  studioTitle: PropTypes.string.isRequired
};

export default MovieStudioLink;
