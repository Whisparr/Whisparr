import React, { useCallback, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { useSelect } from 'App/SelectContext';
import { MOVIE_SEARCH, REFRESH_MOVIE } from 'Commands/commandNames';
import Icon from 'Components/Icon';
import IconButton from 'Components/Link/IconButton';
import SpinnerIconButton from 'Components/Link/SpinnerIconButton';
import RottenTomatoRating from 'Components/RottenTomatoRating';
import RelativeDateCellConnector from 'Components/Table/Cells/RelativeDateCellConnector';
import VirtualTableRowCell from 'Components/Table/Cells/VirtualTableRowCell';
import VirtualTableSelectCell from 'Components/Table/Cells/VirtualTableSelectCell';
import Column from 'Components/Table/Column';
import TagListConnector from 'Components/TagListConnector';
import TmdbRating from 'Components/TmdbRating';
import Tooltip from 'Components/Tooltip/Tooltip';
import { icons, kinds } from 'Helpers/Props';
import EditMovieModalConnector from 'Movie/Edit/EditMovieModalConnector';
import DeleteSceneModal from 'Scene/Delete/DeleteSceneModal';
import SceneDetailsLinks from 'Scene/Details/SceneDetailsLinks';
import createSceneIndexItemSelector from 'Scene/Index/createSceneIndexItemSelector';
import SceneTitleLink from 'Scene/SceneTitleLink';
import { executeCommand } from 'Store/Actions/commandActions';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import { SelectStateInputProps } from 'typings/props';
import formatRuntime from 'Utilities/Date/formatRuntime';
import formatBytes from 'Utilities/Number/formatBytes';
import translate from 'Utilities/String/translate';
import SceneIndexProgressBar from '../ProgressBar/SceneIndexProgressBar';
import SceneStatusCell from './SceneStatusCell';
import selectTableOptions from './selectTableOptions';
import styles from './SceneIndexRow.css';

interface SceneIndexRowProps {
  sceneId: number;
  sortKey: string;
  columns: Column[];
  isSelectMode: boolean;
}

function SceneIndexRow(props: SceneIndexRowProps) {
  const { sceneId, columns, isSelectMode } = props;

  const { scene, qualityProfile, isRefreshingScene, isSearchingScene } =
    useSelector(createSceneIndexItemSelector(props.sceneId));

  const { showSearchAction } = useSelector(selectTableOptions);

  const { movieRuntimeFormat } = useSelector(createUISettingsSelector());

  const {
    monitored,
    titleSlug,
    title,
    collection,
    studioTitle,
    status,
    originalLanguage,
    added,
    year,
    releaseDate,
    runtime,
    path,
    sizeOnDisk,
    genres = [],
    ratings,
    tags = [],
    tmdbId,
    imdbId,
    isAvailable,
    hasFile,
    movieFile,
    isSaving = false,
  } = scene;

  const dispatch = useDispatch();
  const [isEditSceneModalOpen, setIsEditSceneModalOpen] = useState(false);
  const [isDeleteSceneModalOpen, setIsDeleteSceneModalOpen] = useState(false);
  const [selectState, selectDispatch] = useSelect();

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

  const onSelectedChange = useCallback(
    ({ id, value, shiftKey }: SelectStateInputProps) => {
      selectDispatch({
        type: 'toggleSelected',
        id,
        isSelected: value,
        shiftKey,
      });
    },
    [selectDispatch]
  );

  return (
    <>
      {isSelectMode ? (
        <VirtualTableSelectCell
          id={sceneId}
          isSelected={selectState.selectedState[sceneId]}
          isDisabled={false}
          onSelectedChange={onSelectedChange}
        />
      ) : null}

      {columns.map((column) => {
        const { name, isVisible } = column;

        if (!isVisible) {
          return null;
        }

        if (name === 'status') {
          return (
            <SceneStatusCell
              key={name}
              className={styles[name]}
              sceneId={sceneId}
              monitored={monitored}
              status={status}
              isSelectMode={isSelectMode}
              isSaving={isSaving}
              component={VirtualTableRowCell}
            />
          );
        }

        if (name === 'sortTitle') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              <SceneTitleLink titleSlug={titleSlug} title={title} />
            </VirtualTableRowCell>
          );
        }

        if (name === 'collection') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              {collection ? collection.title : null}
            </VirtualTableRowCell>
          );
        }

        if (name === 'studio') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              {studioTitle}
            </VirtualTableRowCell>
          );
        }

        if (name === 'originalLanguage') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              {originalLanguage.name}
            </VirtualTableRowCell>
          );
        }

        if (name === 'qualityProfileId') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              {qualityProfile?.name ?? ''}
            </VirtualTableRowCell>
          );
        }

        if (name === 'added') {
          return (
            // eslint-disable-next-line @typescript-eslint/ban-ts-comment
            // @ts-ignore ts(2739)
            <RelativeDateCellConnector
              key={name}
              className={styles[name]}
              date={added}
              component={VirtualTableRowCell}
            />
          );
        }

        if (name === 'year') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              {year}
            </VirtualTableRowCell>
          );
        }

        if (name === 'releaseDate') {
          return (
            // eslint-disable-next-line @typescript-eslint/ban-ts-comment
            // @ts-ignore ts(2739)
            <RelativeDateCellConnector
              key={name}
              className={styles[name]}
              date={releaseDate}
              component={VirtualTableRowCell}
            />
          );
        }

        if (name === 'runtime') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              {formatRuntime(runtime, movieRuntimeFormat)}
            </VirtualTableRowCell>
          );
        }

        if (name === 'path') {
          return (
            <VirtualTableRowCell
              key={name}
              className={styles[name]}
              title={path}
            >
              {path}
            </VirtualTableRowCell>
          );
        }

        if (name === 'sizeOnDisk') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              {formatBytes(sizeOnDisk)}
            </VirtualTableRowCell>
          );
        }

        if (name === 'genres') {
          const joinedGenres = genres.join(', ');

          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              <span title={joinedGenres}>{joinedGenres}</span>
            </VirtualTableRowCell>
          );
        }

        if (name === 'sceneStatus') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              <SceneIndexProgressBar
                sceneId={sceneId}
                sceneFile={movieFile}
                monitored={monitored}
                hasFile={hasFile}
                isAvailable={isAvailable}
                status={status}
                width={125}
                detailedProgressBar={true}
                bottomRadius={false}
                isStandAlone={true}
              />
            </VirtualTableRowCell>
          );
        }

        if (name === 'tmdbRating') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              <TmdbRating ratings={ratings} />
            </VirtualTableRowCell>
          );
        }

        if (name === 'rottenTomatoesRating') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              <RottenTomatoRating ratings={ratings} />
            </VirtualTableRowCell>
          );
        }

        if (name === 'tags') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              <TagListConnector tags={tags} />
            </VirtualTableRowCell>
          );
        }

        if (name === 'actions') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              <span className={styles.externalLinks}>
                <Tooltip
                  anchor={<Icon name={icons.EXTERNAL_LINK} size={12} />}
                  tooltip={
                    <SceneDetailsLinks tmdbId={tmdbId} imdbId={imdbId} />
                  }
                  canFlip={true}
                  kind={kinds.INVERSE}
                />
              </span>

              <SpinnerIconButton
                name={icons.REFRESH}
                title={translate('RefreshScene')}
                isSpinning={isRefreshingScene}
                onPress={onRefreshPress}
              />

              {showSearchAction ? (
                <SpinnerIconButton
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
            </VirtualTableRowCell>
          );
        }

        return null;
      })}

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
    </>
  );
}

export default SceneIndexRow;
