import PropTypes from 'prop-types';
import React, { Component } from 'react';
import TextTruncate from 'react-text-truncate';
import Alert from 'Components/Alert';
import FieldSet from 'Components/FieldSet';
import Icon from 'Components/Icon';
import InfoLabel from 'Components/InfoLabel';
import Marquee from 'Components/Marquee';
import Measure from 'Components/Measure';
import MonitorToggleButton from 'Components/MonitorToggleButton';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import PageToolbar from 'Components/Page/Toolbar/PageToolbar';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
import PageToolbarSection from 'Components/Page/Toolbar/PageToolbarSection';
import PageToolbarSeparator from 'Components/Page/Toolbar/PageToolbarSeparator';
import TmdbRating from 'Components/TmdbRating';
import Popover from 'Components/Tooltip/Popover';
import Tooltip from 'Components/Tooltip/Tooltip';
import { icons, kinds, sizes, tooltipPositions } from 'Helpers/Props';
import InteractiveImportModal from 'InteractiveImport/InteractiveImportModal';
import DeleteMovieModal from 'Movie/Delete/DeleteMovieModal';
import EditMovieModalConnector from 'Movie/Edit/EditMovieModalConnector';
import getMovieStatusDetails from 'Movie/getMovieStatusDetails';
import MovieHistoryModal from 'Movie/History/MovieHistoryModal';
import MovieImage from 'Movie/MovieImage';
import MovieInteractiveSearchModal from 'Movie/Search/MovieInteractiveSearchModal';
import MovieFileEditorTable from 'MovieFile/Editor/MovieFileEditorTable';
import ExtraFileTable from 'MovieFile/Extras/ExtraFileTable';
import OrganizePreviewModalConnector from 'Organize/OrganizePreviewModalConnector';
import QualityProfileNameConnector from 'Settings/Profiles/Quality/QualityProfileNameConnector';
import fonts from 'Styles/Variables/fonts';
import formatRuntime from 'Utilities/Date/formatRuntime';
import formatBytes from 'Utilities/Number/formatBytes';
import translate from 'Utilities/String/translate';
import MovieCastPostersConnector from './Credits/Cast/MovieCastPostersConnector';
import MovieDetailsLinks from './MovieDetailsLinks';
import MovieReleaseDates from './MovieReleaseDates';
import MovieStatusLabel from './MovieStatusLabel';
import MovieStudioLink from './MovieStudioLink';
import MovieTagsConnector from './MovieTagsConnector';
import styles from './MovieDetails.css';

const defaultFontSize = parseInt(fonts.defaultFontSize);
const lineHeight = parseFloat(fonts.lineHeight);
const screenshotStyle = {
  'object-fit': 'cover'
};

const posterPlaceholder =
  'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAKgAAAD3CAMAAAC+Te+kAAAAZlBMVEUvLi8vLy8vLzAvMDAwLy8wLzAwMDAwMDEwMTExMDAxMDExMTExMTIxMjIyMjIyMjMyMzMzMjMzMzMzMzQzNDQ0NDQ0NDU0NTU1NTU1NTY1NjY2NTY2NjY2Njc2Nzc3Njc3Nzc3NziHChLWAAAAAWJLR0QAiAUdSAAAAAlwSFlzAAALEwAACxMBAJqcGAAAAAd0SU1FB+MKCgEdHeShUbsAAALZSURBVHja7dxNcuwgDEZR1qAVmP1vMrNUJe91GfTzCSpXo575lAymjYWGXRIDKFCgQIECBQoUKFCgQIECBQoUKFCgQIECBQoUKFCgQIECBQoUKNA/AZ3fcTR0/owjofNDnAadnwPoPnS+xTXQeQZ0rkQ/dC4H0Gzo7ITO3bgGOnug/2PcAF3Mczt0fUj0QncG7znQBupw3PkWqh8qpkagpnyqjuArkkxaC02kRqGypCZANVYFdJZCdy9WTRVB5znQ6qTmjFFBWnOhdg20Lqnp0CpqAbRmAJRAK5JaA32zngTNvv910OSkVkJTs1oLtWugeTkNQZ/nkT2rotBHldUwNE6VQTVWGTQ6AHKggqGaBS23JkKf0hUgE1qa01Ro5fzPhoapR0HtCGg4q0poSCqFRgaAFhqxqqEr1EOgmdJaqHdaHQq1I6CunPZAHdY2aIJUBN2V9kE3H1Wd0BXrNVA7BLpgdUCtALo8pZqhdgd0Z6OyE7q1pdoH3dv7tS7o7iZ1E3R/N70Huuz795cQao65vvkqooT+vEgDdPcbj2s3zxTv9Qt/7cuhdgfUo2yAOplyqNuphfqZSqhFmEJo0HkcdPZCo0rRymRxpwSawHR+YtyBZihfvi+nQO0OqCmcYahGqYPGS4qCUJkzBpUpJdCkordyaFZxXi1UUpaZAJ2XQFOLh8ug2XXjVdD0+vYiqLIO3w1VH8EogtoxUPnpGxe04zyTA1p57i4T2nTmbnnnUuLMg1afYE2C1h+1zYEKjlknQLtPg9tb3YzU+dL054qOBb8cvcz3DlqBZhUmhdrnKo9j+pR0rkN5UHkznZHPtJIYN2TTCe1poTUyk9nWPO0bt8Ys7Ug34mlUMONtPUXMaEdXnXN1MnUzN2Z9q3Lr8XQN1DaLQJpXpiamZwltYdIUHShQoECBAgUKFChQoECBAgUKFChQoECBAgUKFChQoECBAgUKFCjQ+vgCff/mEp/vtiIAAAAASUVORK5CYII=';

