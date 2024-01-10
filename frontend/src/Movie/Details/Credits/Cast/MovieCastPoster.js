import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Link from 'Components/Link/Link';
import MonitorToggleButton from 'Components/MonitorToggleButton';
import MovieHeadshot from 'Movie/MovieHeadshot';
import styles from '../MovieCreditPoster.css';

class MovieCastPoster extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      hasPosterError: false
    };
  }

  //
  // Listeners

  onPosterLoad = () => {
    if (this.state.hasPosterError) {
      this.setState({ hasPosterError: false });
    }
  };

  onPosterLoadError = () => {
    if (!this.state.hasPosterError) {
      this.setState({ hasPosterError: true });
    }
  };

  //
  // Render

  render() {
    const {
      performer,
      character,
      posterWidth,
      posterHeight,
      safeForWorkMode,
      onTogglePerformerMonitored
    } = this.props;

    const {
      hasPosterError
    } = this.state;

    const elementStyle = {
      width: `${posterWidth}px`,
      height: `${posterHeight}px`,
      borderRadius: '5px'
    };

    const contentStyle = {
      width: `${posterWidth}px`
    };

    const link = `/performer/${performer.foreignId}`;

    return (
      <div
        className={styles.content}
        style={contentStyle}
      >
        <div className={styles.posterContainer}>
          <div className={styles.controls}>
            <MonitorToggleButton
              className={styles.action}
              monitored={performer.monitored}
              size={20}
              onPress={onTogglePerformerMonitored}
            />
          </div>

          <div
            style={elementStyle}
          >
            <Link className={styles.link} to={link}>
              <MovieHeadshot
                blur={safeForWorkMode}
                className={styles.poster}
                style={elementStyle}
                images={performer.images}
                size={250}
                lazy={false}
                overflow={true}
                onError={this.onPosterLoadError}
                onLoad={this.onPosterLoad}
              />

              {
                hasPosterError &&
                  <div className={styles.overlayTitle}>
                    {performer.fullName}
                  </div>
              }
            </Link>
          </div>
        </div>

        <div className={styles.title}>
          {performer.fullName}
        </div>
        <div className={styles.title}>
          {character}
        </div>
      </div>
    );
  }
}

MovieCastPoster.propTypes = {
  performer: PropTypes.object.isRequired,
  character: PropTypes.string,
  posterWidth: PropTypes.number.isRequired,
  posterHeight: PropTypes.number.isRequired,
  safeForWorkMode: PropTypes.bool.isRequired,
  onTogglePerformerMonitored: PropTypes.func.isRequired
};

export default MovieCastPoster;
