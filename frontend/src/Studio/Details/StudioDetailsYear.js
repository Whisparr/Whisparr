import _ from 'lodash';
import moment from 'moment';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Icon from 'Components/Icon';
import Label from 'Components/Label';
import IconButton from 'Components/Link/IconButton';
import Link from 'Components/Link/Link';
import SpinnerIconButton from 'Components/Link/SpinnerIconButton';
import Menu from 'Components/Menu/Menu';
import MenuButton from 'Components/Menu/MenuButton';
import MenuContent from 'Components/Menu/MenuContent';
import MenuItem from 'Components/Menu/MenuItem';
import SpinnerIcon from 'Components/SpinnerIcon';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import Popover from 'Components/Tooltip/Popover';
import { align, icons, kinds, sizes, sortDirections, tooltipPositions } from 'Helpers/Props';
import formatBytes from 'Utilities/Number/formatBytes';
import translate from 'Utilities/String/translate';
import SceneRowConnector from './SceneRowConnector';
import YearInfo from './YearInfo';
import styles from './StudioDetailsYear.css';

function getYearStatistics(movies) {
  let movieCount = 0;
  let movieFileCount = 0;
  let totalMovieCount = 0;
  let monitoredMovieCount = 0;
  let hasMonitoredMovies = false;
  const sizeOnDisk = 0;

  movies.forEach((movie) => {
    if (movie.movieFile || (movie.monitored && movie.isAvailable)) {
      movieCount++;
    }

    if (movie.movieFile) {
      movieFileCount++;
    }

    if (movie.monitored) {
      monitoredMovieCount++;
      hasMonitoredMovies = true;
    }

    totalMovieCount++;
  });

  return {
    movieCount,
    movieFileCount,
    totalMovieCount,
    monitoredMovieCount,
    hasMonitoredMovies,
    sizeOnDisk
  };
}

function getMovieCountKind(monitored, movieFileCount, movieCount) {
  if (movieFileCount === movieCount && movieCount > 0) {
    return kinds.SUCCESS;
  }

  if (!monitored) {
    return kinds.WARNING;
  }

  return kinds.DANGER;
}

