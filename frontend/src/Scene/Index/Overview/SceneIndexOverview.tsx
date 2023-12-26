import React, { useCallback, useMemo, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import TextTruncate from 'react-text-truncate';
import AppState from 'App/State/AppState';
import { MOVIE_SEARCH, REFRESH_MOVIE } from 'Commands/commandNames';
import Icon from 'Components/Icon';
import IconButton from 'Components/Link/IconButton';
import Link from 'Components/Link/Link';
import SpinnerIconButton from 'Components/Link/SpinnerIconButton';
import Popover from 'Components/Tooltip/Popover';
import { icons } from 'Helpers/Props';
import DeleteSceneModal from 'Scene/Delete/DeleteSceneModal';
import SceneDetailsLinks from 'Scene/Details/SceneDetailsLinks';
import EditSceneModalConnector from 'Scene/Edit/EditSceneModalConnector';
import SceneIndexProgressBar from 'Scene/Index/ProgressBar/SceneIndexProgressBar';
import SceneIndexPosterSelect from 'Scene/Index/Select/SceneIndexPosterSelect';
import ScenePoster from 'Scene/ScenePoster';
import { executeCommand } from 'Store/Actions/commandActions';
import dimensions from 'Styles/Variables/dimensions';
import fonts from 'Styles/Variables/fonts';
import translate from 'Utilities/String/translate';
import createSceneIndexItemSelector from '../createSceneIndexItemSelector';
import SceneIndexOverviewInfo from './SceneIndexOverviewInfo';
import selectOverviewOptions from './selectOverviewOptions';
import styles from './SceneIndexOverview.css';

const columnPadding = parseInt(dimensions.movieIndexColumnPadding);
const columnPaddingSmallScreen = parseInt(
  dimensions.movieIndexColumnPaddingSmallScreen
);
const defaultFontSize = parseInt(fonts.defaultFontSize);
const lineHeight = parseFloat(fonts.lineHeight);

// Hardcoded height beased on line-height of 32 + bottom margin of 10.
// Less side-effecty than using react-measure.
const titleRowHeight = 42;

interface SceneIndexOverviewProps {
  sceneId: number;
  sortKey: string;
  posterWidth: number;
  posterHeight: number;
  rowHeight: number;
  isSelectMode: boolean;
  isSmallScreen: boolean;
}

function SceneIndexOverview(props: SceneIndexOverviewProps) {
  const {
    sceneId,
    sortKey,
    posterWidth,
    posterHeight,
    rowHeight,
    isSelectMode,
    isSmallScreen,
  } = props;

  const { scene, qualityProfile, isRefreshingScene, isSearchingScene } =
    useSelector(createSceneIndexItemSelector(props.sceneId));

  const safeForWorkMode = useSelector(
    (state: AppState) => state.settings.safeForWorkMode
  );

  const overviewOptions = useSelector(selectOverviewOptions);

  const {
    title,
    monitored,
    status,
    path,
    overview,
    images,
    hasFile,
    isAvailable,
    foreignId,
    tmdbId,
    imdbId,
    studio,
    sizeOnDisk,
    added,
  } = scene;

  const dispatch = useDispatch();
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

  const contentHeight = useMemo(() => {
    const padding = isSmallScreen ? columnPaddingSmallScreen : columnPadding;

    return rowHeight - padding;
  }, [rowHeight, isSmallScreen]);

  const overviewHeight = contentHeight - titleRowHeight;

  return (
    <div>
      <div className={styles.content}>
        <div className={styles.poster}>
          <div className={styles.posterContainer}>
            {isSelectMode ? <SceneIndexPosterSelect sceneId={sceneId} /> : null}
            <Link className={styles.link} style={elementStyle} to={link}>
              <ScenePoster
                blur={safeForWorkMode}
                className={styles.poster}
                style={elementStyle}
                images={images}
                size={250}
                lazy={false}
                overflow={true}
              />
            </Link>
          </div>

          <SceneIndexProgressBar
            sceneId={sceneId}
            sceneFile={scene.movieFile}
            monitored={monitored}
            hasFile={hasFile}
            isAvailable={isAvailable}
            status={status}
            width={posterWidth}
            detailedProgressBar={overviewOptions.detailedProgressBar}
            bottomRadius={false}
          />
        </div>

        <div className={styles.info} style={{ maxHeight: contentHeight }}>
          <div className={styles.titleRow}>
            <Link className={styles.title} to={link}>
              {title}
            </Link>

            <div className={styles.actions}>
              <span className={styles.externalLinks}>
                <Popover
                  anchor={<Icon name={icons.EXTERNAL_LINK} size={12} />}
                  title={translate('Links')}
                  body={<SceneDetailsLinks tmdbId={tmdbId} imdbId={imdbId} />}
                />
              </span>

              <SpinnerIconButton
                name={icons.REFRESH}
                title={translate('RefreshScene')}
                isSpinning={isRefreshingScene}
                onPress={onRefreshPress}
              />

              {overviewOptions.showSearchAction ? (
                <SpinnerIconButton
                  className={styles.actions}
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
            </div>
          </div>

          <div className={styles.details}>
            <Link className={styles.overview} to={link}>
              <TextTruncate
                line={Math.floor(
                  overviewHeight / (defaultFontSize * lineHeight)
                )}
                text={overview}
              />
            </Link>

            <SceneIndexOverviewInfo
              height={overviewHeight}
              monitored={monitored}
              qualityProfile={qualityProfile}
              studio={studio}
              sizeOnDisk={sizeOnDisk}
              added={added}
              path={path}
              sortKey={sortKey}
              {...overviewOptions}
            />
          </div>
        </div>
      </div>

      <EditSceneModalConnector
        isOpen={isEditSceneModalOpen}
        sceneId={sceneId}
        onModalClose={onEditSceneModalClose}
        onDeleteScenePress={onDeleteScenePress}
      />

      <DeleteSceneModal
        isOpen={isDeleteSceneModalOpen}
        sceneId={sceneId}
        onModalClose={onDeleteSceneModalClose}
      />
    </div>
  );
}

export default SceneIndexOverview;
