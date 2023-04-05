import React, { useCallback, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import { MOVIE_SEARCH, REFRESH_MOVIE } from 'Commands/commandNames';
import Label from 'Components/Label';
import IconButton from 'Components/Link/IconButton';
import Link from 'Components/Link/Link';
import SpinnerIconButton from 'Components/Link/SpinnerIconButton';
import { icons } from 'Helpers/Props';
import DeleteMovieModal from 'Movie/Delete/DeleteMovieModal';
import EditMovieModalConnector from 'Movie/Edit/EditMovieModalConnector';
import MovieIndexProgressBar from 'Movie/Index/ProgressBar/MovieIndexProgressBar';
import MovieIndexPosterSelect from 'Movie/Index/Select/MovieIndexPosterSelect';
import { Statistics } from 'Movie/Movie';
import MoviePoster from 'Movie/MoviePoster';
import { executeCommand } from 'Store/Actions/commandActions';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import translate from 'Utilities/String/translate';
import createMovieIndexItemSelector from '../createMovieIndexItemSelector';
import MovieIndexPosterInfo from './MovieIndexPosterInfo';
import selectPosterOptions from './selectPosterOptions';
import styles from './MovieIndexPoster.css';

interface MovieIndexPosterProps {
  movieId: number;
  sortKey: string;
  isSelectMode: boolean;
  posterWidth: number;
  posterHeight: number;
}

function MovieIndexPoster(props: MovieIndexPosterProps) {
  const { movieId, sortKey, isSelectMode, posterWidth, posterHeight } = props;

  const { movie, qualityProfile, isRefreshingMovie, isSearchingMovie } =
    useSelector(createMovieIndexItemSelector(props.movieId));

  const {
    detailedProgressBar,
    showTitle,
    showMonitored,
    showQualityProfile,
    showSearchAction,
  } = useSelector(selectPosterOptions);

  const safeForWorkMode = useSelector(
    (state: AppState) => state.settings.safeForWorkMode
  );

  const { showRelativeDates, shortDateFormat, longDateFormat, timeFormat } =
    useSelector(createUISettingsSelector());

  const {
    title,
    monitored,
    status,
    images,
    tmdbId,
    studio,
    added,
    year,
    path,
    statistics = {} as Statistics,
    originalLanguage,
  } = movie;

  const { sizeOnDisk = 0 } = statistics;

  const dispatch = useDispatch();
  const [hasPosterError, setHasPosterError] = useState(false);
  const [isEditMovieModalOpen, setIsEditMovieModalOpen] = useState(false);
  const [isDeleteMovieModalOpen, setIsDeleteMovieModalOpen] = useState(false);

  const onRefreshPress = useCallback(() => {
    dispatch(
      executeCommand({
        name: REFRESH_MOVIE,
        movieIds: [movieId],
      })
    );
  }, [movieId, dispatch]);

  const onSearchPress = useCallback(() => {
    dispatch(
      executeCommand({
        name: MOVIE_SEARCH,
        movieIds: [movieId],
      })
    );
  }, [movieId, dispatch]);

  const onPosterLoadError = useCallback(() => {
    setHasPosterError(true);
  }, [setHasPosterError]);

  const onPosterLoad = useCallback(() => {
    setHasPosterError(false);
  }, [setHasPosterError]);

  const onEditMoviePress = useCallback(() => {
    setIsEditMovieModalOpen(true);
  }, [setIsEditMovieModalOpen]);

  const onEditMovieModalClose = useCallback(() => {
    setIsEditMovieModalOpen(false);
  }, [setIsEditMovieModalOpen]);

  const onDeleteMoviePress = useCallback(() => {
    setIsEditMovieModalOpen(false);
    setIsDeleteMovieModalOpen(true);
  }, [setIsDeleteMovieModalOpen]);

  const onDeleteMovieModalClose = useCallback(() => {
    setIsDeleteMovieModalOpen(false);
  }, [setIsDeleteMovieModalOpen]);

  const link = `/movie/${tmdbId}`;

  const elementStyle = {
    width: `${posterWidth}px`,
    height: `${posterHeight}px`,
  };

  return (
    <div className={styles.content}>
      <div className={styles.posterContainer} title={title}>
        {isSelectMode ? <MovieIndexPosterSelect movieId={movieId} /> : null}

        <Label className={styles.controls}>
          <SpinnerIconButton
            name={icons.REFRESH}
            title={translate('RefreshMovie')}
            isSpinning={isRefreshingMovie}
            onPress={onRefreshPress}
          />

          {showSearchAction ? (
            <SpinnerIconButton
              className={styles.action}
              name={icons.SEARCH}
              title={translate('SearchForMovie')}
              isSpinning={isSearchingMovie}
              onPress={onSearchPress}
            />
          ) : null}

          <IconButton
            name={icons.EDIT}
            title={translate('EditMovie')}
            onPress={onEditMoviePress}
          />
        </Label>

        <Link className={styles.link} style={elementStyle} to={link}>
          <MoviePoster
            style={elementStyle}
            images={images}
            blur={safeForWorkMode}
            size={250}
            lazy={false}
            overflow={true}
            onError={onPosterLoadError}
            onLoad={onPosterLoad}
          />

          {hasPosterError ? (
            <div className={styles.overlayTitle}>{title}</div>
          ) : null}
        </Link>
      </div>

      <MovieIndexProgressBar
        movieId={movieId}
        monitored={monitored}
        status={status}
        width={posterWidth}
        detailedProgressBar={detailedProgressBar}
        bottomRadius={false}
      />

      {showTitle ? (
        <div className={styles.title} title={title}>
          {title}
        </div>
      ) : null}

      {showMonitored ? (
        <div className={styles.title}>
          {monitored ? translate('Monitored') : translate('Unmonitored')}
        </div>
      ) : null}

      {showQualityProfile ? (
        <div className={styles.title} title={translate('QualityProfile')}>
          {qualityProfile.name}
        </div>
      ) : null}

      <MovieIndexPosterInfo
        studio={studio}
        qualityProfile={qualityProfile}
        added={added}
        year={year}
        showQualityProfile={showQualityProfile}
        showRelativeDates={showRelativeDates}
        shortDateFormat={shortDateFormat}
        longDateFormat={longDateFormat}
        timeFormat={timeFormat}
        sizeOnDisk={sizeOnDisk}
        sortKey={sortKey}
        path={path}
        originalLanguage={originalLanguage}
      />

      <EditMovieModalConnector
        isOpen={isEditMovieModalOpen}
        movieId={movieId}
        onModalClose={onEditMovieModalClose}
        onDeleteMoviePress={onDeleteMoviePress}
      />

      <DeleteMovieModal
        isOpen={isDeleteMovieModalOpen}
        movieId={movieId}
        onModalClose={onDeleteMovieModalClose}
      />
    </div>
  );
}

export default MovieIndexPoster;
