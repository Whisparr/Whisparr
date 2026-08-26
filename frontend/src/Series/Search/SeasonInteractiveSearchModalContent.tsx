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

interface SeasonInteractiveSearchModalContentProps {
  seriesId: number;
  seasonNumber: number;
  onModalClose(): void;
}

function SeasonInteractiveSearchModalContent(
  props: SeasonInteractiveSearchModalContentProps
) {
  const { seriesId, seasonNumber, onModalClose } = props;

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

      <ModalFooter>
        <Button onPress={onModalClose}>{translate('Close')}</Button>
      </ModalFooter>
    </ModalContent>
  );
}

export default SeasonInteractiveSearchModalContent;
