import React, { useCallback, useState } from 'react';
import { useSelector } from 'react-redux';
import ProtocolLabel from 'Activity/Queue/ProtocolLabel';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import SpinnerIconButton from 'Components/Link/SpinnerIconButton';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableRow from 'Components/Table/TableRow';
import Popover from 'Components/Tooltip/Popover';
import Tooltip from 'Components/Tooltip/Tooltip';
import EpisodeFormats from 'Episode/EpisodeFormats';
import EpisodeLanguages from 'Episode/EpisodeLanguages';
import EpisodeQuality from 'Episode/EpisodeQuality';
import IndexerFlags from 'Episode/IndexerFlags';
import { icons, kinds, tooltipPositions } from 'Helpers/Props';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import Release from 'typings/Release';
import formatDateTime from 'Utilities/Date/formatDateTime';
import formatAge from 'Utilities/Number/formatAge';
import formatBytes from 'Utilities/Number/formatBytes';
import formatCustomFormatScore from 'Utilities/Number/formatCustomFormatScore';
import translate from 'Utilities/String/translate';
import OverrideMatchModal from './OverrideMatch/OverrideMatchModal';
import Peers from './Peers';
import ReleaseSceneIndicator from './ReleaseSceneIndicator';
import styles from './InteractiveSearchRow.css';

function getDownloadIcon(
  isGrabbing: boolean,
  isGrabbed: boolean,
  grabError?: string
) {
  if (isGrabbing) {
    return icons.SPINNER;
  } else if (isGrabbed) {
    return icons.DOWNLOADING;
  } else if (grabError) {
    return icons.DOWNLOADING;
  }

  return icons.DOWNLOAD;
}

function getDownloadKind(isGrabbed: boolean, grabError?: string) {
  if (isGrabbed) {
    return kinds.SUCCESS;
  }

  if (grabError) {
    return kinds.DANGER;
  }

  return kinds.DEFAULT;
}

function getDownloadTooltip(
  isGrabbing: boolean,
  isGrabbed: boolean,
  grabError?: string
) {
  if (isGrabbing) {
    return '';
  } else if (isGrabbed) {
    return translate('AddedToDownloadQueue');
  } else if (grabError) {
    return grabError;
  }

  return translate('AddToDownloadQueue');
}

interface InteractiveSearchRowProps extends Release {
  searchPayload: {
    episodeId?: number;
    seriesId?: number;
    seasonNumber?: number;
  };
  onGrabPress(...args: unknown[]): void;
  onForceDownloadPress?(...args: unknown[]): void;
}

