import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Label from 'Components/Label';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import { kinds, sizes } from 'Helpers/Props';
import SeriesPerformerConnector from 'Series/SeriesPerformerConnector';
import QualityProfileNameConnector from 'Settings/Profiles/Quality/QualityProfileNameConnector';
import translate from 'Utilities/String/translate';
import EpisodeAiringConnector from './EpisodeAiringConnector';
import EpisodeFileRow from './EpisodeFileRow';
import styles from './EpisodeSummary.css';

const columns = [
  {
    name: 'path',
    label: () => translate('Path'),
    isSortable: false,
    isVisible: true
  },
  {
    name: 'size',
    label: () => translate('Size'),
    isSortable: false,
    isVisible: true
  },
  {
    name: 'languages',
    label: () => translate('Languages'),
    isSortable: false,
    isVisible: true
  },
  {
    name: 'quality',
    label: () => translate('Quality'),
    isSortable: false,
    isVisible: true
  },
  {
    name: 'customFormats',
    label: () => translate('Formats'),
    isSortable: false,
    isVisible: true
  },
  {
    name: 'actions',
    label: '',
    isSortable: false,
    isVisible: true
  }
];

class EpisodeSummary extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isRemoveEpisodeFileModalOpen: false
    };
  }

  //
  // Listeners

  onRemoveEpisodeFilePress = () => {
    this.setState({ isRemoveEpisodeFileModalOpen: true });
  };

  onConfirmRemoveEpisodeFile = () => {
    this.props.onDeleteEpisodeFile();
    this.setState({ isRemoveEpisodeFileModalOpen: false });
  };

  onRemoveEpisodeFileModalClose = () => {
    this.setState({ isRemoveEpisodeFileModalOpen: false });
  };

  //
  // Render

  render() {
    const {
      seriesId,
      qualityProfileId,
      network,
      overview,
      releaseDate,
      mediaInfo,
      path,
      size,
      actors,
      languages,
      quality,
      customFormats,
      qualityCutoffNotMet,
      onDeleteEpisodeFile
    } = this.props;

    const hasOverview = !!overview;

    return (
      <div>
        <div>
          <span className={styles.infoTitle}>{translate('Released')}</span>

          <EpisodeAiringConnector
            releaseDate={releaseDate}
            network={network}
          />
        </div>

        <div>
          <span className={styles.infoTitle}>{translate('QualityProfile')}</span>

          <Label
            kind={kinds.PRIMARY}
            size={sizes.MEDIUM}
          >
            <QualityProfileNameConnector
              qualityProfileId={qualityProfileId}
            />
          </Label>
        </div>

        <div>
          <span className={styles.actors}>Performers</span>

          <div className={styles.actorContainer}>
            {
              actors.map((a) => {
                const {
                  tpdbId,
                  name: actorName
                } = a;

                return (
                  <div className={styles.actor} key={a.tpdbId}>
                    <SeriesPerformerConnector
                      tpdbId={tpdbId}
                      seriesId={seriesId}
                      name={actorName}
                      key={a.id}
                    />
                  </div>
                );
              })
            }
          </div>
        </div>

        <div className={styles.overview}>
          {
            hasOverview ?
              overview :
              translate('NoEpisodeOverview')
          }
        </div>

        {
          path ?
            <Table columns={columns}>
              <TableBody>
                <EpisodeFileRow
                  path={path}
                  size={size}
                  languages={languages}
                  quality={quality}
                  qualityCutoffNotMet={qualityCutoffNotMet}
                  customFormats={customFormats}
                  mediaInfo={mediaInfo}
                  columns={columns}
                  onDeleteEpisodeFile={onDeleteEpisodeFile}
                />
              </TableBody>
            </Table> :
            null
        }

        <ConfirmModal
          isOpen={this.state.isRemoveEpisodeFileModalOpen}
          kind={kinds.DANGER}
          title={translate('DeleteEpisodeFile')}
          message={translate('DeleteEpisodeFileMessage', { path })}
          confirmLabel={translate('Delete')}
          onConfirm={this.onConfirmRemoveEpisodeFile}
          onCancel={this.onRemoveEpisodeFileModalClose}
        />
      </div>
    );
  }
}

EpisodeSummary.propTypes = {
  seriesId: PropTypes.number.isRequired,
  episodeFileId: PropTypes.number.isRequired,
  qualityProfileId: PropTypes.number.isRequired,
  network: PropTypes.string.isRequired,
  overview: PropTypes.string,
  releaseDate: PropTypes.string.isRequired,
  mediaInfo: PropTypes.object,
  path: PropTypes.string,
  size: PropTypes.number,
  actors: PropTypes.arrayOf(PropTypes.object),
  joinedPerformers: PropTypes.string,
  languages: PropTypes.arrayOf(PropTypes.object),
  quality: PropTypes.object,
  qualityCutoffNotMet: PropTypes.bool,
  customFormats: PropTypes.arrayOf(PropTypes.object),
  onDeleteEpisodeFile: PropTypes.func.isRequired
};

EpisodeSummary.defaultProps = {
  actors: []
};

export default EpisodeSummary;