function getFanartUrl(images) {
  const image = images.find((img) => img.coverType === 'fanart');
  return image?.url ?? image?.remoteUrl;
}

class MovieDetails extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isOrganizeModalOpen: false,
      isEditMovieModalOpen: false,
      isDeleteMovieModalOpen: false,
      isInteractiveImportModalOpen: false,
      isInteractiveSearchModalOpen: false,
      isMovieHistoryModalOpen: false,
      overviewHeight: 0,
      titleWidth: 0
    };
  }

  componentDidMount() {
    window.addEventListener('touchstart', this.onTouchStart);
    window.addEventListener('touchend', this.onTouchEnd);
    window.addEventListener('touchcancel', this.onTouchCancel);
    window.addEventListener('touchmove', this.onTouchMove);
    window.addEventListener('keyup', this.onKeyUp);
  }

  componentWillUnmount() {
    window.removeEventListener('touchstart', this.onTouchStart);
    window.removeEventListener('touchend', this.onTouchEnd);
    window.removeEventListener('touchcancel', this.onTouchCancel);
    window.removeEventListener('touchmove', this.onTouchMove);
    window.removeEventListener('keyup', this.onKeyUp);
  }

  //
  // Listeners

  onOrganizePress = () => {
    this.setState({ isOrganizeModalOpen: true });
  };

  onOrganizeModalClose = () => {
    this.setState({ isOrganizeModalOpen: false });
  };

  onInteractiveImportPress = () => {
    this.setState({ isInteractiveImportModalOpen: true });
  };

  onInteractiveImportModalClose = () => {
    this.setState({ isInteractiveImportModalOpen: false });
  };

  onEditMoviePress = () => {
    this.setState({ isEditMovieModalOpen: true });
  };

  onEditMovieModalClose = () => {
    this.setState({ isEditMovieModalOpen: false });
  };

  onInteractiveSearchPress = () => {
    this.setState({ isInteractiveSearchModalOpen: true });
  };

  onInteractiveSearchModalClose = () => {
    this.setState({ isInteractiveSearchModalOpen: false });
  };

  onDeleteMoviePress = () => {
    this.setState({
      isEditMovieModalOpen: false,
      isDeleteMovieModalOpen: true
    });
  };

  onDeleteMovieModalClose = () => {
    this.setState({ isDeleteMovieModalOpen: false });
  };

  onMovieHistoryPress = () => {
    this.setState({ isMovieHistoryModalOpen: true });
  };

  onMovieHistoryModalClose = () => {
    this.setState({ isMovieHistoryModalOpen: false });
  };

  onMeasure = ({ height }) => {
    this.setState({ overviewHeight: height });
  };

  onTitleMeasure = ({ width }) => {
    this.setState({ titleWidth: width });
  };

  onTouchStart = (event) => {
    const touches = event.touches;
    const touchStart = touches[0].pageX;
    const touchY = touches[0].pageY;

    // Only change when swipe is on header, we need horizontal scroll on tables
    if (touchY > 470) {
      return;
    }

    if (touches.length !== 1) {
      return;
    }

    if (
      touchStart < 50 ||
      this.props.isSidebarVisible ||
      this.state.isOrganizeModalOpen ||
      this.state.isEditMovieModalOpen ||
      this.state.isDeleteMovieModalOpen ||
      this.state.isInteractiveImportModalOpen ||
      this.state.isInteractiveSearchModalOpen ||
      this.state.isMovieHistoryModalOpen
    ) {
      return;
    }

    this._touchStart = touchStart;
  };

  //
  // Render

  render() {
    const {
      id,
      tmdbId,
      foreignId,
      stashId,
      title,
      code,
      year,
      releaseDate,
      runtime,
      certification,
      ratings,
      path,
      statistics,
      qualityProfileId,
      monitored,
      studioTitle,
      studioForeignId,
      genres,
      overview,
      status,
      studio,
      isAvailable,
      images,
      tags,
      itemType,
      isSaving,
      isRefreshing,
      isSearching,
      isFetching,
      isSmallScreen,
      movieFilesError,
      extraFilesError,
      hasMovieFiles,
      onMonitorTogglePress,
      onRefreshPress,
      onSearchPress,
      queueItem,
      movieRuntimeFormat,
      safeForWorkMode
    } = this.props;

    const {
      sizeOnDisk = 0
    } = statistics;

    const {
      isOrganizeModalOpen,
      isEditMovieModalOpen,
      isDeleteMovieModalOpen,
      isInteractiveImportModalOpen,
      isInteractiveSearchModalOpen,
      isMovieHistoryModalOpen,
      overviewHeight,
      titleWidth
    } = this.state;

    const statusDetails = getMovieStatusDetails(status);

    const fanartUrl = getFanartUrl(images);
    const marqueeWidth = isSmallScreen ? titleWidth : (titleWidth - 150);

    const titleWithYear = `${title}${year > 0 ? ` (${year})` : ''}`;

    return (
      <PageContent title={titleWithYear}>
        <PageToolbar>
          <PageToolbarSection>
            <PageToolbarButton
              label={translate('RefreshAndScan')}
              iconName={icons.REFRESH}
              spinningName={icons.REFRESH}
              title={translate('RefreshInformationAndScanDisk')}
              isSpinning={isRefreshing}
              onPress={onRefreshPress}
            />

            <PageToolbarButton
              label={translate('SearchMovie')}
              iconName={icons.SEARCH}
              isSpinning={isSearching}
              title={undefined}
              onPress={onSearchPress}
            />

            <PageToolbarButton
              label={translate('InteractiveSearch')}
              iconName={icons.INTERACTIVE}
              isSpinning={isSearching}
              title={undefined}
              onPress={this.onInteractiveSearchPress}
            />

            <PageToolbarSeparator />

            <PageToolbarButton
              label={translate('PreviewRename')}
              iconName={icons.ORGANIZE}
              isDisabled={!hasMovieFiles}
              onPress={this.onOrganizePress}
            />

            <PageToolbarButton
              label={translate('ManageFiles')}
              iconName={icons.MOVIE_FILE}
              onPress={this.onInteractiveImportPress}
            />

            <PageToolbarButton
              label={translate('History')}
              iconName={icons.HISTORY}
              onPress={this.onMovieHistoryPress}
            />

            <PageToolbarSeparator />

            <PageToolbarButton
              label={translate('Edit')}
              iconName={icons.EDIT}
              onPress={this.onEditMoviePress}
            />

            <PageToolbarButton
              label={translate('Delete')}
              iconName={icons.DELETE}
              onPress={this.onDeleteMoviePress}
            />
          </PageToolbarSection>
        </PageToolbar>

        <PageContentBody innerClassName={styles.innerContentBody}>
          <div className={itemType === 'movie' ? styles.header : styles.sceneHeader}>
            <div
              className={styles.backdrop}
              style={
                fanartUrl && !safeForWorkMode ?
                  { backgroundImage: `url(${fanartUrl})` } :
                  null
              }
            >
              <div className={styles.backdropOverlay} />
            </div>

            <div className={styles.headerContent}>
              <MovieImage style={screenshotStyle}
                safeForWorkMode={safeForWorkMode}
                className={itemType === 'movie' ? styles.poster : styles.screenShot}
                coverType={itemType === 'movie' ? 'poster' : 'screenshot'}
                images={images}
                size={500}
                lazy={false}
                placeholder={posterPlaceholder}
              />

              <div className={styles.info}>
                <Measure onMeasure={this.onTitleMeasure}>
                  <div className={styles.titleRow}>
                    <div className={styles.titleContainer}>
                      <div className={styles.toggleMonitoredContainer}>
                        <MonitorToggleButton
                          className={styles.monitorToggleButton}
                          monitored={monitored}
                          isSaving={isSaving}
                          size={40}
                          onPress={onMonitorTogglePress}
                        />
                      </div>

                      <div className={styles.title} style={{ width: marqueeWidth }}>
                        <Marquee text={title} />
                      </div>
                    </div>
                  </div>
                </Measure>

                <div className={styles.details}>
                  <div>
                    {
                      certification ?
                        <span className={styles.certification} title={translate('Certification')}>
                          {certification}
                        </span> :
                        null
                    }

                    <span className={styles.year}>
                      <Popover
                        anchor={
                          year > 0 ? (
                            year
                          ) : (
                            <Icon
                              name={icons.WARNING}
                              kind={kinds.WARNING}
                              size={20}
                            />
                          )
                        }
                        title={translate('ReleaseDates')}
                        body={
                          <MovieReleaseDates
                            foreignId={foreignId}
                            itemType={itemType}
                            releaseDate={releaseDate}
                          />
                        }
                        position={tooltipPositions.BOTTOM}
                      />
                    </span>

                    {studioTitle ?
                      <span className={styles.studio}>
                        <MovieStudioLink foreignId={studioForeignId} studioTitle={studioTitle} />
                      </span> :
                      null
                    }

                    {runtime ?
                      <span className={styles.runtime} title={translate('Runtime')}>
                        {formatRuntime(runtime, movieRuntimeFormat)}
                      </span> :
                      null
                    }

                    {
                      <span className={styles.links}>
                        <Tooltip
                          anchor={
                            <Icon
                              name={icons.EXTERNAL_LINK}
                              size={20}
                            />
                          }
                          tooltip={
                            <MovieDetailsLinks
                              tmdbId={tmdbId}
                              stashId={stashId}
                            />
                          }
                          position={tooltipPositions.BOTTOM}
                        />
                      </span>
                    }

                    {!!tags.length &&
                      <span>
                        <Tooltip
                          anchor={
                            <Icon
                              name={icons.TAGS}
                              size={20}
                            />
                          }
                          tooltip={
                            <MovieTagsConnector movieId={id} />
                          }
                          position={tooltipPositions.BOTTOM}
                        />
                      </span>
                    }
                  </div>
                </div>

                <div className={styles.details}>
                  {!!ratings.tmdb &&
                    <span className={styles.rating}>
                      <TmdbRating
                        ratings={ratings}
                        iconSize={20}
                      />
                    </span>
                  }
                </div>

                <div className={styles.detailsLabels}>
                  <InfoLabel
                    className={styles.detailsInfoLabel}
                    name={translate('Path')}
                    size={sizes.LARGE}
                  >
                    <span className={styles.path}>
                      {path}
                    </span>
                  </InfoLabel>

                  <InfoLabel
                    className={styles.detailsInfoLabel}
                    name={translate('Status')}
                    title={statusDetails.message}
                    kind={kinds.DELETE}
                    size={sizes.LARGE}
                  >
                    <span className={styles.statusName}>
                      <MovieStatusLabel
                        status={status}
                        hasMovieFiles={hasMovieFiles}
                        monitored={monitored}
                        isAvailable={isAvailable}
                        queueItem={queueItem}
                      />
                    </span>
                  </InfoLabel>

                  <InfoLabel
                    className={styles.detailsInfoLabel}
                    name={translate('QualityProfile')}
                    size={sizes.LARGE}
                  >
                    <span className={styles.qualityProfileName}>
                      {
                        <QualityProfileNameConnector
                          qualityProfileId={qualityProfileId}
                        />
                      }
                    </span>
                  </InfoLabel>

                  <InfoLabel
                    className={styles.detailsInfoLabel}
                    name={translate('Size')}
                    size={sizes.LARGE}
                  >
                    <span className={styles.sizeOnDisk}>
                      {formatBytes(sizeOnDisk)}
                    </span>
                  </InfoLabel>

                  {!!code && !!code.length &&
                    <InfoLabel
                      className={styles.detailsInfoLabel}
                      title={translate('Code')}
                      size={sizes.LARGE}
                    >
                      <span className={styles.code}>
                        {code}
                      </span>
                    </InfoLabel>
                  }

                  {
                    studio && !isSmallScreen ?
                      <InfoLabel
                        className={styles.detailsInfoLabel}
                        name={translate('Studio')}
                        size={sizes.LARGE}
                      >
                        <span className={styles.studio}>
                          {studio}
                        </span>
                      </InfoLabel> :
                      null
                  }

                  {!!genres.length && !isSmallScreen &&
                    <InfoLabel
                      className={styles.detailsInfoLabel}
                      title={translate('Genres')}
                      size={sizes.LARGE}
                    >
                      <span className={styles.genres}>
                        {genres.slice(0, 3).join(', ')}
                      </span>
                    </InfoLabel>
                  }
                </div>

                <Measure onMeasure={this.onMeasure}>
                  <div className={styles.overview}>
                    <TextTruncate className={styles.overview}
                      line={Math.floor(overviewHeight / (defaultFontSize * lineHeight))}
                      text={overview}
                    />
                  </div>
                </Measure>
              </div>
            </div>
          </div>

          <div className={styles.contentContainer}>
            {
              !isFetching && movieFilesError ?
                <Alert kind={kinds.DANGER}>
                  {translate('LoadingMovieFilesFailed')}
                </Alert> :
                null
            }

            {
              !isFetching && extraFilesError ?
                <Alert kind={kinds.DANGER}>
                  {translate('LoadingMovieExtraFilesFailed')}
                </Alert> :
                null
            }

            <FieldSet legend={translate('Files')}>
              <MovieFileEditorTable
                movieId={id}
              />

              <ExtraFileTable
                movieId={id}
              />
            </FieldSet>

            {itemType === 'scene' ?
              <FieldSet legend={translate('Cast')}>
                <MovieCastPostersConnector
                  movieId={id}
                  isSmallScreen={isSmallScreen}
                />
              </FieldSet> :
              null
            }
          </div>

          <OrganizePreviewModalConnector
            isOpen={isOrganizeModalOpen}
            movieId={id}
            onModalClose={this.onOrganizeModalClose}
          />

          <EditMovieModalConnector
            isOpen={isEditMovieModalOpen}
            movieId={id}
            onModalClose={this.onEditMovieModalClose}
            onDeleteMoviePress={this.onDeleteMoviePress}
          />

          <MovieHistoryModal
            isOpen={isMovieHistoryModalOpen}
            movieId={id}
            onModalClose={this.onMovieHistoryModalClose}
          />

          <DeleteMovieModal
            isOpen={isDeleteMovieModalOpen}
            movieId={id}
            onModalClose={this.onDeleteMovieModalClose}
          />

          <InteractiveImportModal
            isOpen={isInteractiveImportModalOpen}
            movieId={id}
            modalTitle={translate('ManageFiles')}
            folder={path}
            allowMovieChange={false}
            showFilterExistingFiles={true}
            showImportMode={false}
            onModalClose={this.onInteractiveImportModalClose}
          />

          <MovieInteractiveSearchModal
            isOpen={isInteractiveSearchModalOpen}
            movieId={id}
            movieTitle={title}
            onModalClose={this.onInteractiveSearchModalClose}
          />
        </PageContentBody>
      </PageContent>
    );
  }
}

