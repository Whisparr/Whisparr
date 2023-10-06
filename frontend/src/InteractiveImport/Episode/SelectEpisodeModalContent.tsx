import React, { useCallback, useEffect, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import EpisodesAppState from 'App/State/EpisodesAppState';
import TextInput from 'Components/Form/TextInput';
import Button from 'Components/Link/Button';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import Scroller from 'Components/Scroller/Scroller';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import Episode from 'Episode/Episode';
import useSelectState from 'Helpers/Hooks/useSelectState';
import { kinds, scrollDirections } from 'Helpers/Props';
import SortDirection from 'Helpers/Props/SortDirection';
import {
  clearEpisodes,
  fetchEpisodes,
  setEpisodesSort,
} from 'Store/Actions/episodeSelectionActions';
import createClientSideCollectionSelector from 'Store/Selectors/createClientSideCollectionSelector';
import { CheckInputChanged } from 'typings/inputs';
import { SelectStateInputProps } from 'typings/props';
import getErrorMessage from 'Utilities/Object/getErrorMessage';
import translate from 'Utilities/String/translate';
import getSelectedIds from 'Utilities/Table/getSelectedIds';
import SelectEpisodeRow from './SelectEpisodeRow';
import styles from './SelectEpisodeModalContent.css';

const columns = [
  {
    name: 'title',
    label: () => translate('Title'),
    isVisible: true,
  },
  {
    name: 'actors',
    label: () => translate('Performers'),
    isVisible: true,
  },
  {
    name: 'releaseDate',
    label: () => translate('ReleaseDate'),
    isVisible: true,
  },
];

function episodesSelector() {
  return createSelector(
    createClientSideCollectionSelector('episodeSelection'),
    (episodes: EpisodesAppState) => {
      return episodes;
    }
  );
}

export interface SelectedEpisode {
  id: number;
  episodes: Episode[];
}

interface SelectEpisodeModalContentProps {
  selectedIds: number[] | string[];
  seriesId?: number;
  seasonNumber?: number;
  selectedDetails?: string;
  modalTitle: string;
  onEpisodesSelect(selectedEpisodes: SelectedEpisode[]): unknown;
  onModalClose(): unknown;
}

//
// Render

function SelectEpisodeModalContent(props: SelectEpisodeModalContentProps) {
  const {
    selectedIds,
    seriesId,
    seasonNumber,
    selectedDetails,
    modalTitle,
    onEpisodesSelect,
    onModalClose,
  } = props;

  const [filter, setFilter] = useState('');
  const [selectState, setSelectState] = useSelectState();

  const { allSelected, allUnselected, selectedState } = selectState;
  const { isFetching, isPopulated, items, error, sortKey, sortDirection } =
    useSelector(episodesSelector());
  const dispatch = useDispatch();

  const errorMessage = getErrorMessage(error, translate('EpisodesLoadError'));
  const selectedCount = selectedIds.length;
  const selectedEpisodesCount = getSelectedIds(selectedState).length;
  const selectionIsValid =
    selectedEpisodesCount > 0 && selectedEpisodesCount % selectedCount === 0;

  const onFilterChange = useCallback(
    ({ value }: { value: string }) => {
      setFilter(value.toLowerCase());
    },
    [setFilter]
  );

  const onSelectAllChange = useCallback(
    ({ value }: CheckInputChanged) => {
      setSelectState({ type: value ? 'selectAll' : 'unselectAll', items });
    },
    [items, setSelectState]
  );

  const onSelectedChange = useCallback(
    ({ id, value, shiftKey = false }: SelectStateInputProps) => {
      setSelectState({
        type: 'toggleSelected',
        items,
        id,
        isSelected: value,
        shiftKey,
      });
    },
    [items, setSelectState]
  );

  const onSortPress = useCallback(
    (newSortKey: string, newSortDirection: SortDirection) => {
      dispatch(
        setEpisodesSort({
          sortKey: newSortKey,
          sortDirection: newSortDirection,
        })
      );
    },
    [dispatch]
  );

  const onEpisodesSelectWrapper = useCallback(() => {
    const episodeIds: number[] = getSelectedIds(selectedState);

    const selectedEpisodes = items.reduce((acc: Episode[], item) => {
      if (episodeIds.indexOf(item.id) > -1) {
        acc.push(item);
      }

      return acc;
    }, []);

    const episodesPerFile = selectedEpisodes.length / selectedIds.length;
    const sortedEpisodes = selectedEpisodes.sort((a, b) => {
      return a.seasonNumber - b.seasonNumber;
    });

    const mappedEpisodes = selectedIds.map((id, index): SelectedEpisode => {
      const startingIndex = index * episodesPerFile;
      const episodes = sortedEpisodes.slice(
        startingIndex,
        startingIndex + episodesPerFile
      );

      return {
        id: id as number,
        episodes,
      };
    });

    onEpisodesSelect(mappedEpisodes);
  }, [selectedIds, items, selectedState, onEpisodesSelect]);

  useEffect(
    () => {
      dispatch(fetchEpisodes({ seriesId, seasonNumber }));

      return () => {
        dispatch(clearEpisodes());
      };
    },
    // eslint-disable-next-line react-hooks/exhaustive-deps
    []
  );

  let details = selectedDetails;

  if (!details) {
    details =
      selectedCount > 1
        ? translate('CountSelectedFiles', { selectedCount })
        : translate('CountSelectedFile', { selectedCount });
  }

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {translate('SelectEpisodesModalTitle', { modalTitle })}
      </ModalHeader>

      <ModalBody
        className={styles.modalBody}
        scrollDirection={scrollDirections.NONE}
      >
        <TextInput
          className={styles.filterInput}
          placeholder={translate('FilterEpisodesPlaceholder')}
          name="filter"
          value={filter}
          autoFocus={true}
          onChange={onFilterChange}
        />

        <Scroller className={styles.scroller} autoFocus={false}>
          {isFetching ? <LoadingIndicator /> : null}

          {error ? <div>{errorMessage}</div> : null}

          {isPopulated && !!items.length ? (
            <Table
              columns={columns}
              selectAll={true}
              allSelected={allSelected}
              allUnselected={allUnselected}
              sortKey={sortKey}
              sortDirection={sortDirection}
              onSortPress={onSortPress}
              onSelectAllChange={onSelectAllChange}
            >
              <TableBody>
                {items.map((item) => {
                  return item.title.toLowerCase().includes(filter) ? (
                    <SelectEpisodeRow
                      key={item.id}
                      id={item.id}
                      title={item.title}
                      actors={item.actors}
                      releaseDate={item.releaseDate}
                      isSelected={selectedState[item.id]}
                      onSelectedChange={onSelectedChange}
                    />
                  ) : null;
                })}
              </TableBody>
            </Table>
          ) : null}

          {isPopulated && !items.length
            ? translate('NoEpisodesFoundForSelectedYear')
            : null}
        </Scroller>
      </ModalBody>

      <ModalFooter className={styles.footer}>
        <div className={styles.details}>{details}</div>

        <div className={styles.buttons}>
          <Button onPress={onModalClose}>{translate('Cancel')}</Button>

          <Button
            kind={kinds.SUCCESS}
            isDisabled={!selectionIsValid}
            onPress={onEpisodesSelectWrapper}
          >
            {translate('SelectEpisodes')}
          </Button>
        </div>
      </ModalFooter>
    </ModalContent>
  );
}

export default SelectEpisodeModalContent;
