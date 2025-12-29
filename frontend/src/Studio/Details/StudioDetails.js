import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Alert from 'Components/Alert';
import Delayed from 'Components/Delayed';
import FieldSet from 'Components/FieldSet';
import Icon from 'Components/Icon';
import Label from 'Components/Label';
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
import QualityProfileName from 'Settings/Profiles/Quality/QualityProfileName';
import DeleteStudioModalConnector from 'Studio/Delete/DeleteStudioModalConnector';
import EditStudioModalConnector from 'Studio/Edit/EditStudioModalConnector';
import StudioLogo from 'Studio/StudioLogo';
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
      isDeleteMovieModalOpen: false,
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

  onDeleteMoviePress = () => {
    this.setState({ isDeleteMovieModalOpen: true });
  };

  onDeleteMovieModalClose = () => {
    this.setState({ isDeleteMovieModalOpen: false });
  };

  onEditMoviePress = () => {
    this.setState({ isEditMovieModalOpen: true });
  };

  onEditMovieModalClose = () => {
    this.setState({ isEditMovieModalOpen: false });
  };

  onTitleMeasure = ({ width }) => {
    this.setState({ titleWidth: width });
  };

  onExpandAllPress = () => {
    const {
      allExpanded,
      expandedState
    } = this.state;

    this.setState(getExpandedState(selectAll(expandedState, !allExpanded)));
  };

  onExpandPress = (year, isExpanded) => {
    this.setState((state) => {
      const convertedState = {
        allSelected: state.allExpanded,
        allUnselected: state.allCollapsed,
        selectedState: state.expandedState
      };

      const newState = toggleSelected(convertedState, [], year, isExpanded, false);

      return getExpandedState(newState);
    });
  };

  //
  // Render

  render() {
    const {
      id,
      foreignId,
      tpdbId,
      website,
      title,
      aliases,
      rootFolderPath,
      sizeOnDisk,
      qualityProfileId,
      monitored,
      years,
      genres,
      images,
      network,
      tags,
      isSaving,
      isRefreshing,
      isSearching,
      isFetching,
      isPopulated,
      isSmallScreen,
      hasMovies,
      hasScenes,
      totalSceneCount,
      sceneCount,
      onMonitorTogglePress,
      onRefreshPress,
      onSearchPress,
      moviesError,
      safeForWorkMode
    } = this.props;

    const {
      isEditMovieModalOpen,
      isDeleteMovieModalOpen,
      expandedState,
      allExpanded,
      allCollapsed
    } = this.state;

    let expandIcon = icons.EXPAND_INDETERMINATE;
    if (allExpanded) {
      expandIcon = icons.COLLAPSE;
    } else if (allCollapsed) {
      expandIcon = icons.EXPAND;
    }

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

          <PageToolbarSection alignContent="right">
            <PageToolbarButton
              label={allExpanded ? 'Collapse All' : 'Expand All'}
              iconName={expandIcon}
              onPress={this.onExpandAllPress}
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
                safeForWorkMode={safeForWorkMode}
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
                  </div>
                </Measure>

                <div className={styles.details}>
                  <div>
                    <span className={styles.years}>
                      {runningYears}
                    </span>

                    <span className={styles.network}>
                      {network}
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
                              website={website}
                              tpdbId={tpdbId}
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
                        <QualityProfileName
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

                  <Label
                    className={styles.detailsLabel}
                    size={sizes.LARGE}
                  >
                    <Icon
                      name={icons.SCENE}
                      size={17}
                    />

                    <span className={styles.qualityProfileName}>
                      Scenes: {sceneCount || 0}/{totalSceneCount}
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

                  {
                    !!aliases && !!aliases.length &&
                      <Label
                        className={styles.detailsInfoLabel}
                        title={translate('Aliases')}
                        size={sizes.LARGE}
                      >
                        <Icon
                          name={icons.TAGS}
                          size={17}
                        />

                        <span className={styles.aliases}>
                          {aliases.join(', ')}
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
                  {
                    isPopulated && !!years.length &&
                      <div>
                        {
                          years.slice(0).reverse().map((year) => {
                            return (
                              <Delayed key={year} waitBeforeShow={50}>
                                <StudioDetailsYearConnector
                                  key={year}
                                  studioId={id}
                                  studioForeignId={foreignId}
                                  year={year}
                                  isScenes={false}
                                  isExpanded={expandedState[year]}
                                  onExpandPress={this.onExpandPress}
                                />
                              </Delayed>
                            );
                          })
                        }
                      </div>
                  }
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
                              <Delayed key={year} waitBeforeShow={50}>
                                <StudioDetailsYearConnector
                                  key={year}
                                  studioId={id}
                                  studioForeignId={foreignId}
                                  year={year}
                                  isScenes={true}
                                  isExpanded={expandedState[year]}
                                  onExpandPress={this.onExpandPress}
                                />
                              </Delayed>
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

          <DeleteStudioModalConnector
            isOpen={isDeleteMovieModalOpen}
            studioId={id}
            onModalClose={this.onDeleteMovieModalClose}
            onDeleteMoviePress={this.onDeleteMoviePress}
          />

        </PageContentBody>
      </PageContent>
    );
  }
}

StudioDetails.propTypes = {
  id: PropTypes.number.isRequired,
  foreignId: PropTypes.string,
  tpdbId: PropTypes.string,
  website: PropTypes.string,
  title: PropTypes.string.isRequired,
  aliases: PropTypes.arrayOf(PropTypes.string),
  network: PropTypes.string.isRequired,
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
  totalSceneCount: PropTypes.number.isRequired,
  sceneCount: PropTypes.number.isRequired,
  safeForWorkMode: PropTypes.bool
};

StudioDetails.defaultProps = {
  genres: [],
  tags: [],
  isSaving: false,
  sizeOnDisk: 0
};

export default StudioDetails;
