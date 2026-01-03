import React from 'react';
import { useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import posterPlaceholder from 'Components/posterPlaceholder';
import MovieImage, { MovieImageProps } from './MovieImage';

interface MoviePosterProps
  extends Omit<MovieImageProps, 'coverType' | 'placeholder'> {
  size?: 250 | 500;
  safeForWorkMode?: boolean;
}

function MoviePoster({ size = 250, ...otherProps }: MoviePosterProps) {
  const safeForWorkMode = useSelector(
    (state: AppState) => state.settings.safeForWorkMode
  );
  return (
    <MovieImage
      {...otherProps}
      size={size}
      safeForWorkMode={safeForWorkMode}
      coverType="poster"
      placeholder={posterPlaceholder}
    />
  );
}

export default MoviePoster;
