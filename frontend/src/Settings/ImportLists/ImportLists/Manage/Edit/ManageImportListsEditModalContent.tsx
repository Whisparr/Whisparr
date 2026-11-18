import React, { useCallback, useState } from 'react';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import { EnhancedSelectInputValue } from 'Components/Form/Select/EnhancedSelectInput';
import Button from 'Components/Link/Button';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { inputTypes } from 'Helpers/Props';
import { InputChanged } from 'typings/inputs';
import translate from 'Utilities/String/translate';
import styles from './ManageImportListsEditModalContent.css';

interface SavePayload {
  enableAutomaticAdd?: boolean;
  qualityProfileId?: number;
  rootFolderPath?: string;
  searchForMissingEpisodes?: boolean;
  shouldMonitor?: string;
  siteMonitorType?: string;
  monitorNewItems?: string;
}

interface ManageImportListsEditModalContentProps {
  importListIds: number[];
  onSavePress(payload: object): void;
  onModalClose(): void;
}

const NO_CHANGE = 'noChange';

const autoAddOptions: EnhancedSelectInputValue<string>[] = [
  {
    key: NO_CHANGE,
    get value() {
      return translate('NoChange');
    },
    isDisabled: true,
  },
  {
    key: 'enabled',
    get value() {
      return translate('Enabled');
    },
  },
  {
    key: 'disabled',
    get value() {
      return translate('Disabled');
    },
  },
];

const searchForMissingEpisodesOptions = [
  {
    key: NO_CHANGE,
    get value() {
      return translate('NoChange');
    },
    disabled: true,
  },
  {
    key: 'enabled',
    get value() {
      return translate('Enabled');
    },
  },
  {
    key: 'disabled',
    get value() {
      return translate('Disabled');
    },
  },
];

const shouldMonitorOptions = [
  {
    key: NO_CHANGE,
    get value() {
      return translate('NoChange');
    },
    disabled: true,
  },
  {
    key: 'none',
    get value() {
      return translate('None');
    },
  },
  {
    key: 'specificEpisode',
    get value() {
      return translate('SpecificEpisode');
    },
  },
  {
    key: 'entireSite',
    get value() {
      return translate('AllSiteEpisodes');
    },
  },
];

function ManageImportListsEditModalContent(
  props: ManageImportListsEditModalContentProps
) {
  const { importListIds, onSavePress, onModalClose } = props;

  const [enableAutomaticAdd, setEnableAutomaticAdd] = useState(NO_CHANGE);
  const [qualityProfileId, setQualityProfileId] = useState<string | number>(
    NO_CHANGE
  );
  const [rootFolderPath, setRootFolderPath] = useState(NO_CHANGE);
  const [searchForMissingEpisodes, setSearchForMissingEpisodes] =
    useState(NO_CHANGE);
  const [shouldMonitor, setShouldMonitor] = useState(NO_CHANGE);
  const [siteMonitorType, setSiteMonitorType] = useState(NO_CHANGE);
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

    if (siteMonitorType !== NO_CHANGE) {
      hasChanges = true;
      payload.siteMonitorType = siteMonitorType;
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
    siteMonitorType,
    monitorNewItems,
    onSavePress,
    onModalClose,
  ]);

  const onInputChange = useCallback(({ name, value }: InputChanged) => {
    switch (name) {
      case 'enableAutomaticAdd':
        setEnableAutomaticAdd(value as string);
        break;
      case 'qualityProfileId':
        setQualityProfileId(value as number);
        break;
      case 'rootFolderPath':
        setRootFolderPath(value as string);
        break;
      case 'searchForMissingEpisodes':
        setSearchForMissingEpisodes(value as string);
        break;
      case 'shouldMonitor':
        setShouldMonitor(value as string);
        break;
      case 'siteMonitorType':
        setSiteMonitorType(value as string);
        break;
      case 'monitorNewItems':
        setMonitorNewItems(value as string);
        break;
      default:
        console.warn(
          `EditImportListsEditModalContent Unknown Input: '${name}'`
        );
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
          <FormLabel>{translate('Monitor')}</FormLabel>
          <FormInputGroup
            type={inputTypes.SELECT}
            name="shouldMonitor"
            value={shouldMonitor}
            values={shouldMonitorOptions}
            onChange={onInputChange}
          />
        </FormGroup>

        {shouldMonitor === 'entireSite' && (
          <FormGroup>
            <FormLabel>{translate('SiteMonitoringOptions')}</FormLabel>
            <FormInputGroup
              type={inputTypes.MONITOR_EPISODES_SELECT}
              name="siteMonitorType"
              value={siteMonitorType}
              includeNoChange={true}
              includeNoChangeDisabled={false}
              onChange={onInputChange}
            />
          </FormGroup>
        )}

        <FormGroup>
          <FormLabel>{translate('MonitorNewScenes')}</FormLabel>
          <FormInputGroup
            type={inputTypes.MONITOR_NEW_ITEMS_SELECT}
            name="monitorNewItems"
            value={monitorNewItems}
            includeNoChange={true}
            includeNoChangeDisabled={false}
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
