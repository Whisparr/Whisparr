import PropTypes from 'prop-types';
import React, { PureComponent } from 'react';
import Link from 'Components/Link/Link';

class StudioTitleLink extends PureComponent {

  render() {
    const {
      foreignId,
      title
    } = this.props;

    const link = `/studio/${foreignId}`;

    return (
      <Link
        to={link}
        title={title}
      >
        {title}
      </Link>
    );
  }
}

StudioTitleLink.propTypes = {
  foreignId: PropTypes.string.isRequired,
  title: PropTypes.string.isRequired
};

export default StudioTitleLink;
