import classNames from 'classnames';
import moment from 'moment';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import CalendarEventConnector from 'Calendar/Events/CalendarEventConnector';
import getStatusStyle from 'Calendar/getStatusStyle';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import { icons } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import styles from './CalendarEventGroup.css';

function getEventsInfo(series, events) {
  let files = 0;
  let queued = 0;
  let monitored = 0;
  let absoluteEpisodeNumbers = 0;

  events.forEach((event) => {
    if (event.episodeFileId) {
      files++;
    }

    if (event.queued) {
      queued++;
    }

    if (series.monitored && event.monitored) {
      monitored++;
    }

    if (event.absoluteEpisodeNumber) {
      absoluteEpisodeNumbers++;
    }
  });

  return {
    allDownloaded: files === events.length,
    anyQueued: queued > 0,
    anyMonitored: monitored > 0,
    allAbsoluteEpisodeNumbers: absoluteEpisodeNumbers === events.length
  };
}

class CalendarEventGroup extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isExpanded: false
    };
  }

  //
  // Listeners

  onExpandPress = () => {
    this.setState({ isExpanded: !this.state.isExpanded });
  };

  //
  // Render

  render() {
    const {
      series,
      events,
      isDownloading,
      showEpisodeInformation,
      fullColorEvents,
      colorImpairedMode,
      onEventModalOpenToggle
    } = this.props;

    const { isExpanded } = this.state;
    const {
      allDownloaded,
      anyQueued,
      anyMonitored
    } = getEventsInfo(series, events);
    const anyDownloading = isDownloading || anyQueued;
    const firstEpisode = events[0];
    const lastEpisode = events[events.length -1];
    const releaseDate = firstEpisode.releaseDate;
    const startTime = moment(releaseDate);
    const endTime = moment(lastEpisode.releaseDate).add(series.runtime, 'minutes');
    const seasonNumber = firstEpisode.seasonNumber;
    const statusStyle = getStatusStyle(allDownloaded, anyDownloading, startTime, endTime, anyMonitored);

    if (isExpanded) {
      return (
        <div>
          {
            events.map((event) => {
              if (event.isGroup) {
                return null;
              }

              return (
                <CalendarEventConnector
                  key={event.id}
                  episodeId={event.id}
                  {...event}
                  onEventModalOpenToggle={onEventModalOpenToggle}
                />
              );
            })
          }

          <Link
            className={styles.collapseContainer}
            component="div"
            onPress={this.onExpandPress}
          >
            <Icon
              name={icons.COLLAPSE}
            />
          </Link>
        </div>
      );
    }

    return (
      <div
        className={classNames(
          styles.eventGroup,
          styles[statusStyle],
          colorImpairedMode && 'colorImpaired',
          fullColorEvents && 'fullColor'
        )}
      >
        <div className={styles.info}>
          <div className={styles.seriesTitle}>
            {series.title}
          </div>

          {
            anyDownloading &&
              <Icon
                containerClassName={styles.statusIcon}
                name={icons.DOWNLOADING}
                title={translate('AnEpisodeIsDownloading')}
              />
          }
        </div>

        <div className={styles.airingInfo}>
          {
            showEpisodeInformation ?
              <div className={styles.episodeInfo}>
                {seasonNumber}
              </div> :
              <Link
                className={styles.expandContainerInline}
                component="div"
                onPress={this.onExpandPress}
              >
                <Icon
                  name={icons.EXPAND}
                />
              </Link>
          }
        </div>

        {
          showEpisodeInformation &&
            <Link
              className={styles.expandContainer}
              component="div"
              onPress={this.onExpandPress}
            >
              <Icon
                name={icons.EXPAND}
              />
            </Link>
        }
      </div>
    );
  }
}

CalendarEventGroup.propTypes = {
  // Most of these props come from the connector and are required, but TS is confused.
  series: PropTypes.object,
  events: PropTypes.arrayOf(PropTypes.object).isRequired,
  isDownloading: PropTypes.bool,
  showEpisodeInformation: PropTypes.bool,
  showFinaleIcon: PropTypes.bool,
  fullColorEvents: PropTypes.bool,
  timeFormat: PropTypes.string,
  colorImpairedMode: PropTypes.bool,
  onEventModalOpenToggle: PropTypes.func.isRequired
};

export default CalendarEventGroup;
