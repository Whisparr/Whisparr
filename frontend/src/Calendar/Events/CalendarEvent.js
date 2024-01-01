import classNames from 'classnames';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import { icons, kinds } from 'Helpers/Props';
import getStatusStyle from 'Utilities/Movie/getStatusStyle';
import firstCharToUpper from 'Utilities/String/firstCharToUpper';
import translate from 'Utilities/String/translate';
import CalendarEventQueueDetails from './CalendarEventQueueDetails';
import styles from './CalendarEvent.css';

class CalendarEvent extends Component {

  //
  // Render

  render() {
    const {
      movieFile,
      isAvailable,
      title,
      titleSlug,
      genres,
      itemType,
      monitored,
      certification,
      hasFile,
      grabbed,
      queueItem,
      showMovieInformation,
      showCutoffUnmetIcon,
      fullColorEvents,
      colorImpairedMode
    } = this.props;

    const isDownloading = !!(queueItem || grabbed);
    const isMonitored = monitored;
    const statusStyle = getStatusStyle(null, isMonitored, hasFile, isAvailable, 'style', isDownloading);
    const joinedGenres = genres.slice(0, 2).join(', ');
    const link = `/movie/${titleSlug}`;

    return (
      <div
        className={classNames(
          styles.event,
          styles[statusStyle],
          colorImpairedMode && 'colorImpaired',
          fullColorEvents && 'fullColor'
        )}
      >
        <Link
          className={styles.underlay}
          to={link}
        />

        <div className={styles.overlay} >
          <div className={styles.info}>
            <div className={styles.movieTitle}>
              {title}
            </div>

            <div className={styles.statusContainer}>
              {
                queueItem ?
                  <span className={styles.statusIcon}>
                    <CalendarEventQueueDetails
                      {...queueItem}
                    />
                  </span> :
                  null
              }

              {
                !queueItem && grabbed ?
                  <Icon
                    className={styles.statusIcon}
                    name={icons.DOWNLOADING}
                    title={translate('MovieIsDownloading')}
                  /> :
                  null
              }

              {
                showCutoffUnmetIcon && !!movieFile && movieFile.qualityCutoffNotMet ?
                  <Icon
                    className={styles.statusIcon}
                    name={icons.MOVIE_FILE}
                    kind={kinds.WARNING}
                    title={translate('QualityCutoffHasNotBeenMet')}
                  /> :
                  null
              }
            </div>
          </div>

          {
            showMovieInformation ?
              <div className={styles.movieInfo}>
                <div className={styles.genres}>
                  {joinedGenres}
                </div>
              </div> :
              null
          }

          {
            showMovieInformation ?
              <div className={styles.movieInfo}>
                <div className={styles.genres}>
                  {firstCharToUpper(itemType)}
                </div>
                <div>
                  {certification}
                </div>
              </div> :
              null
          }
        </div>
      </div>
    );
  }
}

CalendarEvent.propTypes = {
  id: PropTypes.number.isRequired,
  genres: PropTypes.arrayOf(PropTypes.string).isRequired,
  movieFile: PropTypes.object,
  title: PropTypes.string.isRequired,
  titleSlug: PropTypes.string.isRequired,
  isAvailable: PropTypes.bool.isRequired,
  releaseDate: PropTypes.string,
  itemType: PropTypes.string.isRequired,
  date: PropTypes.string.isRequired,
  monitored: PropTypes.bool.isRequired,
  certification: PropTypes.string,
  hasFile: PropTypes.bool.isRequired,
  grabbed: PropTypes.bool,
  queueItem: PropTypes.object,
  // These props come from the connector, not marked as required to appease TS for now.
  showMovieInformation: PropTypes.bool,
  showCutoffUnmetIcon: PropTypes.bool,
  fullColorEvents: PropTypes.bool,
  timeFormat: PropTypes.string,
  colorImpairedMode: PropTypes.bool
};

CalendarEvent.defaultProps = {
  genres: []
};

export default CalendarEvent;
