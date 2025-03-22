import PropTypes from 'prop-types';
import React from 'react';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';

class Delayed extends React.Component {

  constructor(props) {
    super(props);
    this.state = { hidden: true };
  }

  componentDidMount() {
    setTimeout(() => {
      this.setState({ hidden: false });
    }, this.props.waitBeforeShow);
  }

  render() {
    return this.state.hidden ? <LoadingIndicator /> : this.props.children;
  }
}

Delayed.propTypes = {
  waitBeforeShow: PropTypes.number.isRequired,
  children: PropTypes.element
};

export default Delayed;
