import React, { useCallback, useEffect, useRef, useState } from 'react';
import { CoverType, Image } from './Movie';
import styles from './MovieImage.css';

function findImage(images: Image[], coverType: CoverType) {
  const preferred = images.find((image) => image.coverType === coverType);
  if (preferred) return preferred;

  const fallbacks = ['logo', 'clearlogo', 'poster'];
  for (const fb of fallbacks) {
    const found = images.find((image) => image.coverType === fb);
    if (found) return found;
  }

  return images.length > 0 ? images[0] : undefined;
}

function getUrl(image: Image, coverType: CoverType, size: number) {
  const imageUrl = image?.url ?? image?.remoteUrl;
  return imageUrl
    ? imageUrl.replace(`${coverType}.jpg`, `${coverType}-${size}.jpg`)
    : null;
}

export interface MovieImageProps {
  className?: string;
  style?: object;
  images: Image[];
  coverType: CoverType;
  safeForWorkMode?: boolean;
  placeholder: string;
  size?: number;
  lazy?: boolean;
  overflow?: boolean;
  onError?: () => void;
  onLoad?: () => void;
}

const pixelRatio = Math.max(Math.round(window.devicePixelRatio), 1);

function MovieImage({
  className,
  style,
  images,
  coverType,
  placeholder,
  safeForWorkMode,
  size = 250,
  lazy = true,
  onError,
  onLoad,
}: MovieImageProps) {
  const [url, setUrl] = useState<string | null>(null);
  const [hasError, setHasError] = useState(false);
  const image = useRef<Image | null>(null);
  const triedRemote = useRef(false);

  const handleLoad = useCallback(() => {
    setHasError(false);
    onLoad?.();
  }, [onLoad]);

  const handleError = useCallback(() => {
    const nextImage = image.current;
    if (!triedRemote.current && nextImage?.remoteUrl) {
      triedRemote.current = true;
      setUrl(nextImage.remoteUrl);
      return;
    }

    setHasError(true);
    onError?.();
  }, [onError]);

  useEffect(() => {
    const nextImage = findImage(images, coverType);

    if (nextImage && (!image.current || nextImage.url !== image.current.url)) {
      image.current = nextImage;

      const computedUrl = getUrl(nextImage, coverType, pixelRatio * size);
      triedRemote.current = false;
      setUrl(computedUrl);
      setHasError(false);
    } else if (!nextImage) {
      if (image.current) {
        image.current = null;
        setUrl(placeholder);
        setHasError(false);
        onError?.();
      }
    }
  }, [images, coverType, placeholder, size, onError]);

  useEffect(() => {
    if (!image.current) {
      onError?.();
    }
    // run once only
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  if (hasError || !url) {
    return <img className={className} style={style} src={placeholder} />;
  }

  const blurClass = safeForWorkMode ? styles.blur : 'blur';
  return (
    <img
      className={`${className ?? ''} ${blurClass}`}
      style={style}
      src={url ?? placeholder}
      loading={lazy ? 'lazy' : undefined}
      onError={handleError}
      onLoad={handleLoad}
    />
  );
}

export default MovieImage;