MovieDetails.propTypes = {
  id: PropTypes.number.isRequired,
  tmdbId: PropTypes.number.isRequired,
  foreignId: PropTypes.string,
  stashId: PropTypes.string,
  title: PropTypes.string.isRequired,
  code: PropTypes.string,
  year: PropTypes.number.isRequired,
  runtime: PropTypes.number.isRequired,
  certification: PropTypes.string,
  ratings: PropTypes.object.isRequired,
  path: PropTypes.string.isRequired,
  statistics: PropTypes.object.isRequired,
  qualityProfileId: PropTypes.number.isRequired,
  monitored: PropTypes.bool.isRequired,
  status: PropTypes.string.isRequired,
  studio: PropTypes.string,
  studioTitle: PropTypes.string,
  studioForeignId: PropTypes.string,
  genres: PropTypes.arrayOf(PropTypes.string).isRequired,
  collection: PropTypes.object,
  isAvailable: PropTypes.bool.isRequired,
  releaseDate: PropTypes.string,
  overview: PropTypes.string.isRequired,
  images: PropTypes.arrayOf(PropTypes.object).isRequired,
  alternateTitles: PropTypes.arrayOf(PropTypes.string).isRequired,
  tags: PropTypes.arrayOf(PropTypes.number).isRequired,
  itemType: PropTypes.string.isRequired,
  isSaving: PropTypes.bool.isRequired,
  isRefreshing: PropTypes.bool.isRequired,
  isSearching: PropTypes.bool.isRequired,
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  isSmallScreen: PropTypes.bool.isRequired,
  isSidebarVisible: PropTypes.bool.isRequired,
  movieFilesError: PropTypes.object,
  extraFilesError: PropTypes.object,
  hasMovieFiles: PropTypes.bool.isRequired,
  onMonitorTogglePress: PropTypes.func.isRequired,
  onRefreshPress: PropTypes.func.isRequired,
  onSearchPress: PropTypes.func.isRequired,
  onGoToMovie: PropTypes.func.isRequired,
  queueItem: PropTypes.object,
  movieRuntimeFormat: PropTypes.string.isRequired,
  safeForWorkMode: PropTypes.bool
};

MovieDetails.defaultProps = {
  genres: [],
  statistics: {},
  tags: [],
  isSaving: false
};

export default MovieDetails;
