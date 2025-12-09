import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import Icon from 'Components/Icon';
import Label from 'Components/Label';
import Link from 'Components/Link/Link';
import TmdbRating from 'Components/TmdbRating';
import Tooltip from 'Components/Tooltip/Tooltip';
import { icons, kinds, sizes, tooltipPositions } from 'Helpers/Props';
import MovieDetailsLinks from 'Movie/Details/MovieDetailsLinks';
import MovieStatusLabel from 'Movie/Details/MovieStatusLabel';
import MovieIndexProgressBar from 'Movie/Index/ProgressBar/MovieIndexProgressBar';
import MoviePoster from 'Movie/MoviePoster';
import ScenePoster from 'Scene/ScenePoster';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import StudioLogo from 'Studio/StudioLogo';
import formatRuntime from 'Utilities/Date/formatRuntime';
import getRelativeDate from 'Utilities/Date/getRelativeDate';
import firstCharToUpper from 'Utilities/String/firstCharToUpper';
import translate from 'Utilities/String/translate';
import AddNewMovieModal from './AddNewMovieModal';
import styles from './AddNewMovieSearchResult.css';

function createMapStateToProps() {
  return createSelector(
    createUISettingsSelector(),
    (uiSettings) => {
      return {
        shortDateFormat: uiSettings.shortDateFormat,
        timeFormat: uiSettings.timeFormat
      };
    }
  );
}
class AddNewMovieSearchResult extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isNewAddMovieModalOpen: false
    };
  }

  componentDidUpdate(prevProps) {
    if (!prevProps.isExistingMovie && this.props.isExistingMovie) {
      this.onAddMovieModalClose();
    }
  }

  //
  // Listeners

  onPress = () => {
    this.setState({ isNewAddMovieModalOpen: true });
  };

  onAddMovieModalClose = () => {
    this.setState({ isNewAddMovieModalOpen: false });
  };

  onExternalLinkPress = (event) => {
    event.stopPropagation();
  };

  //
  // Render

  render() {
    const {
      foreignId,
      title,
      titleSlug,
      year,
      studioTitle,
      genres,
      status,
      itemType,
      releaseDate,
      overview,
      ratings,
      folder,
      images,
      existingMovieId,
      isExistingMovie,
      isExcluded,
      isSmallScreen,
      colorImpairedMode,
      id,
      monitored,
      isAvailable,
      movieFile,
      queueItem,
      runtime,
      movieRuntimeFormat,
      certification,
      safeForWorkMode
    } = this.props;

    const { showRelativeDates, shortDateFormat } = this.props;

    const {
      isNewAddMovieModalOpen
    } = this.state;

    const hasMovieFile = !!movieFile;

    const linkProps = isExistingMovie ? { to: `/movie/${titleSlug}` } : { onPress: this.onPress };
    const providerProps = itemType === 'movie' ? { tmdbId: foreignId } : { stashId: foreignId };

    let ImageItem = MoviePoster;
    let posterWidth = 167;
    let posterHeight = 250;

    if (itemType === 'scene') {
      ImageItem = ScenePoster;
      posterWidth = 300;
      posterHeight = 169;
    }

    if (itemType === 'studio') {
      ImageItem = StudioLogo;
      posterWidth = 250;
      posterHeight = 250;
    }

    // (debug logging removed)

    const elementStyle = {
      width: `${posterWidth}px`,
      height: `${posterHeight}px`,
      objectFit: 'contain'
    };

    return (
      <div className={styles.searchResult}>
        <Link
          className={styles.underlay}
          {...linkProps}
        />

        <div className={styles.overlay}>
          {
            isSmallScreen ?
              null :
              <div>
                <div className={styles.posterContainer}>
                  <ImageItem
                    safeForWorkMode={safeForWorkMode}
                    className={itemType === 'scene' ? styles.scene : styles.poster}
                    style={elementStyle}
                    images={images}
                    size={250}
                    overflow={true}
                    lazy={false}
                  />
                </div>

                {
                  isExistingMovie &&
                    <MovieIndexProgressBar
                      movieId={existingMovieId}
                      movieFile={movieFile}
                      monitored={monitored}
                      hasFile={hasMovieFile}
                      status={status}
                      width={posterWidth}
                      detailedProgressBar={true}
                      isAvailable={isAvailable}
                    />
                }
              </div>
          }

          <div className={styles.content}>
            <div className={styles.titleRow}>
              <div className={styles.titleContainer}>
                <div className={styles.title}>
                  {title}

                  {
                    !!year && !(String(title || '').includes(String(year))) ?
                      <span className={styles.year}>
                        ({releaseDate ? getRelativeDate({
                          date: releaseDate,
                          shortDateFormat,
                          showRelativeDates
                        }) : year})
                      </span> :
                      null
                  }
                </div>
              </div>

              <div className={styles.icons}>
                <div>
                  {
                    isExistingMovie &&
                      <Icon
                        className={styles.alreadyExistsIcon}
                        name={icons.CHECK_CIRCLE}
                        size={36}
                        title={translate('AlreadyInYourLibrary')}
                      />
                  }

                  {
                    isExcluded &&
                      <Icon
                        className={styles.exclusionIcon}
                        name={icons.DANGER}
                        size={36}
                        title={translate('MovieIsOnImportExclusionList')}
                      />
                  }
                </div>
              </div>
            </div>

            <div>
              {
                !!certification &&
                  <span className={styles.certification}>
                    {certification}
                  </span>
              }

              {
                !!runtime &&
                  <span className={styles.runtime}>
                    {formatRuntime(runtime, movieRuntimeFormat)}
                  </span>
              }
            </div>

            <div>
              <Label size={sizes.LARGE} kind={kinds.PINK}>
                {firstCharToUpper(itemType)}
              </Label>

              {
                itemType === 'movie' &&
                  <Label size={sizes.LARGE}>
                    <TmdbRating
                      ratings={ratings}
                      iconSize={13}
                    />
                  </Label>
              }

              {
                !!studioTitle &&
                  <Label size={sizes.LARGE}>
                    <Icon
                      name={icons.STUDIO}
                      size={13}
                    />
                    <span className={styles.studio}>
                      {studioTitle}
                    </span>
                  </Label>
              }

              {
                genres.length > 0 ?
                  <Label size={sizes.LARGE}>
                    <Icon
                      name={icons.GENRE}
                      size={13}
                    />

                    <span className={styles.genres}>
                      {genres.slice(0, 3).join(', ')}
                    </span>
                  </Label> :
                  null
              }

              {
                genres.length > 0 ?
                  <Label size={sizes.LARGE}>
                    <Icon
                      name={icons.GENRE}
                      size={13}
                    />

                    <span className={styles.genres}>
                      {genres.slice(0, 3).join(', ')}
                    </span>
                  </Label> :
                  null
              }

              <Tooltip
                anchor={
                  <Label
                    size={sizes.LARGE}
                  >
                    <Icon
                      name={icons.EXTERNAL_LINK}
                      size={13}
                    />

                    <span className={styles.links}>
                      {translate('Links')}
                    </span>
                  </Label>
                }
                tooltip={
                  <MovieDetailsLinks
                    {...providerProps}
                  />
                }
                canFlip={true}
                kind={kinds.INVERSE}
                position={tooltipPositions.BOTTOM}
              />

              {
                isExistingMovie && isSmallScreen &&
                  <MovieStatusLabel
                    status={status}
                    hasMovieFiles={hasMovieFile}
                    monitored={monitored}
                    isAvailable={isAvailable}
                    queueItem={queueItem}
                    id={id}
                    useLabel={true}
                    colorImpairedMode={colorImpairedMode}
                  />
              }
            </div>

            <div className={styles.overview}>
              {overview}
            </div>
          </div>
        </div>

        <AddNewMovieModal
          isOpen={isNewAddMovieModalOpen && !isExistingMovie}
          foreignId={foreignId}
          title={title}
          year={year}
          overview={overview}
          folder={folder}
          images={images}
          onModalClose={this.onAddMovieModalClose}
        />
      </div>
    );
  }
}