class StudioDetailsYear extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isOrganizeModalOpen: false,
      isManageMoviesOpen: false,
      isHistoryModalOpen: false,
      isInteractiveSearchModalOpen: false,
      lastToggledMovie: null
    };
  }

  componentDidMount() {
    this._expandByDefault();
  }

  componentDidUpdate(prevProps) {
    const {
      studioId,
      items
    } = this.props;

    if (prevProps.studioId !== studioId) {
      this._expandByDefault();
      return;
    }

    if (
      getYearStatistics(prevProps.items).movieFileCount > 0 &&
      getYearStatistics(items).movieFileCount === 0
    ) {
      this.setState({
        isOrganizeModalOpen: false,
        isManageMoviesOpen: false
      });
    }
  }

  //
  // Control

  _expandByDefault() {
    const {
      year,
      onExpandPress,
      items
    } = this.props;

    const expand = _.some(items, (item) => {
      return year === moment().year() && items.length < 60;
    });

    onExpandPress(year, expand);
  }

  //
  // Listeners

  onInteractiveSearchPress = () => {
    this.setState({ isInteractiveSearchModalOpen: true });
  };

  onInteractiveSearchModalClose = () => {
    this.setState({ isInteractiveSearchModalOpen: false });
  };

  onExpandPress = () => {
    const {
      year,
      isExpanded
    } = this.props;

    this.props.onExpandPress(year, !isExpanded);
  };

  onMonitorMoviePress = (movieId, monitored, { shiftKey }) => {
    this.setState({ lastToggledMovie: movieId });

    this.props.onMonitorMoviePress(movieId, monitored);
  };

  //
  // Render

  render() {
    const {
      year,
      items,
      isExpanded,
      isSearching,
      studioMonitored,
      isSmallScreen,
      onSearchPress,
      columns,
      sortKey,
      sortDirection,
      onSortPress,
      onTableOptionChange
    } = this.props;

    const {
      movieCount,
      movieFileCount,
      totalMovieCount,
      monitoredMovieCount,
      hasMonitoredMovies
    } = getYearStatistics(items);

    const sizeOnDisk = _.sumBy(items, 'sizeOnDisk');

    return (
      <div
        className={styles.season}
      >
        <div className={styles.header}>
          <div className={styles.left}>
            <span className={styles.seasonNumber}>
              {year}
            </span>

            <Popover
              className={styles.movieCountTooltip}
              canFlip={true}
              anchor={
                <Label
                  kind={getMovieCountKind(true, movieFileCount, movieCount)}
                  size={sizes.LARGE}
                >
                  <span>{movieFileCount} / {movieCount}</span>
                </Label>
              }
              title={translate('SeasonInformation')}
              body={
                <div>
                  <YearInfo
                    totalMovieCount={totalMovieCount}
                    monitoredMovieCount={monitoredMovieCount}
                    movieFileCount={movieFileCount}
                    sizeOnDisk={sizeOnDisk}
                  />
                </div>
              }
              position={tooltipPositions.BOTTOM}
            />

            {
              sizeOnDisk ?
                <div className={styles.sizeOnDisk}>
                  {formatBytes(sizeOnDisk)}
                </div> :
                null
            }
          </div>

          <Link
            className={styles.expandButton}
            onPress={this.onExpandPress}
          >
            <Icon
              className={styles.expandButtonIcon}
              name={isExpanded ? icons.COLLAPSE : icons.EXPAND}
              title={isExpanded ? translate('HideMovies') : translate('ShowMovies')}
              size={24}
            />
            {
              !isSmallScreen &&
                <span>&nbsp;</span>
            }
          </Link>

          {
            isSmallScreen ?
              <Menu
                className={styles.actionsMenu}
                alignMenu={align.RIGHT}
                enforceMaxHeight={false}
              >
                <MenuButton>
                  <Icon
                    name={icons.ACTIONS}
                    size={22}
                  />
                </MenuButton>

                <MenuContent className={styles.actionsMenuContent}>
                  <MenuItem
                    isDisabled={isSearching || !hasMonitoredMovies || !studioMonitored}
                    onPress={onSearchPress}
                  >
                    <SpinnerIcon
                      className={styles.actionMenuIcon}
                      name={icons.SEARCH}
                      isSpinning={isSearching}
                    />

                    {translate('Search')}
                  </MenuItem>

                  <MenuItem
                    onPress={this.onInteractiveSearchPress}
                    isDisabled={!totalMovieCount}
                  >
                    <Icon
                      className={styles.actionMenuIcon}
                      name={icons.INTERACTIVE}
                    />

                    {translate('InteractiveSearch')}
                  </MenuItem>
                </MenuContent>
              </Menu> :

              <div className={styles.actions}>
                <SpinnerIconButton
                  className={styles.actionButton}
                  name={icons.SEARCH}
                  title={hasMonitoredMovies && studioMonitored ? translate('SearchForMonitoredMoviesSeason') : translate('NoMonitoredMoviesSeason')}
                  size={24}
                  isSpinning={isSearching}
                  isDisabled={isSearching || !hasMonitoredMovies || !studioMonitored}
                  onPress={onSearchPress}
                />

                <IconButton
                  className={styles.actionButton}
                  name={icons.INTERACTIVE}
                  title={translate('InteractiveSearchSeason')}
                  size={24}
                  isDisabled={!totalMovieCount}
                  onPress={this.onInteractiveSearchPress}
                />
              </div>
          }

        </div>

        <div>
          {
            isExpanded &&
              <div className={styles.movies}>
                {
                  items.length ?
                    <Table
                      columns={columns}
                      sortKey={sortKey}
                      sortDirection={sortDirection}
                      onSortPress={onSortPress}
                      onTableOptionChange={onTableOptionChange}
                    >
                      <TableBody>
                        {
                          items.map((item) => {
                            return (
                              <SceneRowConnector
                                key={item.id}
                                columns={columns}
                                {...item}
                                onMonitorMoviePress={this.onMonitorMoviePress}
                              />
                            );
                          })
                        }
                      </TableBody>
                    </Table> :

                    <div className={styles.noMovies}>
                      {translate('NoMoviesInThisSeason')}
                    </div>
                }
                <div className={styles.collapseButtonContainer}>
                  <IconButton
                    iconClassName={styles.collapseButtonIcon}
                    name={icons.COLLAPSE}
                    size={20}
                    title={translate('HideMovies')}
                    onPress={this.onExpandPress}
                  />
                </div>
              </div>
          }
        </div>
      </div>
    );
  }
}

StudioDetailsYear.propTypes = {
  studioId: PropTypes.number.isRequired,
  year: PropTypes.number.isRequired,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  columns: PropTypes.arrayOf(PropTypes.object).isRequired,
  sortKey: PropTypes.string.isRequired,
  sortDirection: PropTypes.oneOf(sortDirections.all),
  isSaving: PropTypes.bool,
  isExpanded: PropTypes.bool,
  isSearching: PropTypes.bool.isRequired,
  studioMonitored: PropTypes.bool.isRequired,
  isSmallScreen: PropTypes.bool.isRequired,
  onTableOptionChange: PropTypes.func.isRequired,
  onSortPress: PropTypes.func.isRequired,
  onExpandPress: PropTypes.func.isRequired,
  onMonitorMoviePress: PropTypes.func.isRequired,
  onSearchPress: PropTypes.func.isRequired
};

StudioDetailsYear.defaultProps = {
  statistics: {}
};

export default StudioDetailsYear;
