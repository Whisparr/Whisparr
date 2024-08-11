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
import DeleteMovieModal from 'Movie/Delete/DeleteMovieModal';
import MovieDetailsLinks from 'Movie/Details/MovieDetailsLinks';
import EditMovieModalConnector from 'Movie/Edit/EditMovieModalConnector';
import createMovieIndexItemSelector from 'Movie/Index/createMovieIndexItemSelector';
import MovieTitleLink from 'Movie/MovieTitleLink';
import { executeCommand } from 'Store/Actions/commandActions';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import { SelectStateInputProps } from 'typings/props';
import formatRuntime from 'Utilities/Date/formatRuntime';
import formatBytes from 'Utilities/Number/formatBytes';
import translate from 'Utilities/String/translate';
import MovieIndexProgressBar from '../ProgressBar/MovieIndexProgressBar';
import MovieStatusCell from './MovieStatusCell';
import selectTableOptions from './selectTableOptions';
import styles from './MovieIndexRow.css';

interface MovieIndexRowProps {
  movieId: number;
  sortKey: string;
  columns: Column[];
  isSelectMode: boolean;
}

function MovieIndexRow(props: MovieIndexRowProps) {
  const { movieId, columns, isSelectMode } = props;

  const { movie, qualityProfile, isRefreshingMovie, isSearchingMovie } =
    useSelector(createMovieIndexItemSelector(props.movieId));

  const { showSearchAction } = useSelector(selectTableOptions);

  const { movieRuntimeFormat } = useSelector(createUISettingsSelector());

  const {
    monitored,
    foreignId,
    title,
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
    isAvailable,
    hasFile,
    movieFile,
    isSaving = false,
  } = movie;

  const dispatch = useDispatch();
  const [isEditMovieModalOpen, setIsEditMovieModalOpen] = useState(false);
  const [isDeleteMovieModalOpen, setIsDeleteMovieModalOpen] = useState(false);
  const [selectState, selectDispatch] = useSelect();

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
          id={movieId}
          isSelected={selectState.selectedState[movieId]}
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
            <MovieStatusCell
              key={name}
              className={styles[name]}
              movieId={movieId}
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
              <MovieTitleLink foreignId={foreignId} title={title} />
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

        if (name === 'movieStatus') {
          return (
            <VirtualTableRowCell key={name} className={styles[name]}>
              <MovieIndexProgressBar
                movieId={movieId}
                movieFile={movieFile}
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
                  tooltip={<MovieDetailsLinks tmdbId={tmdbId} />}
                  canFlip={true}
                  kind={kinds.INVERSE}
                />
              </span>

              <SpinnerIconButton
                name={icons.REFRESH}
                title={translate('RefreshMovie')}
                isSpinning={isRefreshingMovie}
                onPress={onRefreshPress}
              />

              {showSearchAction ? (
                <SpinnerIconButton
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
            </VirtualTableRowCell>
          );
        }

        return null;
      })}

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
    </>
  );
}

export default MovieIndexRow;