AddNewMovieSearchResult.propTypes = {
  foreignId: PropTypes.string.isRequired,
  title: PropTypes.string.isRequired,
  titleSlug: PropTypes.string.isRequired,
  year: PropTypes.number.isRequired,
  studioTitle: PropTypes.string,
  genres: PropTypes.arrayOf(PropTypes.string),
  status: PropTypes.string.isRequired,
  releaseDate: PropTypes.string,
  itemType: PropTypes.string.isRequired,
  overview: PropTypes.string,
  ratings: PropTypes.object.isRequired,
  folder: PropTypes.string.isRequired,
  images: PropTypes.arrayOf(PropTypes.object).isRequired,
  existingMovieId: PropTypes.number,
  isExistingMovie: PropTypes.bool.isRequired,
  isExcluded: PropTypes.bool,
  isSmallScreen: PropTypes.bool.isRequired,
  id: PropTypes.number,
  monitored: PropTypes.bool.isRequired,
  isAvailable: PropTypes.bool.isRequired,
  movieFile: PropTypes.object,
  queueItem: PropTypes.object,
  colorImpairedMode: PropTypes.bool,
  runtime: PropTypes.number.isRequired,
  movieRuntimeFormat: PropTypes.string.isRequired,
  certification: PropTypes.string,
  safeForWorkMode: PropTypes.bool,
  showRelativeDates: PropTypes.bool,
  shortDateFormat: PropTypes.string,
  longDateFormat: PropTypes.string,
  timeFormat: PropTypes.string
};

AddNewMovieSearchResult.defaultProps = {
  genres: [],
  isExcluded: false
};

export default connect(createMapStateToProps)(AddNewMovieSearchResult);