function InteractiveSearchRow(props: InteractiveSearchRowProps) {
  const {
    guid,
    indexerId,
    protocol,
    age,
    ageHours,
    ageMinutes,
    publishDate,
    title,
    infoUrl,
    indexer,
    size,
    seeders,
    leechers,
    quality,
    languages,
    customFormatScore,
    customFormats,
    sceneMapping,
    seasonNumber,
    episodeNumbers,
    absoluteEpisodeNumbers,
    mappedSeriesId,
    mappedSeasonNumber,
    mappedEpisodeNumbers,
    mappedAbsoluteEpisodeNumbers,
    mappedEpisodeInfo,
    indexerFlags = 0,
    rejections = [],
    episodeRequested,
    downloadAllowed,
    isDaily,
    isGrabbing = false,
    isGrabbed = false,
    grabError,
    searchPayload,
    onGrabPress,
    onForceDownloadPress,
  } = props;

  const { longDateFormat, timeFormat } = useSelector(
    createUISettingsSelector()
  );

  const [isConfirmGrabModalOpen, setIsConfirmGrabModalOpen] = useState(false);
  const [isOverrideModalOpen, setIsOverrideModalOpen] = useState(false);

  const onGrabPressWrapper = useCallback(() => {
    if (downloadAllowed) {
      onGrabPress({
        guid,
        indexerId,
      });

      return;
    }

    setIsConfirmGrabModalOpen(true);
  }, [
    guid,
    indexerId,
    downloadAllowed,
    onGrabPress,
    setIsConfirmGrabModalOpen,
  ]);

  const onGrabConfirm = useCallback(() => {
    setIsConfirmGrabModalOpen(false);

    onGrabPress({
      guid,
      indexerId,
      ...searchPayload,
    });
  }, [guid, indexerId, searchPayload, onGrabPress, setIsConfirmGrabModalOpen]);

  const onGrabCancel = useCallback(() => {
    setIsConfirmGrabModalOpen(false);
  }, [setIsConfirmGrabModalOpen]);

  const onOverridePress = useCallback(() => {
    setIsOverrideModalOpen(true);
  }, [setIsOverrideModalOpen]);

  const onOverrideModalClose = useCallback(() => {
    setIsOverrideModalOpen(false);
  }, [setIsOverrideModalOpen]);

  const onForceDownloadPressWrapper = useCallback(() => {
    if (onForceDownloadPress) {
      // Get episode IDs and series ID from available data
      let episodeIds: number[] = [];
      let seriesId = mappedSeriesId;

      // Try to get episode IDs from mappedEpisodeInfo first
      if (mappedEpisodeInfo && mappedEpisodeInfo.length > 0) {
        episodeIds = mappedEpisodeInfo.map(
          (episode: { id: number }) => episode.id
        );
      }
      // If no mapped episode info, check if this is an episode search (searchPayload.episodeId)
      else if (searchPayload && searchPayload.episodeId) {
        episodeIds = [searchPayload.episodeId];
      }

      // Get series ID from searchPayload if not available in mappedSeriesId
      if (!seriesId && searchPayload && searchPayload.seriesId) {
        seriesId = searchPayload.seriesId;
      }

      onForceDownloadPress({
        guid,
        indexerId,
        ...searchPayload,
        shouldOverride: true,
        seriesId,
        episodeIds,
        quality,
        languages,
      });
    }
  }, [
    guid,
    indexerId,
    searchPayload,
    onForceDownloadPress,
    mappedSeriesId,
    mappedEpisodeInfo,
    quality,
    languages,
  ]);

  return (
    <TableRow>
      <TableRowCell className={styles.protocol}>
        <ProtocolLabel protocol={protocol} />
      </TableRowCell>

      <TableRowCell
        className={styles.age}
        title={formatDateTime(publishDate, longDateFormat, timeFormat, {
          includeSeconds: true,
        })}
      >
        {formatAge(age, ageHours, ageMinutes)}
      </TableRowCell>

      <TableRowCell>
        <div className={styles.titleContent}>
          <Link to={infoUrl}>{title}</Link>
          <ReleaseSceneIndicator
            className={styles.sceneMapping}
            seasonNumber={mappedSeasonNumber}
            episodeNumbers={mappedEpisodeNumbers}
            absoluteEpisodeNumbers={mappedAbsoluteEpisodeNumbers}
            sceneSeasonNumber={seasonNumber}
            sceneEpisodeNumbers={episodeNumbers}
            sceneAbsoluteEpisodeNumbers={absoluteEpisodeNumbers}
            sceneMapping={sceneMapping}
            episodeRequested={episodeRequested}
            isDaily={isDaily}
          />
        </div>
      </TableRowCell>

      <TableRowCell className={styles.indexer}>{indexer}</TableRowCell>

      <TableRowCell className={styles.size}>{formatBytes(size)}</TableRowCell>

      <TableRowCell className={styles.peers}>
        {protocol === 'torrent' ? (
          <Peers seeders={seeders} leechers={leechers} />
        ) : null}
      </TableRowCell>

      <TableRowCell className={styles.languages}>
        <EpisodeLanguages languages={languages} />
      </TableRowCell>

      <TableRowCell className={styles.quality}>
        <EpisodeQuality quality={quality} showRevision={true} />
      </TableRowCell>

      <TableRowCell className={styles.customFormatScore}>
        <Tooltip
          anchor={formatCustomFormatScore(
            customFormatScore,
            customFormats.length
          )}
          tooltip={<EpisodeFormats formats={customFormats} />}
          position={tooltipPositions.LEFT}
        />
      </TableRowCell>

      <TableRowCell className={styles.indexerFlags}>
        {indexerFlags ? (
          <Popover
            anchor={<Icon name={icons.FLAG} kind={kinds.PRIMARY} />}
            title={translate('IndexerFlags')}
            body={<IndexerFlags indexerFlags={indexerFlags} />}
            position={tooltipPositions.LEFT}
          />
        ) : null}
      </TableRowCell>

      <TableRowCell className={styles.rejected}>
        {rejections.length ? (
          <Popover
            anchor={<Icon name={icons.DANGER} kind={kinds.DANGER} />}
            title={translate('ReleaseRejected')}
            body={
              <ul>
                {rejections.map((rejection, index) => {
                  return <li key={index}>{rejection}</li>;
                })}
              </ul>
            }
            position={tooltipPositions.LEFT}
          />
        ) : null}
      </TableRowCell>

      <TableRowCell className={styles.download}>
        <SpinnerIconButton
          name={getDownloadIcon(isGrabbing, isGrabbed, grabError)}
          kind={getDownloadKind(isGrabbed, grabError)}
          title={getDownloadTooltip(isGrabbing, isGrabbed, grabError)}
          isSpinning={isGrabbing}
          onPress={onGrabPressWrapper}
        />

        {onForceDownloadPress && (
          <Link
            className={styles.forceDownloadContent}
            title={translate('ForceDownload')}
            onPress={onForceDownloadPressWrapper}
          >
            <div className={styles.forceDownloadContent}>
              <Icon
                className={styles.forceIcon}
                name={icons.ACTIONS}
                size={12}
              />

              <Icon
                className={styles.downloadIcon}
                name={icons.CIRCLE_DOWN}
                size={10}
              />
            </div>
          </Link>
        )}

        <Link
          className={styles.manualDownloadContent}
          title={translate('OverrideAndAddToDownloadQueue')}
          onPress={onOverridePress}
        >
          <div className={styles.manualDownloadContent}>
            <Icon
              className={styles.interactiveIcon}
              name={icons.INTERACTIVE}
              size={12}
            />

            <Icon
              className={styles.downloadIcon}
              name={icons.CIRCLE_DOWN}
              size={10}
            />
          </div>
        </Link>
      </TableRowCell>

      <ConfirmModal
        isOpen={isConfirmGrabModalOpen}
        kind={kinds.WARNING}
        title={translate('GrabRelease')}
        message={translate('GrabReleaseMessageText', { title })}
        confirmLabel={translate('Grab')}
        onConfirm={onGrabConfirm}
        onCancel={onGrabCancel}
      />

      <OverrideMatchModal
        isOpen={isOverrideModalOpen}
        title={title}
        indexerId={indexerId}
        guid={guid}
        seriesId={mappedSeriesId}
        seasonNumber={mappedSeasonNumber}
        episodes={mappedEpisodeInfo}
        languages={languages}
        quality={quality}
        protocol={protocol}
        isGrabbing={isGrabbing}
        grabError={grabError}
        onModalClose={onOverrideModalClose}
      />
    </TableRow>
  );
}

export default InteractiveSearchRow;
