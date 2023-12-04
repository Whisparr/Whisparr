import PropTypes from 'prop-types';
import React, { PureComponent } from 'react';
import Link from 'Components/Link/Link';

class PerformerNameLink extends PureComponent {

  render() {
    const {
      foreignId,
      title
    } = this.props;

    const link = `/performer/${foreignId}`;

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

PerformerNameLink.propTypes = {
  foreignId: PropTypes.string.isRequired,
  title: PropTypes.string.isRequired
};

export default PerformerNameLink;
