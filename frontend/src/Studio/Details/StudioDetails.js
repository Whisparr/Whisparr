import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Alert from 'Components/Alert';
import FieldSet from 'Components/FieldSet';
import Icon from 'Components/Icon';
import Label from 'Components/Label';
import IconButton from 'Components/Link/IconButton';
import Measure from 'Components/Measure';
import MonitorToggleButton from 'Components/MonitorToggleButton';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import PageToolbar from 'Components/Page/Toolbar/PageToolbar';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
import PageToolbarSection from 'Components/Page/Toolbar/PageToolbarSection';
import PageToolbarSeparator from 'Components/Page/Toolbar/PageToolbarSeparator';
import Tooltip from 'Components/Tooltip/Tooltip';
import { icons, kinds, sizes, tooltipPositions } from 'Helpers/Props';
import MovieInteractiveSearchModalConnector from 'Movie/Search/MovieInteractiveSearchModalConnector';
import QualityProfileNameConnector from 'Settings/Profiles/Quality/QualityProfileNameConnector';
import EditStudioModalConnector from 'Studio/Edit/EditStudioModalConnector';
import StudioLogo from 'Studio/StudioLogo';
import * as keyCodes from 'Utilities/Constants/keyCodes';
import formatBytes from 'Utilities/Number/formatBytes';
import translate from 'Utilities/String/translate';
import selectAll from 'Utilities/Table/selectAll';
import toggleSelected from 'Utilities/Table/toggleSelected';
import StudioDetailsLinks from './StudioDetailsLinks';
import StudioDetailsYearConnector from './StudioDetailsYearConnector';
import StudioTagsConnector from './StudioTagsConnector';
import styles from './StudioDetails.css';

function getFanartUrl(images) {
  return _.find(images, { coverType: 'fanart' })?.url;
}

function getExpandedState(newState) {
  return {
    allExpanded: newState.allSelected,
    allCollapsed: newState.allUnselected,
    expandedState: newState.selectedState
  };
}

