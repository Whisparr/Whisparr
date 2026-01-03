import React from 'react';
import { useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import posterPlaceholder from 'Components/posterPlaceholder';
import MovieImage, { MovieImageProps } from '../Movie/MovieImage';

interface ScenePosterProps
  extends Omit<MovieImageProps, 'coverType' | 'placeholder'> {
  size?: 180;
  safeForWorkMode: boolean;
}

function ScenePoster({ size = 180, ...otherProps }: ScenePosterProps) {
  const safeForWorkMode = useSelector(
    (state: AppState) => state.settings.safeForWorkMode
  );
  return (
    <MovieImage
      {...otherProps}
      size={size}
      safeForWorkMode={safeForWorkMode}
      coverType="screenshot"
      placeholder={posterPlaceholder}
    />
  );
}

export default ScenePoster;
