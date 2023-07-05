import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { Tab, TabList, TabPanel, Tabs } from 'react-tabs';
import Button from 'Components/Link/Button';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import MonitorToggleButton from 'Components/MonitorToggleButton';
import episodeEntities from 'Episode/episodeEntities';
import EpisodeNumber from './EpisodeNumber';
import EpisodeHistoryConnector from './History/EpisodeHistoryConnector';
import EpisodeSearchConnector from './Search/EpisodeSearchConnector';
import EpisodeSummaryConnector from './Summary/EpisodeSummaryConnector';
import styles from './EpisodeDetailsModalContent.css';

const tabs = [
  'details',
  'history',
  'search'
];

class EpisodeDetailsModalContent extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      selectedTab: props.selectedTab
    };
  }

  //
  // Listeners

  onTabSelect = (index, lastIndex) => {
    const selectedTab = tabs[index];
    this.props.onTabChange(selectedTab === 'search');
    this.setState({ selectedTab });
  };

  //
  // Render

  render() {
    const {
      episodeId,
      episodeEntity,
      episodeFileId,
      seriesId,
      seriesTitle,
      titleSlug,
      seriesMonitored,
      episodeTitle,
      actors,
      releaseDate,
      monitored,
      isSaving,
      showOpenSeriesButton,
      startInteractiveSearch,
      onMonitorEpisodePress,
      onModalClose
    } = this.props;

    const seriesLink = `/site/${titleSlug}`;
    const joinedPerformers = actors.map((a) => a.character).slice(0, 4).join(', ');

    return (
      <ModalContent
        onModalClose={onModalClose}
      >
        <ModalHeader>
          <MonitorToggleButton
            className={styles.toggleButton}
            id={episodeId}
            monitored={monitored}
            size={18}
            isDisabled={!seriesMonitored}
            isSaving={isSaving}
            onPress={onMonitorEpisodePress}
          />

          <span className={styles.seriesTitle}>
            {seriesTitle}
          </span>

          <span className={styles.separator}>-</span>

          <EpisodeNumber
            releaseDate={releaseDate}
          />

          <span className={styles.separator}>-</span>

          {episodeTitle}

          <span className={styles.separator}>-</span>

          {joinedPerformers}

        </ModalHeader>

        <ModalBody>
          <Tabs
            className={styles.tabs}
            selectedIndex={tabs.indexOf(this.state.selectedTab)}
            onSelect={this.onTabSelect}
          >
            <TabList
              className={styles.tabList}
            >
              <Tab
                className={styles.tab}
                selectedClassName={styles.selectedTab}
              >
                Details
              </Tab>

              <Tab
                className={styles.tab}
                selectedClassName={styles.selectedTab}
              >
                History
              </Tab>

              <Tab
                className={styles.tab}
                selectedClassName={styles.selectedTab}
              >
                Search
              </Tab>
            </TabList>

            <TabPanel>
              <div className={styles.tabContent}>
                <EpisodeSummaryConnector
                  episodeId={episodeId}
                  episodeEntity={episodeEntity}
                  episodeFileId={episodeFileId}
                  seriesId={seriesId}
                />
              </div>
            </TabPanel>

            <TabPanel>
              <div className={styles.tabContent}>
                <EpisodeHistoryConnector
                  episodeId={episodeId}
                />
              </div>
            </TabPanel>

            <TabPanel>
              {/* Don't wrap in tabContent so we not have a top margin */}
              <EpisodeSearchConnector
                episodeId={episodeId}
                startInteractiveSearch={startInteractiveSearch}
                onModalClose={onModalClose}
              />
            </TabPanel>
          </Tabs>
        </ModalBody>

        <ModalFooter>
          {
            showOpenSeriesButton &&
              <Button
                className={styles.openSeriesButton}
                to={seriesLink}
                onPress={onModalClose}
              >
                Open Site
              </Button>
          }

          <Button
            onPress={onModalClose}
          >
            Close
          </Button>
        </ModalFooter>
      </ModalContent>
    );
  }
}

EpisodeDetailsModalContent.propTypes = {
  episodeId: PropTypes.number.isRequired,
  episodeEntity: PropTypes.string.isRequired,
  episodeFileId: PropTypes.number,
  seriesId: PropTypes.number.isRequired,
  seriesTitle: PropTypes.string.isRequired,
  titleSlug: PropTypes.string.isRequired,
  seriesMonitored: PropTypes.bool.isRequired,
  actors: PropTypes.arrayOf(PropTypes.object),
  releaseDate: PropTypes.string.isRequired,
  episodeTitle: PropTypes.string.isRequired,
  monitored: PropTypes.bool.isRequired,
  isSaving: PropTypes.bool,
  showOpenSeriesButton: PropTypes.bool,
  selectedTab: PropTypes.string.isRequired,
  startInteractiveSearch: PropTypes.bool.isRequired,
  onMonitorEpisodePress: PropTypes.func.isRequired,
  onTabChange: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

EpisodeDetailsModalContent.defaultProps = {
  selectedTab: 'details',
  episodeEntity: episodeEntities.EPISODES,
  startInteractiveSearch: false,
  actors: []
};

export default EpisodeDetailsModalContent;
