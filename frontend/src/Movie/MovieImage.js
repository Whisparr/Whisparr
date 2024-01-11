import classNames from 'classnames';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import LazyLoad from 'react-lazyload';
import styles from './MovieImage.css';

function findImage(images, coverType) {
  return images.find((image) => image.coverType === coverType);
}

function getUrl(image, coverType, size) {
  const imageUrl = image?.url ?? image?.remoteUrl;

  if (imageUrl) {
    return imageUrl.replace(`${coverType}.jpg`, `${coverType}-${size}.jpg`);
  }
}

class MovieImage extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    const pixelRatio = Math.ceil(window.devicePixelRatio);

    const {
      images,
      coverType,
      size
    } = props;

    const image = findImage(images, coverType);

    this.state = {
      pixelRatio,
      image,
      url: getUrl(image, coverType, pixelRatio * size),
      isLoaded: false,
      hasError: false
    };
  }

  componentDidMount() {
    if (!this.state.url && this.props.onError) {
      this.props.onError();
    }
  }

  componentDidUpdate() {
    const {
      images,
      coverType,
      placeholder,
      size,
      onError
    } = this.props;

    const {
      image,
      pixelRatio
    } = this.state;

    const nextImage = findImage(images, coverType);

    if (nextImage && (!image || nextImage.url !== image.url || nextImage.remoteUrl !== image.remoteUrl)) {
      this.setState({
        image: nextImage,
        url: getUrl(nextImage, coverType, pixelRatio * size),
        hasError: false
        // Don't reset isLoaded, as we want to immediately try to
        // show the new image, whether an image was shown previously
        // or the placeholder was shown.
      });
    } else if (!nextImage && image) {
      this.setState({
        image: nextImage,
        url: placeholder,
        hasError: false
      });

      if (onError) {
        onError();
      }
    }
  }

  //
  // Listeners

  onError = () => {
    this.setState({
      hasError: true
    });

    if (this.props.onError) {
      this.props.onError();
    }
  };

  onLoad = () => {
    this.setState({
      isLoaded: true,
      hasError: false
    });

    if (this.props.onLoad) {
      this.props.onLoad();
    }
  };

  //
  // Render

  render() {
    const {
      className,
      style,
      placeholder,
      size,
      blur,
      lazy,
      overflow
    } = this.props;

    const {
      url,
      hasError,
      isLoaded
    } = this.state;

    if (hasError || !url) {
      return (
        <img
          className={className}
          style={style}
          src={placeholder}
        />
      );
    }

    if (lazy) {
      return (
        <LazyLoad
          height={size}
          offset={100}
          overflow={overflow}
          placeholder={
            <img
              className={className}
              style={style}
              src={placeholder}
            />
          }
        >
          <img
            className={classNames(
              className,
              blur && styles.blur
            )}
            style={style}
            src={url}
            onError={this.onError}
            onLoad={this.onLoad}
            rel="noreferrer"
          />
        </LazyLoad>
      );
    }

    return (
      <div className={classNames(styles.container, className)}>
        <img
          className={classNames(
            styles.image,
            blur && url && styles.blur
          )}
          style={style}
          src={isLoaded ? url : placeholder}
          onError={this.onError}
          onLoad={this.onLoad}
        />
      </div>
    );
  }
}

MovieImage.propTypes = {
  className: PropTypes.string,
  style: PropTypes.object,
  images: PropTypes.arrayOf(PropTypes.object).isRequired,
  coverType: PropTypes.string.isRequired,
  placeholder: PropTypes.string.isRequired,
  size: PropTypes.number.isRequired,
  lazy: PropTypes.bool.isRequired,
  blur: PropTypes.bool,
  overflow: PropTypes.bool.isRequired,
  onError: PropTypes.func,
  onLoad: PropTypes.func
};

MovieImage.defaultProps = {
  size: 250,
  lazy: true,
  blur: false,
  overflow: false
};

export default MovieImage;
