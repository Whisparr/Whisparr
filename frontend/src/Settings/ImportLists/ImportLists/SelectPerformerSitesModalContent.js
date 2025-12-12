import PropTypes from 'prop-types';
import React, { useCallback, useEffect, useMemo, useState } from 'react';
import Alert from 'Components/Alert';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import SpinnerButton from 'Components/Link/SpinnerButton';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import Scroller from 'Components/Scroller/Scroller';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import useSelectState from 'Helpers/Hooks/useSelectState';
import { icons, kinds, scrollDirections } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import getSelectedIds from 'Utilities/Table/getSelectedIds';
import requestAction from 'Utilities/requestAction';
import SelectPerformerSitesRow from './SelectPerformerSitesRow';
import styles from './SelectPerformerSitesModalContent.css';

const PAGE_SIZE = 25;

const columns = [
  {
    name: 'title',
    label: () => translate('Site'),
    isVisible: true
  },
  {
    name: 'year',
    label: () => translate('Year'),
    isVisible: true
  },
  {
    name: 'sceneCount',
    label: () => translate('Scenes'),
    isVisible: true
  },
  {
    name: 'exists',
    label: () => translate('InLibrary'),
    isVisible: true
  }
];

function SelectPerformerSitesModalContent(props) {
  const {
    providerData,
    onSitesSelected,
    onModalClose
  } = props;

  const [isFetching, setIsFetching] = useState(true);
  const [error, setError] = useState(null);
  const [sites, setSites] = useState([]);
  const [currentPage, setCurrentPage] = useState(1);
  const [selectState, setSelectState] = useSelectState();

  const { allSelected, allUnselected, selectedState } = selectState;

  const performerId = useMemo(() => {
    const field = providerData.fields?.find((f) => f.name === 'performerId');
    return field?.value || '';
  }, [providerData.fields]);

  const currentExcludedSiteIds = useMemo(() => {
    const field = providerData.fields?.find((f) => f.name === 'excludedSiteIds');
    return new Set(field?.value || []);
  }, [providerData.fields]);

  // Check if this is a new import list (no ID) vs editing existing
  const isNewList = !providerData.id;

  const totalSites = sites.length;
  const totalPages = Math.ceil(totalSites / PAGE_SIZE);
  const startIndex = (currentPage - 1) * PAGE_SIZE;
  const endIndex = Math.min(startIndex + PAGE_SIZE, totalSites);
  const currentPageSites = sites.slice(startIndex, endIndex);

  const selectedSiteIds = getSelectedIds(selectedState);
  const excludedSiteIds = sites
    .filter((s) => !selectedState[s.tvdbId])
    .map((s) => s.tvdbId);

  const fetchSites = useCallback(async () => {
    setIsFetching(true);
    setError(null);

    try {
      const promise = requestAction({
        provider: 'importlist',
        action: 'previewPerformer',
        providerData,
        queryParams: { performerId }
      });

      promise.done((data) => {
        const sitesWithId = (data.sites || []).map((site) => ({
          ...site,
          id: site.tvdbId
        }));

        setSites(sitesWithId);

        const initialSelectedState = sitesWithId.reduce((acc, site) => {
          if (isNewList) {
            // New list: all sites start unselected
            acc[site.tvdbId] = false;
          } else {
            // Existing list: excluded sites are unselected, others are selected
            acc[site.tvdbId] = !currentExcludedSiteIds.has(site.tvdbId);
          }
          return acc;
        }, {});

        setSelectState({
          type: 'updateItems',
          items: sitesWithId
        });

        Object.entries(initialSelectedState).forEach(([id, value]) => {
          setSelectState({
            type: 'toggleSelected',
            items: sitesWithId,
            id: parseInt(id),
            isSelected: value,
            shiftKey: false
          });
        });

        setIsFetching(false);
      });

      promise.fail((xhr) => {
        setError(xhr);
        setIsFetching(false);
      });
    } catch (err) {
      setError(err);
      setIsFetching(false);
    }
  }, [providerData, performerId, isNewList, currentExcludedSiteIds, setSelectState]);

  useEffect(() => {
    if (performerId) {
      fetchSites();
    }
  }, [performerId, fetchSites]);

  const onSelectAllChange = useCallback(
    ({ value }) => {
      setSelectState({ type: value ? 'selectAll' : 'unselectAll', items: sites });
    },
    [sites, setSelectState]
  );

  const onSelectedChange = useCallback(
    ({ id, value, shiftKey = false }) => {
      setSelectState({
        type: 'toggleSelected',
        items: sites,
        id,
        isSelected: value,
        shiftKey
      });
    },
    [sites, setSelectState]
  );

  const onPreviousPagePress = useCallback(() => {
    setCurrentPage((prev) => Math.max(1, prev - 1));
  }, []);

  const onNextPagePress = useCallback(() => {
    setCurrentPage((prev) => Math.min(totalPages, prev + 1));
  }, [totalPages]);

  const onConfirmPress = useCallback(() => {
    onSitesSelected(excludedSiteIds);
    onModalClose();
  }, [excludedSiteIds, onSitesSelected, onModalClose]);

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {translate('SelectSitesToImport')} ({totalSites} {totalSites === 1 ? 'site' : 'sites'})
      </ModalHeader>

      <ModalBody
        className={styles.modalBody}
        scrollDirection={scrollDirections.NONE}
      >
        {isFetching ? <LoadingIndicator /> : null}

        {!isFetching && error ? (
          <Alert kind={kinds.DANGER}>
            {translate('ErrorLoadingSites')}
          </Alert>
        ) : null}

        {!isFetching && !error && sites.length === 0 ? (
          <Alert kind={kinds.INFO}>
            {translate('NoSitesFound')}
          </Alert>
        ) : null}

        {!isFetching && !error && sites.length > 0 ? (
          <Scroller className={styles.scroller} autoFocus={false}>
            <Table
              columns={columns}
              selectAll={true}
              allSelected={allSelected}
              allUnselected={allUnselected}
              onSelectAllChange={onSelectAllChange}
            >
              <TableBody>
                {currentPageSites.map((site) => (
                  <SelectPerformerSitesRow
                    key={site.tvdbId}
                    id={site.tvdbId}
                    title={site.title}
                    year={site.year}
                    sceneCount={site.sceneCount}
                    exists={site.exists}
                    isSelected={selectedState[site.tvdbId]}
                    onSelectedChange={onSelectedChange}
                  />
                ))}
              </TableBody>
            </Table>
          </Scroller>
        ) : null}
      </ModalBody>

      <ModalFooter className={styles.footer}>
        <div className={styles.pageInfo}>
          {totalPages > 1 ? (
            <div className={styles.pagination}>
              <Button
                isDisabled={currentPage === 1}
                onPress={onPreviousPagePress}
              >
                <Icon name={icons.PAGE_PREVIOUS} />
              </Button>
              <span className={styles.pageText}>
                Page {currentPage} of {totalPages}
              </span>
              <Button
                isDisabled={currentPage === totalPages}
                onPress={onNextPagePress}
              >
                <Icon name={icons.PAGE_NEXT} />
              </Button>
            </div>
          ) : null}
          <span className={styles.selectionInfo}>
            {selectedSiteIds.length} of {totalSites} selected
          </span>
        </div>

        <div className={styles.buttons}>
          <Button onPress={onModalClose}>
            {translate('Cancel')}
          </Button>

          <SpinnerButton
            kind={kinds.SUCCESS}
            isDisabled={selectedSiteIds.length === 0}
            onPress={onConfirmPress}
          >
            {translate('Confirm')}
          </SpinnerButton>
        </div>
      </ModalFooter>
    </ModalContent>
  );
}

SelectPerformerSitesModalContent.propTypes = {
  providerData: PropTypes.object.isRequired,
  onSitesSelected: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default SelectPerformerSitesModalContent;