import PropTypes from 'prop-types';
import React, { PureComponent } from 'react';
import Link from 'Components/Link/Link';

class SceneStudioTitleLink extends PureComponent {

  render() {
    const {
      studioForeignId,
      studioTitle
    } = this.props;

    const link = `/studio/${studioForeignId}`;

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

SceneStudioTitleLink.propTypes = {
  studioForeignId: PropTypes.string.isRequired,
  studioTitle: PropTypes.string.isRequired
};

export default SceneStudioTitleLink;
