import React, { useCallback, useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import Alert from 'Components/Alert';
import FieldSet from 'Components/FieldSet';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import useShowAdvancedSettings from 'Helpers/Hooks/useShowAdvancedSettings';
import { inputTypes, kinds } from 'Helpers/Props';
import { clearPendingChanges } from 'Store/Actions/baseActions';
import {
  fetchIndexerOptions,
  saveIndexerOptions,
  setIndexerOptionsValue,
} from 'Store/Actions/settingsActions';
import createSettingsSectionSelector from 'Store/Selectors/createSettingsSectionSelector';
import { InputChanged } from 'typings/inputs';
import {
  OnChildStateChange,
  SetChildSave,
} from 'typings/Settings/SettingsState';
import translate from 'Utilities/String/translate';

const SECTION = 'indexerOptions';

interface IndexerOptionsProps {
  setChildSave: SetChildSave;
  onChildStateChange: OnChildStateChange;
}

function IndexerOptions({
  setChildSave,
  onChildStateChange,
}: IndexerOptionsProps) {
  const dispatch = useDispatch();
  const {
    isFetching,
    isPopulated,
    isSaving,
    error,
    settings,
    hasSettings,
    hasPendingChanges,
  } = useSelector(createSettingsSectionSelector(SECTION));

  const showAdvancedSettings = useShowAdvancedSettings();
  const searchDateFormatOptions = [
    {
      key: 'yymmdd',
      value: translate('yymmdd'),
    },
    {
      key: 'ddmmyyyy',
      value: translate('ddmmyyyy'),
    },
    {
      key: 'both',
      value: translate('Both'),
    },
  ];
  const searchStudioFormatOptions = [
    {
      key: 'original',
      value: translate('Original'),
    },
    {
      key: 'clean',
      value: translate('Clean'),
    },
    {
      key: 'both',
      value: translate('Both'),
    },
  ];

  const handleInputChange = useCallback(
    (change: InputChanged) => {
      // @ts-expect-error - actions aren't typed
      dispatch(setIndexerOptionsValue(change));
    },
    [dispatch]
  );

  const handleWhitelistedSubtitleChange = useCallback(
    ({ name, value }: InputChanged<string[] | null>) => {
      // @ts-expect-error - actions aren't typed
      dispatch(setIndexerOptionsValue({ name, value: value?.join(',') }));
    },
    [dispatch]
  );

  useEffect(() => {
    dispatch(fetchIndexerOptions());
    setChildSave(() => dispatch(saveIndexerOptions()));
  }, [dispatch, setChildSave]);

  useEffect(() => {
    onChildStateChange({
      isSaving,
      hasPendingChanges,
    });
  }, [hasPendingChanges, isSaving, onChildStateChange]);

  useEffect(() => {
    return () => {
      dispatch(clearPendingChanges({ section: `settings.${SECTION}` }));
    };
  }, [dispatch]);

  return (
    <FieldSet legend={translate('Options')}>
      {isFetching ? <LoadingIndicator /> : null}

      {!isFetching && error ? (
        <Alert kind={kinds.DANGER}>
          {translate('IndexerOptionsLoadError')}
        </Alert>
      ) : null}

      {hasSettings && isPopulated && !error ? (
        <Form>
          <FormGroup>
            <FormLabel>{translate('MinimumAge')}</FormLabel>

            <FormInputGroup
              type={inputTypes.NUMBER}
              name="minimumAge"
              min={0}
              unit="minutes"
              helpText={translate('MinimumAgeHelpText')}
              onChange={handleInputChange}
              {...settings.minimumAge}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>{translate('Retention')}</FormLabel>

            <FormInputGroup
              type={inputTypes.NUMBER}
              name="retention"
              min={0}
              unit="days"
              helpText={translate('RetentionHelpText')}
              onChange={handleInputChange}
              {...settings.retention}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>{translate('MaximumSize')}</FormLabel>

            <FormInputGroup
              type={inputTypes.NUMBER}
              name="maximumSize"
              min={0}
              unit="MB"
              helpText={translate('MaximumSizeHelpText')}
              onChange={handleInputChange}
              {...settings.maximumSize}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>{translate('PreferIndexerFlags')}</FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="preferIndexerFlags"
              helpText={translate('PreferIndexerFlagsHelpText')}
              helpLink="https://wiki.servarr.com/whisparr/settings#indexer-flags"
              onChange={handleInputChange}
              {...settings.preferIndexerFlags}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>{translate('AvailabilityDelay')}</FormLabel>

            <FormInputGroup
              type={inputTypes.NUMBER}
              name="availabilityDelay"
              unit="days"
              helpText={translate('AvailabilityDelayHelpText')}
              onChange={handleInputChange}
              {...settings.availabilityDelay}
            />
          </FormGroup>

          <FormGroup advancedSettings={showAdvancedSettings} isAdvanced={true}>
            <FormLabel>{translate('RssSyncInterval')}</FormLabel>

            <FormInputGroup
              type={inputTypes.NUMBER}
              name="rssSyncInterval"
              min={0}
              max={120}
              unit="minutes"
              helpText={translate('RssSyncIntervalHelpText')}
              helpTextWarning={translate('RssSyncIntervalHelpTextWarning')}
              helpLink="https://wiki.servarr.com/whisparr/faq#how-does-whisparr-work"
              onChange={handleInputChange}
              {...settings.rssSyncInterval}
            />
          </FormGroup>

          <FormGroup advancedSettings={showAdvancedSettings} isAdvanced={true}>
            <FormLabel>{translate('WhitelistedSubtitleTags')}</FormLabel>

            <FormInputGroup
              type={inputTypes.TEXT_TAG}
              name="whitelistedHardcodedSubs"
              helpText={translate('WhitelistedHardcodedSubsHelpText')}
              onChange={handleWhitelistedSubtitleChange}
              {...settings.whitelistedHardcodedSubs}
            />
          </FormGroup>

          <FormGroup advancedSettings={showAdvancedSettings} isAdvanced={true}>
            <FormLabel>{translate('AllowHardcodedSubs')}</FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="allowHardcodedSubs"
              helpText={translate('AllowHardcodedSubsHelpText')}
              onChange={handleInputChange}
              {...settings.allowHardcodedSubs}
            />
          </FormGroup>

          <FormGroup advancedSettings={showAdvancedSettings} isAdvanced={true}>
            <FormLabel>{translate('SearchTitleOnly')}</FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="searchTitleOnly"
              helpText={translate('SearchTitleOnlyHelpText')}
              onChange={handleInputChange}
              {...settings.searchTitleOnly}
            />
          </FormGroup>

          <FormGroup advancedSettings={showAdvancedSettings} isAdvanced={true}>
            <FormLabel>{translate('SearchTitleDate')}</FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="searchTitleDate"
              helpText={translate('SearchTitleDateHelpText')}
              onChange={handleInputChange}
              {...settings.searchTitleDate}
            />
          </FormGroup>

          <FormGroup advancedSettings={showAdvancedSettings} isAdvanced={true}>
            <FormLabel>{translate('SearchStudioCode')}</FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="searchStudioCode"
              helpText={translate('SearchStudioCodeHelpText')}
              onChange={handleInputChange}
              {...settings.searchStudioCode}
            />
          </FormGroup>

          <FormGroup advancedSettings={showAdvancedSettings} isAdvanced={true}>
            <FormLabel>{translate('SearchStudioDate')}</FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="searchStudioDate"
              helpText={translate('SearchStudioDateHelpText')}
              onChange={handleInputChange}
              {...settings.searchStudioDate}
            />
          </FormGroup>

          <FormGroup advancedSettings={showAdvancedSettings} isAdvanced={true}>
            <FormLabel>{translate('SearchStudioTitle')}</FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="searchStudioTitle"
              helpText={translate('SearchStudioTitleHelpText')}
              onChange={handleInputChange}
              {...settings.searchStudioTitle}
            />
          </FormGroup>

          <FormGroup advancedSettings={showAdvancedSettings} isAdvanced={true}>
            <FormLabel>{translate('SearchDateFormat')}</FormLabel>

            <FormInputGroup
              type={inputTypes.SELECT}
              name="searchDateFormat"
              values={searchDateFormatOptions}
              helpText={translate('SearchDateFormatHelpText')}
              onChange={handleInputChange}
              {...settings.searchDateFormat}
            />
          </FormGroup>

          <FormGroup advancedSettings={showAdvancedSettings} isAdvanced={true}>
            <FormLabel>{translate('SearchStudioFormat')}</FormLabel>

            <FormInputGroup
              type={inputTypes.SELECT}
              name="searchStudioFormat"
              values={searchStudioFormatOptions}
              helpText={translate('SearchStudioFormatHelpText')}
              onChange={handleInputChange}
              {...settings.searchStudioFormat}
            />
          </FormGroup>
        </Form>
      ) : null}
    </FieldSet>
  );
}

export default IndexerOptions;
