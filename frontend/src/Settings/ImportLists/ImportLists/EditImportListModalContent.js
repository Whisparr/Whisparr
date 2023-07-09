import PropTypes from 'prop-types';
import React from 'react';
import SeriesMonitoringOptionsPopoverContent from 'AddSeries/SeriesMonitoringOptionsPopoverContent';
import Alert from 'Components/Alert';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import ProviderFieldFormGroup from 'Components/Form/ProviderFieldFormGroup';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import SpinnerErrorButton from 'Components/Link/SpinnerErrorButton';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import Popover from 'Components/Tooltip/Popover';
import { icons, inputTypes, kinds, tooltipPositions } from 'Helpers/Props';
import formatShortTimeSpan from 'Utilities/Date/formatShortTimeSpan';
import styles from './EditImportListModalContent.css';

function EditImportListModalContent(props) {

  const monitorOptions = [
    { key: 'none', value: 'None' },
    { key: 'specificEpisode', value: 'Specific Episode' },
    { key: 'entireSite', value: 'All Site Episodes' }
  ];

  const {
    advancedSettings,
    isFetching,
    error,
    isSaving,
    isTesting,
    saveError,
    item,
    onInputChange,
    onFieldChange,
    onModalClose,
    onSavePress,
    onTestPress,
    onDeleteImportListPress,
    ...otherProps
  } = props;

  const {
    id,
    name,
    enableAutomaticAdd,
    minRefreshInterval,
    shouldMonitor,
    siteMonitorType,
    rootFolderPath,
    qualityProfileId,
    tags,
    fields
  } = item;

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {id ? 'Edit List' : 'Add List'}
      </ModalHeader>

      <ModalBody>
        {
          isFetching ?
            <LoadingIndicator /> :
            null
        }

        {
          !isFetching && !!error ?
            <div>Unable to add a new list, please try again.</div> :
            null
        }

        {
          !isFetching && !error ?
            <Form {...otherProps}>

              <Alert
                kind={kinds.INFO}
                className={styles.message}
              >
                {`List will refresh every ${formatShortTimeSpan(minRefreshInterval.value)}`}
              </Alert>

              <FormGroup>
                <FormLabel>Name</FormLabel>

                <FormInputGroup
                  type={inputTypes.TEXT}
                  name="name"
                  {...name}
                  onChange={onInputChange}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>Enable Automatic Add</FormLabel>

                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="enableAutomaticAdd"
                  helpText={'Add site to Whisparr when syncs are performed via the UI or by Whisparr'}
                  {...enableAutomaticAdd}
                  onChange={onInputChange}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>
                  Monitor
                </FormLabel>

                <FormInputGroup
                  type={inputTypes.SELECT}
                  name="shouldMonitor"
                  values={monitorOptions}
                  onChange={onInputChange}
                  {...shouldMonitor}
                />
              </FormGroup>

              {
                shouldMonitor.value === 'entireSite' ?
                  <FormGroup>
                    <FormLabel>
                      Site Monitoring Options

                      <Popover
                        anchor={
                          <Icon
                            className={styles.labelIcon}
                            name={icons.INFO}
                          />
                        }
                        title="Monitoring Options"
                        body={<SeriesMonitoringOptionsPopoverContent />}
                        position={tooltipPositions.RIGHT}
                      />
                    </FormLabel>

                    <FormInputGroup
                      type={inputTypes.MONITOR_EPISODES_SELECT}
                      name="siteMonitorType"
                      onChange={onInputChange}
                      {...siteMonitorType}
                    />
                  </FormGroup> :
                  null
              }

              <FormGroup>
                <FormLabel>Root Folder</FormLabel>

                <FormInputGroup
                  type={inputTypes.ROOT_FOLDER_SELECT}
                  name="rootFolderPath"
                  helpText={'Root Folder list items will be added to'}
                  {...rootFolderPath}
                  includeMissingValue={true}
                  onChange={onInputChange}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>Quality Profile</FormLabel>

                <FormInputGroup
                  type={inputTypes.QUALITY_PROFILE_SELECT}
                  name="qualityProfileId"
                  helpText={'Quality Profile list items will be added with'}
                  {...qualityProfileId}
                  onChange={onInputChange}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>Whisparr Tags</FormLabel>

                <FormInputGroup
                  type={inputTypes.TAG}
                  name="tags"
                  helpText="Tags list items will be added with"
                  {...tags}
                  onChange={onInputChange}
                />
              </FormGroup>

              {
                !!fields && !!fields.length &&
                  <div>
                    {
                      fields.map((field) => {
                        return (
                          <ProviderFieldFormGroup
                            key={field.name}
                            advancedSettings={advancedSettings}
                            provider="importList"
                            providerData={item}
                            section="settings.importLists"
                            {...field}
                            onChange={onFieldChange}
                          />
                        );
                      })
                    }
                  </div>
              }

            </Form> :
            null
        }
      </ModalBody>
      <ModalFooter>
        {
          id &&
            <Button
              className={styles.deleteButton}
              kind={kinds.DANGER}
              onPress={onDeleteImportListPress}
            >
              Delete
            </Button>
        }

        <SpinnerErrorButton
          isSpinning={isTesting}
          error={saveError}
          onPress={onTestPress}
        >
          Test
        </SpinnerErrorButton>

        <Button
          onPress={onModalClose}
        >
          Cancel
        </Button>

        <SpinnerErrorButton
          isSpinning={isSaving}
          error={saveError}
          onPress={onSavePress}
        >
          Save
        </SpinnerErrorButton>
      </ModalFooter>
    </ModalContent>
  );
}

EditImportListModalContent.propTypes = {
  advancedSettings: PropTypes.bool.isRequired,
  isFetching: PropTypes.bool.isRequired,
  error: PropTypes.object,
  isSaving: PropTypes.bool.isRequired,
  isTesting: PropTypes.bool.isRequired,
  saveError: PropTypes.object,
  item: PropTypes.object.isRequired,
  onInputChange: PropTypes.func.isRequired,
  onFieldChange: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired,
  onSavePress: PropTypes.func.isRequired,
  onTestPress: PropTypes.func.isRequired,
  onDeleteImportListPress: PropTypes.func
};

export default EditImportListModalContent;
