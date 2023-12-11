import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import TextTruncate from 'react-text-truncate';
import SceneFileEditorTable from 'SceneFile/Editor/SceneFileEditorTable';
import ExtraFileTable from 'SceneFile/Extras/ExtraFileTable';
import Alert from 'Components/Alert';
import FieldSet from 'Components/FieldSet';
import Icon from 'Components/Icon';
import InfoLabel from 'Components/InfoLabel';
import IconButton from 'Components/Link/IconButton';
import Marquee from 'Components/Marquee';
import Measure from 'Components/Measure';
import MonitorToggleButton from 'Components/MonitorToggleButton';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import PageToolbar from 'Components/Page/Toolbar/PageToolbar';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
import PageToolbarSection from 'Components/Page/Toolbar/PageToolbarSection';
import PageToolbarSeparator from 'Components/Page/Toolbar/PageToolbarSeparator';
import RottenTomatoRating from 'Components/RottenTomatoRating';
import TmdbRating from 'Components/TmdbRating';
import Popover from 'Components/Tooltip/Popover';
import Tooltip from 'Components/Tooltip/Tooltip';
import { icons, kinds, sizes, tooltipPositions } from 'Helpers/Props';
import InteractiveImportModal from 'InteractiveImport/InteractiveImportModal';
import OrganizePreviewModalConnector from 'Organize/OrganizePreviewModalConnector';
import DeleteSceneModal from 'Scene/Delete/DeleteSceneModal';
import EditSceneModalConnector from 'Scene/Edit/EditSceneModalConnector';
import SceneHistoryModal from 'Scene/History/SceneHistoryModal';
import ScenePoster from 'Scene/ScenePoster';
import SceneInteractiveSearchModalConnector from 'Scene/Search/SceneInteractiveSearchModalConnector';
import QualityProfileNameConnector from 'Settings/Profiles/Quality/QualityProfileNameConnector';
import fonts from 'Styles/Variables/fonts';
import * as keyCodes from 'Utilities/Constants/keyCodes';
import formatRuntime from 'Utilities/Date/formatRuntime';
import formatBytes from 'Utilities/Number/formatBytes';
import translate from 'Utilities/String/translate';
import SceneCollectionLabelConnector from './../SceneCollectionLabelConnector';
import SceneCastPostersConnector from './Credits/Cast/SceneCastPostersConnector';
import SceneDetailsLinks from './SceneDetailsLinks';
import SceneReleaseDates from './SceneReleaseDates';
import SceneStatusLabel from './SceneStatusLabel';
import SceneTagsConnector from './SceneTagsConnector';
import styles from './SceneDetails.css';

const defaultFontSize = parseInt(fonts.defaultFontSize);
const lineHeight = parseFloat(fonts.lineHeight);

function getFanartUrl(images) {
  return _.find(images, { coverType: 'fanart' })?.url;
}

