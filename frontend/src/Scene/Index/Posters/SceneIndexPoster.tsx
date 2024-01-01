import React, { useCallback, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import { MOVIE_SEARCH, REFRESH_MOVIE } from 'Commands/commandNames';
import Icon from 'Components/Icon';
import Label from 'Components/Label';
import IconButton from 'Components/Link/IconButton';
import Link from 'Components/Link/Link';
import SpinnerIconButton from 'Components/Link/SpinnerIconButton';
import RottenTomatoRating from 'Components/RottenTomatoRating';
import TmdbRating from 'Components/TmdbRating';
import Popover from 'Components/Tooltip/Popover';
import { icons } from 'Helpers/Props';
import EditMovieModalConnector from 'Movie/Edit/EditMovieModalConnector';
import MovieIndexPosterSelect from 'Movie/Index/Select/MovieIndexPosterSelect';
import DeleteSceneModal from 'Scene/Delete/DeleteSceneModal';
import SceneDetailsLinks from 'Scene/Details/SceneDetailsLinks';
import SceneIndexProgressBar from 'Scene/Index/ProgressBar/SceneIndexProgressBar';
import ScenePoster from 'Scene/ScenePoster';
import { executeCommand } from 'Store/Actions/commandActions';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import getRelativeDate from 'Utilities/Date/getRelativeDate';
import translate from 'Utilities/String/translate';
import createSceneIndexItemSelector from '../createSceneIndexItemSelector';
import SceneIndexPosterInfo from './SceneIndexPosterInfo';
import selectPosterOptions from './selectPosterOptions';
import styles from './SceneIndexPoster.css';

interface SceneIndexPosterProps {
  sceneId: number;
  sortKey: string;
  isSelectMode: boolean;
  posterWidth: number;
  posterHeight: number;
}

function SceneIndexPoster(props: SceneIndexPosterProps) {
  const { sceneId, sortKey, isSelectMode, posterWidth, posterHeight } = props;

  const { scene, qualityProfile, isRefreshingScene, isSearchingScene } =
    useSelector(createSceneIndexItemSelector(props.sceneId));

  const safeForWorkMode = useSelector(
    (state: AppState) => state.settings.safeForWorkMode
  );

  const {
    detailedProgressBar,
    showTitle,
    showMonitored,
    showQualityProfile,
    showReleaseDate,
    showTmdbRating,
    showRottenTomatoesRating,
    showSearchAction,
  } = useSelector(selectPosterOptions);

  const { showRelativeDates, shortDateFormat, longDateFormat, timeFormat } =
    useSelector(createUISettingsSelector());

  const {
    title,
    monitored,
    status,
    images,
    foreignId,
    tmdbId,
    imdbId,
    hasFile,
    isAvailable,
    studioTitle,
    added,
    year,
    releaseDate,
    path,
    movieFile,
    ratings,
    sizeOnDisk,
    originalLanguage,
  } = scene;

  const dispatch = useDispatch();
  const [hasPosterError, setHasPosterError] = useState(false);
  const [isEditSceneModalOpen, setIsEditSceneModalOpen] = useState(false);
  const [isDeleteSceneModalOpen, setIsDeleteSceneModalOpen] = useState(false);

  const onRefreshPress = useCallback(() => {
    dispatch(
      executeCommand({
        name: REFRESH_MOVIE,
        movieIds: [sceneId],
      })
    );
  }, [sceneId, dispatch]);

  const onSearchPress = useCallback(() => {
    dispatch(
      executeCommand({
        name: MOVIE_SEARCH,
        movieIds: [sceneId],
      })
    );
  }, [sceneId, dispatch]);

  const onPosterLoadError = useCallback(() => {
    setHasPosterError(true);
  }, [setHasPosterError]);

  const onPosterLoad = useCallback(() => {
    setHasPosterError(false);
  }, [setHasPosterError]);

  const onEditScenePress = useCallback(() => {
    setIsEditSceneModalOpen(true);
  }, [setIsEditSceneModalOpen]);

  const onEditSceneModalClose = useCallback(() => {
    setIsEditSceneModalOpen(false);
  }, [setIsEditSceneModalOpen]);

  const onDeleteScenePress = useCallback(() => {
    setIsEditSceneModalOpen(false);
    setIsDeleteSceneModalOpen(true);
  }, [setIsDeleteSceneModalOpen]);

  const onDeleteSceneModalClose = useCallback(() => {
    setIsDeleteSceneModalOpen(false);
  }, [setIsDeleteSceneModalOpen]);

  const link = `/movie/${foreignId}`;

  const elementStyle = {
    width: `${posterWidth}px`,
    height: `${posterHeight}px`,
  };

  return (
    <div className={styles.content}>
      <div className={styles.posterContainer} title={title}>
        {isSelectMode ? <MovieIndexPosterSelect movieId={sceneId} /> : null}

        <Label className={styles.controls}>
          <SpinnerIconButton
            name={icons.REFRESH}
            title={translate('RefreshScene')}
            isSpinning={isRefreshingScene}
            onPress={onRefreshPress}
          />

          {showSearchAction ? (
            <SpinnerIconButton
              className={styles.action}
              name={icons.SEARCH}
              title={translate('SearchForScene')}
              isSpinning={isSearchingScene}
              onPress={onSearchPress}
            />
          ) : null}

          <IconButton
            name={icons.EDIT}
            title={translate('EditScene')}
            onPress={onEditScenePress}
          />

          <span className={styles.externalLinks}>
            <Popover
              anchor={<Icon name={icons.EXTERNAL_LINK} size={12} />}
              title={translate('Links')}
              body={<SceneDetailsLinks tmdbId={tmdbId} imdbId={imdbId} />}
            />
          </span>
        </Label>

        <Link className={styles.link} style={elementStyle} to={link}>
          <ScenePoster
            blur={safeForWorkMode}
            style={elementStyle}
            images={images}
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

      <SceneIndexProgressBar
        sceneId={sceneId}
        sceneFile={movieFile}
        monitored={monitored}
        hasFile={hasFile}
        isAvailable={isAvailable}
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

      {showQualityProfile && !!qualityProfile?.name ? (
        <div className={styles.title} title={translate('QualityProfile')}>
          {qualityProfile.name}
        </div>
      ) : null}

      {showReleaseDate && releaseDate ? (
        <div className={styles.title}>
          <Icon name={icons.DISC} />{' '}
          {getRelativeDate(releaseDate, shortDateFormat, showRelativeDates, {
            timeFormat,
            timeForToday: false,
          })}
        </div>
      ) : null}

      {showTmdbRating && !!ratings.tmdb ? (
        <div className={styles.title}>
          <TmdbRating ratings={ratings} iconSize={12} />
        </div>
      ) : null}

      {showRottenTomatoesRating && !!ratings.rottenTomatoes ? (
        <div className={styles.title}>
          <RottenTomatoRating ratings={ratings} iconSize={12} />
        </div>
      ) : null}

      <SceneIndexPosterInfo
        studio={studioTitle}
        qualityProfile={qualityProfile}
        added={added}
        year={year}
        showQualityProfile={showQualityProfile}
        showReleaseDate={showReleaseDate}
        showRelativeDates={showRelativeDates}
        shortDateFormat={shortDateFormat}
        longDateFormat={longDateFormat}
        timeFormat={timeFormat}
        releaseDate={releaseDate}
        ratings={ratings}
        sizeOnDisk={sizeOnDisk}
        sortKey={sortKey}
        path={path}
        originalLanguage={originalLanguage}
        showTmdbRating={showTmdbRating}
        showRottenTomatoesRating={showRottenTomatoesRating}
      />

      <EditMovieModalConnector
        isOpen={isEditSceneModalOpen}
        movieId={sceneId}
        onModalClose={onEditSceneModalClose}
        onDeleteMoviePress={onDeleteScenePress}
      />

      <DeleteSceneModal
        isOpen={isDeleteSceneModalOpen}
        sceneId={sceneId}
        onModalClose={onDeleteSceneModalClose}
      />
    </div>
  );
}

export default SceneIndexPoster;
