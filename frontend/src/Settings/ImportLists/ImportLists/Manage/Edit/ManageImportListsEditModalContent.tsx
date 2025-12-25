import React, { useCallback, useState } from 'react';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import Button from 'Components/Link/Button';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { inputTypes } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import styles from './ManageImportListsEditModalContent.css';

interface SavePayload {
  enableAutomaticAdd?: boolean;
  qualityProfileId?: number;
  rootFolderPath?: string;
  searchForMissingEpisodes?: boolean;
  shouldMonitor?: string;
  monitorNewItems?: string;
}

interface ManageImportListsEditModalContentProps {
  importListIds: number[];
  onSavePress(payload: object): void;
  onModalClose(): void;
}

const NO_CHANGE = 'noChange';

const autoAddOptions = [
  { key: NO_CHANGE, value: translate('NoChange'), disabled: true },
  { key: 'enabled', value: translate('Enabled') },
  { key: 'disabled', value: translate('Disabled') }
];

const searchForMissingEpisodesOptions = [
  { key: NO_CHANGE, value: translate('NoChange'), disabled: true },
  { key: 'enabled', value: translate('Enabled') },
  { key: 'disabled', value: translate('Disabled') }
];

const shouldMonitorOptions = [
  { key: NO_CHANGE, value: translate('NoChange'), disabled: true },
  { key: 'none', value: translate('None') },
  { key: 'specificEpisode', value: translate('SpecificEpisode') },
  { key: 'entireSite', value: translate('EntireSite') }
];

const monitorNewItemsOptions = [
  { key: NO_CHANGE, value: translate('NoChange'), disabled: true },
  { key: 'none', value: translate('None') },
  { key: 'all', value: translate('All') }
];

function ManageImportListsEditModalContent(props: ManageImportListsEditModalContentProps) {
  const { importListIds, onSavePress, onModalClose } = props;

  const [enableAutomaticAdd, setEnableAutomaticAdd] = useState(NO_CHANGE);
  const [qualityProfileId, setQualityProfileId] = useState<string | number>(NO_CHANGE);
  const [rootFolderPath, setRootFolderPath] = useState(NO_CHANGE);
  const [searchForMissingEpisodes, setSearchForMissingEpisodes] = useState(NO_CHANGE);
  const [shouldMonitor, setShouldMonitor] = useState(NO_CHANGE);
  const [monitorNewItems, setMonitorNewItems] = useState(NO_CHANGE);

  const save = useCallback(() => {
    let hasChanges = false;
    const payload: SavePayload = {};

    if (enableAutomaticAdd !== NO_CHANGE) {
      hasChanges = true;
      payload.enableAutomaticAdd = enableAutomaticAdd === 'enabled';
    }

    if (qualityProfileId !== NO_CHANGE) {
      hasChanges = true;
      payload.qualityProfileId = qualityProfileId as number;
    }

    if (rootFolderPath !== NO_CHANGE) {
      hasChanges = true;
      payload.rootFolderPath = rootFolderPath;
    }

    if (searchForMissingEpisodes !== NO_CHANGE) {
      hasChanges = true;
      payload.searchForMissingEpisodes = searchForMissingEpisodes === 'enabled';
    }

    if (shouldMonitor !== NO_CHANGE) {
      hasChanges = true;
      payload.shouldMonitor = shouldMonitor;
    }

    if (monitorNewItems !== NO_CHANGE) {
      hasChanges = true;
      payload.monitorNewItems = monitorNewItems;
    }

    if (hasChanges) {
      onSavePress(payload);
    }

    onModalClose();
  }, [
    enableAutomaticAdd,
    qualityProfileId,
    rootFolderPath,
    searchForMissingEpisodes,
    shouldMonitor,
    monitorNewItems,
    onSavePress,
    onModalClose
  ]);

  const onInputChange = useCallback(({ name, value }: { name: string; value: string }) => {
    switch (name) {
      case 'enableAutomaticAdd':
        setEnableAutomaticAdd(value);
        break;
      case 'qualityProfileId':
        setQualityProfileId(value);
        break;
      case 'rootFolderPath':
        setRootFolderPath(value);
        break;
      case 'searchForMissingEpisodes':
        setSearchForMissingEpisodes(value);
        break;
      case 'shouldMonitor':
        setShouldMonitor(value);
        break;
      case 'monitorNewItems':
        setMonitorNewItems(value);
        break;
    }
  }, []);

  const selectedCount = importListIds.length;

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>{translate('EditSelectedImportLists')}</ModalHeader>

      <ModalBody>
        <FormGroup>
          <FormLabel>{translate('AutomaticAdd')}</FormLabel>
          <FormInputGroup
            type={inputTypes.SELECT}
            name="enableAutomaticAdd"
            value={enableAutomaticAdd}
            values={autoAddOptions}
            onChange={onInputChange}
          />
        </FormGroup>

        <FormGroup>
          <FormLabel>{translate('QualityProfile')}</FormLabel>
          <FormInputGroup
            type={inputTypes.QUALITY_PROFILE_SELECT}
            name="qualityProfileId"
            value={qualityProfileId}
            includeNoChange={true}
            includeNoChangeDisabled={false}
            onChange={onInputChange}
          />
        </FormGroup>

        <FormGroup>
          <FormLabel>{translate('RootFolder')}</FormLabel>
          <FormInputGroup
            type={inputTypes.ROOT_FOLDER_SELECT}
            name="rootFolderPath"
            value={rootFolderPath}
            includeNoChange={true}
            includeNoChangeDisabled={false}
            selectedValueOptions={{ includeFreeSpace: false }}
            onChange={onInputChange}
          />
        </FormGroup>

        <FormGroup>
          <FormLabel>{translate('SearchForMissingEpisodes')}</FormLabel>
          <FormInputGroup
            type={inputTypes.SELECT}
            name="searchForMissingEpisodes"
            value={searchForMissingEpisodes}
            values={searchForMissingEpisodesOptions}
            onChange={onInputChange}
          />
        </FormGroup>

        <FormGroup>
          <FormLabel>{translate('ShouldMonitor')}</FormLabel>
          <FormInputGroup
            type={inputTypes.SELECT}
            name="shouldMonitor"
            value={shouldMonitor}
            values={shouldMonitorOptions}
            onChange={onInputChange}
          />
        </FormGroup>

        <FormGroup>
          <FormLabel>{translate('MonitorNewItems')}</FormLabel>
          <FormInputGroup
            type={inputTypes.SELECT}
            name="monitorNewItems"
            value={monitorNewItems}
            values={monitorNewItemsOptions}
            onChange={onInputChange}
          />
        </FormGroup>
      </ModalBody>

      <ModalFooter className={styles.modalFooter}>
        <div className={styles.selected}>
          {translate('CountImportListsSelected', { count: selectedCount })}
        </div>

        <div>
          <Button onPress={onModalClose}>{translate('Cancel')}</Button>
          <Button onPress={save}>{translate('ApplyChanges')}</Button>
        </div>
      </ModalFooter>
    </ModalContent>
  );
}

export default ManageImportListsEditModalContent;