class SceneDetails extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isOrganizeModalOpen: false,
      isEditSceneModalOpen: false,
      isDeleteSceneModalOpen: false,
      isInteractiveImportModalOpen: false,
      isInteractiveSearchModalOpen: false,
      isSceneHistoryModalOpen: false,
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

  onEditScenePress = () => {
    this.setState({ isEditSceneModalOpen: true });
  };

  onEditSceneModalClose = () => {
    this.setState({ isEditSceneModalOpen: false });
  };

  onInteractiveSearchPress = () => {
    this.setState({ isInteractiveSearchModalOpen: true });
  };

  onInteractiveSearchModalClose = () => {
    this.setState({ isInteractiveSearchModalOpen: false });
  };

  onDeleteScenePress = () => {
    this.setState({
      isEditSceneModalOpen: false,
      isDeleteSceneModalOpen: true
    });
  };

  onDeleteSceneModalClose = () => {
    this.setState({ isDeleteSceneModalOpen: false });
  };

  onSceneHistoryPress = () => {
    this.setState({ isSceneHistoryModalOpen: true });
  };

  onSceneHistoryModalClose = () => {
    this.setState({ isSceneHistoryModalOpen: false });
  };

  onMeasure = ({ height }) => {
    this.setState({ overviewHeight: height });
  };

  onTitleMeasure = ({ width }) => {
    this.setState({ titleWidth: width });
  };

  onKeyUp = (event) => {
    if (event.composedPath && event.composedPath().length === 4) {
      if (event.keyCode === keyCodes.LEFT_ARROW) {
        this.props.onGoToScene(this.props.previousScene.titleSlug);
      }
      if (event.keyCode === keyCodes.RIGHT_ARROW) {
        this.props.onGoToScene(this.props.nextScene.titleSlug);
      }
    }
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
      this.state.isEditSceneModalOpen ||
      this.state.isDeleteSceneModalOpen ||
      this.state.isInteractiveImportModalOpen ||
      this.state.isInteractiveSearchModalOpen ||
      this.state.isSceneHistoryModalOpen
    ) {
      return;
    }

    this._touchStart = touchStart;
  };

  onTouchEnd = (event) => {
    const touches = event.changedTouches;
    const currentTouch = touches[0].pageX;

    if (!this._touchStart) {
      return;
    }

    if (currentTouch > this._touchStart && currentTouch - this._touchStart > 100) {
      this.props.onGoToScene(this.props.previousScene.titleSlug);
    } else if (currentTouch < this._touchStart && this._touchStart - currentTouch > 100) {
      this.props.onGoToScene(this.props.nextScene.titleSlug);
    }

    this._touchStart = null;
  };

  onTouchCancel = (event) => {
    this._touchStart = null;
  };

  onTouchMove = (event) => {
    if (!this._touchStart) {
      return;
    }
  };

  //
  // Render

  render() {
    const {
      id,
      tmdbId,
      imdbId,
      title,
      year,
      releaseDate,
      runtime,
      certification,
      ratings,
      path,
      sizeOnDisk,
      qualityProfileId,
      monitored,
      studio,
      genres,
      collection,
      overview,
      isAvailable,
      images,
      tags,
      isSaving,
      isRefreshing,
      isSearching,
      isFetching,
      isSmallScreen,
      sceneFilesError,
      sceneCreditsError,
      extraFilesError,
      hasSceneFiles,
      previousScene,
      nextScene,
      onMonitorTogglePress,
      onRefreshPress,
      onSearchPress,
      queueItem,
      sceneRuntimeFormat,
      safeForWorkMode
    } = this.props;

    const {
      isOrganizeModalOpen,
      isEditSceneModalOpen,
      isDeleteSceneModalOpen,
      isInteractiveImportModalOpen,
      isInteractiveSearchModalOpen,
      isSceneHistoryModalOpen,
      overviewHeight,
      titleWidth
    } = this.state;

    const fanartUrl = getFanartUrl(images);
    const marqueeWidth = isSmallScreen ? titleWidth : (titleWidth - 150);

    return (
      <PageContent title={title}>
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
              label={translate('SearchScene')}
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
              isDisabled={!hasSceneFiles}
              onPress={this.onOrganizePress}
            />

            <PageToolbarButton
              label={translate('ManualImport')}
              iconName={icons.INTERACTIVE}
              onPress={this.onInteractiveImportPress}
            />

            <PageToolbarButton
              label={translate('History')}
              iconName={icons.HISTORY}
              onPress={this.onSceneHistoryPress}
            />

            <PageToolbarSeparator />

            <PageToolbarButton
              label={translate('Edit')}
              iconName={icons.EDIT}
              onPress={this.onEditScenePress}
            />

            <PageToolbarButton
              label={translate('Delete')}
              iconName={icons.DELETE}
              onPress={this.onDeleteScenePress}
            />
          </PageToolbarSection>
        </PageToolbar>

        <PageContentBody innerClassName={styles.innerContentBody}>
          <div className={styles.header}>
            <div
              className={styles.backdrop}
              style={
                fanartUrl ?
                  { backgroundImage: `url(${fanartUrl})` } :
                  null
              }
            >
              <div className={styles.backdropOverlay} />
            </div>

            <div className={styles.headerContent}>
              <ScenePoster
                blur={safeForWorkMode}
                className={styles.poster}
                images={images}
                size={250}
                lazy={false}
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

                    <div className={styles.sceneNavigationButtons}>
                      <IconButton
                        className={styles.sceneNavigationButton}
                        name={icons.ARROW_LEFT}
                        size={30}
                        title={translate('GoToInterp', [previousScene.title])}
                        to={`/scene/${previousScene.titleSlug}`}
                      />

                      <IconButton
                        className={styles.sceneNavigationButton}
                        name={icons.ARROW_RIGHT}
                        size={30}
                        title={translate('GoToInterp', [nextScene.title])}
                        to={`/scene/${nextScene.titleSlug}`}
                      />
                    </div>
                  </div>
                </Measure>

                <div className={styles.details}>
                  <div>
                    {
                      !!certification &&
                        <span className={styles.certification}>
                          {certification}
                        </span>
                    }

                    {
                      year > 0 &&
                        <span className={styles.year}>
                          <Popover
                            anchor={
                              year
                            }
                            title={translate('ReleaseDates')}
                            body={
                              <SceneReleaseDates
                                releaseDate={releaseDate}
                              />
                            }
                            position={tooltipPositions.BOTTOM}
                          />
                        </span>
                    }

                    {
                      !!runtime &&
                        <span className={styles.runtime}>
                          {formatRuntime(runtime, sceneRuntimeFormat)}
                        </span>
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
                            <SceneDetailsLinks
                              tmdbId={tmdbId}
                              imdbId={imdbId}
                            />
                          }
                          position={tooltipPositions.BOTTOM}
                        />
                      </span>
                    }

                    {
                      !!tags.length &&
                        <span>
                          <Tooltip
                            anchor={
                              <Icon
                                name={icons.TAGS}
                                size={20}
                              />
                            }
                            tooltip={
                              <SceneTagsConnector sceneId={id} />
                            }
                            position={tooltipPositions.BOTTOM}
                          />
                        </span>
                    }
                  </div>
                </div>

                <div className={styles.details}>
                  {
                    !!ratings.tmdb &&
                      <span className={styles.rating}>
                        <TmdbRating
                          ratings={ratings}
                          iconSize={20}
                        />
                      </span>
                  }
                  {
                    !!ratings.rottenTomatoes &&
                      <span className={styles.rating}>
                        <RottenTomatoRating
                          ratings={ratings}
                          iconSize={20}
                        />
                      </span>
                  }
                </div>

                <div className={styles.detailsLabels}>
                  <InfoLabel
                    className={styles.detailsInfoLabel}
                    title={translate('Path')}
                    size={sizes.LARGE}
                  >
                    <span className={styles.path}>
                      {path}
                    </span>
                  </InfoLabel>

                  <InfoLabel
                    className={styles.detailsInfoLabel}
                    title={translate('Status')}
                    kind={kinds.DELETE}
                    size={sizes.LARGE}
                  >
                    <span className={styles.statusName}>
                      <SceneStatusLabel
                        hasSceneFiles={hasSceneFiles}
                        monitored={monitored}
                        isAvailable={isAvailable}
                        queueItem={queueItem}
                      />
                    </span>
                  </InfoLabel>

                  <InfoLabel
                    className={styles.detailsInfoLabel}
                    title={translate('QualityProfile')}
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
                    title={translate('Size')}
                    size={sizes.LARGE}
                  >
                    <span className={styles.sizeOnDisk}>
                      {
                        formatBytes(sizeOnDisk || 0)
                      }
                    </span>
                  </InfoLabel>

                  {
                    !!collection &&
                      <InfoLabel
                        className={styles.detailsInfoLabel}
                        title={translate('Collection')}
                        size={sizes.LARGE}
                      >
                        <div className={styles.collection}>
                          <SceneCollectionLabelConnector
                            tmdbId={collection.tmdbId}
                          />
                        </div>
                      </InfoLabel>
                  }

                  {
                    !!studio && !isSmallScreen &&
                      <InfoLabel
                        className={styles.detailsInfoLabel}
                        title={translate('Studio')}
                        size={sizes.LARGE}
                      >
                        <span className={styles.studio}>
                          {studio}
                        </span>
                      </InfoLabel>
                  }

                  {
                    !!genres.length && !isSmallScreen &&
                      <InfoLabel
                        className={styles.detailsInfoLabel}
                        title={translate('Genres')}
                        size={sizes.LARGE}
                      >
                        <span className={styles.genres}>
                          {genres.join(', ')}
                        </span>
                      </InfoLabel>
                  }
                </div>

                <Measure onMeasure={this.onMeasure}>
                  <div className={styles.overview}>
                    <TextTruncate
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
              !isFetching && sceneFilesError ?
                <Alert kind={kinds.DANGER}>
                  {translate('LoadingSceneFilesFailed')}
                </Alert> :
                null
            }

            {
              !isFetching && sceneCreditsError ?
                <Alert kind={kinds.DANGER}>
                  {translate('LoadingSceneCreditsFailed')}
                </Alert> :
                null
            }

            {
              !isFetching && extraFilesError ?
                <Alert kind={kinds.DANGER}>
                  {translate('LoadingSceneExtraFilesFailed')}
                </Alert> :
                null
            }

            <FieldSet legend={translate('Files')}>
              <SceneFileEditorTable
                sceneId={id}
              />

              <ExtraFileTable
                sceneId={id}
              />
            </FieldSet>

            <FieldSet legend={translate('Cast')}>
              <SceneCastPostersConnector
                isSmallScreen={isSmallScreen}
              />
            </FieldSet>
          </div>

          <OrganizePreviewModalConnector
            isOpen={isOrganizeModalOpen}
            sceneId={id}
            onModalClose={this.onOrganizeModalClose}
          />

          <EditSceneModalConnector
            isOpen={isEditSceneModalOpen}
            sceneId={id}
            onModalClose={this.onEditSceneModalClose}
            onDeleteScenePress={this.onDeleteScenePress}
          />

          <SceneHistoryModal
            isOpen={isSceneHistoryModalOpen}
            sceneId={id}
            onModalClose={this.onSceneHistoryModalClose}
          />

          <DeleteSceneModal
            isOpen={isDeleteSceneModalOpen}
            sceneId={id}
            onModalClose={this.onDeleteSceneModalClose}
            nextSceneRelativePath={`/scene/${nextScene.titleSlug}`}
          />

          <InteractiveImportModal
            isOpen={isInteractiveImportModalOpen}
            sceneId={id}
            folder={path}
            allowSceneChange={false}
            showFilterExistingFiles={true}
            showImportMode={false}
            onModalClose={this.onInteractiveImportModalClose}
          />

          <SceneInteractiveSearchModalConnector
            isOpen={isInteractiveSearchModalOpen}
            sceneId={id}
            onModalClose={this.onInteractiveSearchModalClose}
          />
        </PageContentBody>
      </PageContent>
    );
  }
}