class StudioDetails extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isEditMovieModalOpen: false,
      isInteractiveSearchModalOpen: false,
      allExpanded: false,
      allCollapsed: false,
      expandedState: {},
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

  onTitleMeasure = ({ width }) => {
    this.setState({ titleWidth: width });
  };

  onKeyUp = (event) => {
    if (event.composedPath && event.composedPath().length === 4) {
      if (event.keyCode === keyCodes.LEFT_ARROW) {
        this.props.onGoToStudio(this.props.previousStudio.foreignId);
      }
      if (event.keyCode === keyCodes.RIGHT_ARROW) {
        this.props.onGoToStudio(this.props.nextStudio.foreignId);
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
      this.state.isEditMovieModalOpen ||
      this.state.isInteractiveSearchModalOpen
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
      this.props.onGoToStudio(this.props.previousStudio.foreignId);
    } else if (currentTouch < this._touchStart && this._touchStart - currentTouch > 100) {
      this.props.onGoToStudio(this.props.nextStudio.foreignId);
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

  onExpandAllPress = () => {
    const {
      allExpanded,
      expandedState
    } = this.state;

    this.setState(getExpandedState(selectAll(expandedState, !allExpanded)));
  };

  onExpandPress = (studioId, isExpanded) => {
    this.setState((state) => {
      const convertedState = {
        allSelected: state.allExpanded,
        allUnselected: state.allCollapsed,
        selectedState: state.expandedState
      };

      const newState = toggleSelected(convertedState, [], studioId, isExpanded, false);

      return getExpandedState(newState);
    });
  };

  //
  // Render

  render() {
    const {
      id,
      foreignId,
      title,
      rootFolderPath,
      sizeOnDisk,
      qualityProfileId,
      monitored,
      years,
      genres,
      images,
      tags,
      isSaving,
      isRefreshing,
      isSearching,
      isFetching,
      isPopulated,
      isSmallScreen,
      hasMovies,
      hasScenes,
      previousStudio,
      nextStudio,
      onMonitorTogglePress,
      onRefreshPress,
      onSearchPress,
      moviesError,
      safeForWorkMode
    } = this.props;

    const {
      isEditMovieModalOpen,
      isInteractiveSearchModalOpen,
      expandedState
    } = this.state;

    const runningYears = `${years[0]}-${years.slice(-1)}`;

    const fanartUrl = getFanartUrl(images);

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
              label={translate('SearchStudio')}
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
              label={translate('Edit')}
              iconName={icons.EDIT}
              onPress={this.onEditMoviePress}
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
              <StudioLogo
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

                      <div className={styles.title}>
                        {title}
                      </div>
                    </div>

                    <div className={styles.movieNavigationButtons}>
                      <IconButton
                        className={styles.movieNavigationButton}
                        name={icons.ARROW_LEFT}
                        size={30}
                        title={translate('GoToInterp', [previousStudio.fullName])}
                        to={`/studio/${previousStudio.foreignId}`}
                      />

                      <IconButton
                        className={styles.movieNavigationButton}
                        name={icons.ARROW_RIGHT}
                        size={30}
                        title={translate('GoToInterp', [nextStudio.fullName])}
                        to={`/studio/${nextStudio.foreignId}`}
                      />
                    </div>
                  </div>
                </Measure>

                <div className={styles.details}>
                  <div>
                    <span className={styles.years}>
                      {runningYears}
                    </span>

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
                            <StudioDetailsLinks
                              foreignId={foreignId}
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
                              <StudioTagsConnector studioId={id} />
                            }
                            position={tooltipPositions.BOTTOM}
                          />
                        </span>
                    }
                  </div>
                </div>

                <div className={styles.detailsLabels}>
                  <Label
                    className={styles.detailsLabel}
                    size={sizes.LARGE}
                  >
                    <Icon
                      name={icons.FOLDER}
                      size={17}
                    />

                    <span className={styles.path}>
                      {rootFolderPath}
                    </span>
                  </Label>

                  <Label
                    className={styles.detailsLabel}
                    title={translate('QualityProfile')}
                    size={sizes.LARGE}
                  >
                    <Icon
                      name={icons.PROFILE}
                      size={17}
                    />

                    <span className={styles.qualityProfileName}>
                      {
                        <QualityProfileNameConnector
                          qualityProfileId={qualityProfileId}
                        />
                      }
                    </span>
                  </Label>

                  <Label
                    className={styles.detailsLabel}
                    size={sizes.LARGE}
                  >
                    <Icon
                      name={monitored ? icons.MONITORED : icons.UNMONITORED}
                      size={17}
                    />

                    <span className={styles.qualityProfileName}>
                      {monitored ? translate('Monitored') : translate('Unmonitored')}
                    </span>
                  </Label>

                  <Tooltip
                    anchor={
                      <Label
                        className={styles.detailsLabel}
                        size={sizes.LARGE}
                      >
                        <Icon
                          name={icons.DRIVE}
                          size={17}
                        />

                        <span className={styles.sizeOnDisk}>
                          {
                            formatBytes(sizeOnDisk || 0)
                          }
                        </span>
                      </Label>
                    }
                    tooltip={
                      <span>
                        {null}
                      </span>
                    }
                    kind={kinds.INVERSE}
                    position={tooltipPositions.BOTTOM}
                  />

                  {
                    !!genres.length && !isSmallScreen &&
                      <Label
                        className={styles.detailsInfoLabel}
                        title={translate('Genres')}
                        size={sizes.LARGE}
                      >
                        <span className={styles.genres}>
                          {genres.join(', ')}
                        </span>
                      </Label>
                  }
                </div>
              </div>
            </div>
          </div>

          <div className={styles.contentContainer}>
            {
              !isFetching && moviesError ?
                <Alert kind={kinds.DANGER}>
                  {translate('LoadingMoviesFailed')}
                </Alert> :
                null
            }

            {
              !isFetching && isPopulated && hasMovies ?
                <FieldSet legend={translate('Movies')}>
                  {null}
                </FieldSet> :
                null
            }

            {
              !isFetching && isPopulated && hasScenes ?
                <FieldSet legend={translate('Scenes')}>
                  {
                    isPopulated && !!years.length &&
                      <div>
                        {
                          years.slice(0).reverse().map((year) => {
                            return (
                              <StudioDetailsYearConnector
                                key={year}
                                studioId={id}
                                studioForeignId={foreignId}
                                year={year}
                                isExpanded={expandedState[year]}
                                onExpandPress={this.onExpandPress}
                              />
                            );
                          })
                        }
                      </div>
                  }
                </FieldSet> :
                null
            }
          </div>

          <EditStudioModalConnector
            isOpen={isEditMovieModalOpen}
            studioId={id}
            onModalClose={this.onEditMovieModalClose}
            onDeleteMoviePress={this.onDeleteMoviePress}
          />

          <MovieInteractiveSearchModalConnector
            isOpen={isInteractiveSearchModalOpen}
            movieId={id}
            onModalClose={this.onInteractiveSearchModalClose}
          />
        </PageContentBody>
      </PageContent>
    );
  }
}

StudioDetails.propTypes = {
  id: PropTypes.number.isRequired,
  foreignId: PropTypes.string,
  title: PropTypes.string.isRequired,
  rootFolderPath: PropTypes.string.isRequired,
  sizeOnDisk: PropTypes.number.isRequired,
  qualityProfileId: PropTypes.number.isRequired,
  monitored: PropTypes.bool.isRequired,
  years: PropTypes.arrayOf(PropTypes.number).isRequired,
  genres: PropTypes.arrayOf(PropTypes.string).isRequired,
  images: PropTypes.arrayOf(PropTypes.object).isRequired,
  tags: PropTypes.arrayOf(PropTypes.number).isRequired,
  isSaving: PropTypes.bool.isRequired,
  isRefreshing: PropTypes.bool.isRequired,
  isSearching: PropTypes.bool.isRequired,
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  isSmallScreen: PropTypes.bool.isRequired,
  isSidebarVisible: PropTypes.bool.isRequired,
  previousStudio: PropTypes.object.isRequired,
  nextStudio: PropTypes.object.isRequired,
  onMonitorTogglePress: PropTypes.func.isRequired,
  onRefreshPress: PropTypes.func.isRequired,
  onSearchPress: PropTypes.func.isRequired,
  onGoToStudio: PropTypes.func.isRequired,
  moviesError: PropTypes.object,
  hasMovies: PropTypes.bool.isRequired,
  hasScenes: PropTypes.bool.isRequired,
  safeForWorkMode: PropTypes.bool
};

StudioDetails.defaultProps = {
  genres: [],
  tags: [],
  isSaving: false,
  sizeOnDisk: 0
};

export default StudioDetails;
