import React, { useMemo } from 'react';
import Button from 'Components/Link/Button';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { scrollDirections } from 'Helpers/Props';
import InteractiveSearch from 'InteractiveSearch/InteractiveSearch';
import formatSeason from 'Season/formatSeason';
import translate from 'Utilities/String/translate';
import styles from './SeasonInteractiveSearchModalContent.css';

export interface SeasonInteractiveSearchModalContentProps {
  episodeCount: number;
  seriesId: number;
  seasonNumber: number;
  onModalClose(): void;
}

function SeasonInteractiveSearchModalContent({
  episodeCount,
  seriesId,
  seasonNumber,
  onModalClose,
}: SeasonInteractiveSearchModalContentProps) {
  // Must keep a stable identity: InteractiveSearch refetches whenever this
  // changes, so a fresh object each render loops the search forever.
  const searchPayload = useMemo(
    () => ({ seriesId, seasonNumber }),
    [seriesId, seasonNumber]
  );
  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {seasonNumber === null
          ? translate('InteractiveSearchModalHeader')
          : translate('InteractiveSearchModalHeaderSeason', {
              season: formatSeason(seasonNumber) as string,
            })}
      </ModalHeader>

      <ModalBody scrollDirection={scrollDirections.BOTH}>
        <InteractiveSearch type="season" searchPayload={searchPayload} />
      </ModalBody>

      <ModalFooter className={styles.modalFooter}>
        <div>
          {translate('EpisodesInSeason', {
            episodeCount,
          })}
        </div>

        <Button onPress={onModalClose}>{translate('Close')}</Button>
      </ModalFooter>
    </ModalContent>
  );
}

export default SeasonInteractiveSearchModalContent;