SceneDetails.propTypes = {
  id: PropTypes.number.isRequired,
  tmdbId: PropTypes.number.isRequired,
  imdbId: PropTypes.string,
  title: PropTypes.string.isRequired,
  year: PropTypes.number.isRequired,
  runtime: PropTypes.number.isRequired,
  certification: PropTypes.string,
  ratings: PropTypes.object.isRequired,
  path: PropTypes.string.isRequired,
  sizeOnDisk: PropTypes.number.isRequired,
  qualityProfileId: PropTypes.number.isRequired,
  monitored: PropTypes.bool.isRequired,
  status: PropTypes.string.isRequired,
  studio: PropTypes.string,
  genres: PropTypes.arrayOf(PropTypes.string).isRequired,
  collection: PropTypes.object,
  isAvailable: PropTypes.bool.isRequired,
  releaseDate: PropTypes.string,
  overview: PropTypes.string.isRequired,
  images: PropTypes.arrayOf(PropTypes.object).isRequired,
  alternateTitles: PropTypes.arrayOf(PropTypes.string).isRequired,
  tags: PropTypes.arrayOf(PropTypes.number).isRequired,
  isSaving: PropTypes.bool.isRequired,
  isRefreshing: PropTypes.bool.isRequired,
  isSearching: PropTypes.bool.isRequired,
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  isSmallScreen: PropTypes.bool.isRequired,
  isSidebarVisible: PropTypes.bool.isRequired,
  sceneFilesError: PropTypes.object,
  sceneCreditsError: PropTypes.object,
  extraFilesError: PropTypes.object,
  hasSceneFiles: PropTypes.bool.isRequired,
  previousScene: PropTypes.object.isRequired,
  nextScene: PropTypes.object.isRequired,
  onMonitorTogglePress: PropTypes.func.isRequired,
  onRefreshPress: PropTypes.func.isRequired,
  onSearchPress: PropTypes.func.isRequired,
  onGoToScene: PropTypes.func.isRequired,
  queueItem: PropTypes.object,
  sceneRuntimeFormat: PropTypes.string.isRequired,
  safeForWorkMode: PropTypes.bool
};

SceneDetails.defaultProps = {
  genres: [],
  tags: [],
  isSaving: false,
  sizeOnDisk: 0
};

export default SceneDetails;
