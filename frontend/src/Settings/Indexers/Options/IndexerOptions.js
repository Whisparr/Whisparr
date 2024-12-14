import PropTypes from 'prop-types';
import React from 'react';
import Alert from 'Components/Alert';
import FieldSet from 'Components/FieldSet';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import { inputTypes, kinds } from 'Helpers/Props';
import translate from 'Utilities/String/translate';

function IndexerOptions(props) {
  const {
    advancedSettings,
    isFetching,
    error,
    settings,
    hasSettings,
    onInputChange,
    onWhitelistedSubtitleChange
  } = props;

  const searchDateFormatOptions = [
    {
      key: 'yymmdd',
      get value() {
        return translate('yymmdd');
      }
    },
    {
      key: 'ddmmyyyy',
      get value() {
        return translate('ddmmyyyy');
      }
    },
    {
      key: 'both',
      get value() {
        return translate('Both');
      }
    }
  ];

  const searchStudioFormatOptions = [
    {
      key: 'original',
      get value() {
        return translate('Original');
      }
    },
    {
      key: 'clean',
      get value() {
        return translate('Clean');
      }
    },
    {
      key: 'both',
      get value() {
        return translate('Both');
      }
    }
  ];

  return (
    <FieldSet legend={translate('Options')}>
      {
        isFetching &&
          <LoadingIndicator />
      }

      {
        !isFetching && error &&
          <Alert kind={kinds.DANGER}>
            {translate('IndexerOptionsLoadError')}
          </Alert>
      }

      {
        hasSettings && !isFetching && !error &&
          <Form>
            <FormGroup>
              <FormLabel>{translate('MinimumAge')}</FormLabel>

              <FormInputGroup
                type={inputTypes.NUMBER}
                name="minimumAge"
                min={0}
                unit="minutes"
                helpText={translate('MinimumAgeHelpText')}
                onChange={onInputChange}
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
                onChange={onInputChange}
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
                onChange={onInputChange}
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
                onChange={onInputChange}
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
                onChange={onInputChange}
                {...settings.availabilityDelay}
              />
            </FormGroup>

            <FormGroup
              advancedSettings={advancedSettings}
              isAdvanced={true}
            >
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
                onChange={onInputChange}
                {...settings.rssSyncInterval}
              />
            </FormGroup>

            <FormGroup
              advancedSettings={advancedSettings}
              isAdvanced={true}
            >
              <FormLabel>{translate('WhitelistedSubtitleTags')}</FormLabel>

              <FormInputGroup
                type={inputTypes.TEXT_TAG}
                name="whitelistedHardcodedSubs"
                helpText={translate('WhitelistedHardcodedSubsHelpText')}
                onChange={onWhitelistedSubtitleChange}
                {...settings.whitelistedHardcodedSubs}
              />
            </FormGroup>

            <FormGroup
              advancedSettings={advancedSettings}
              isAdvanced={true}
            >
              <FormLabel>{translate('AllowHardcodedSubs')}</FormLabel>

              <FormInputGroup
                type={inputTypes.CHECK}
                name="allowHardcodedSubs"
                helpText={translate('AllowHardcodedSubsHelpText')}
                onChange={onInputChange}
                {...settings.allowHardcodedSubs}
              />
            </FormGroup>

            <FormGroup
              advancedSettings={advancedSettings}
              isAdvanced={true}
            >
              <FormLabel>{translate('SearchTitleOnly')}</FormLabel>

              <FormInputGroup
                type={inputTypes.CHECK}
                name="searchTitleOnly"
                helpText={translate('SearchTitleOnlyHelpText')}
                onChange={onInputChange}
                {...settings.searchTitleOnly}
              />
            </FormGroup>

            <FormGroup
              advancedSettings={advancedSettings}
              isAdvanced={true}
            >
              <FormLabel>{translate('SearchTitleDate')}</FormLabel>

              <FormInputGroup
                type={inputTypes.CHECK}
                name="searchTitleDate"
                helpText={translate('SearchTitleDateHelpText')}
                onChange={onInputChange}
                {...settings.searchTitleDate}
              />
            </FormGroup>

            <FormGroup
              advancedSettings={advancedSettings}
              isAdvanced={true}
            >
              <FormLabel>{translate('SearchStudioDate')}</FormLabel>

              <FormInputGroup
                type={inputTypes.CHECK}
                name="searchStudioDate"
                helpText={translate('SearchStudioDateHelpText')}
                onChange={onInputChange}
                {...settings.searchStudioDate}
              />
            </FormGroup>

            <FormGroup
              advancedSettings={advancedSettings}
              isAdvanced={true}
            >
              <FormLabel>{translate('SearchStudioTitle')}</FormLabel>

              <FormInputGroup
                type={inputTypes.CHECK}
                name="searchStudioTitle"
                helpText={translate('SearchStudioTitleHelpText')}
                onChange={onInputChange}
                {...settings.searchStudioTitle}
              />
            </FormGroup>

            <FormGroup
              advancedSettings={advancedSettings}
              isAdvanced={true}
            >
              <FormLabel>{translate('SearchDateFormat')}</FormLabel>

              <FormInputGroup
                type={inputTypes.SELECT}
                name="searchDateFormat"
                values={searchDateFormatOptions}
                helpText={translate('SearchDateFormatHelpText')}
                onChange={onInputChange}
                {...settings.searchDateFormat}
              />
            </FormGroup>

            <FormGroup
              advancedSettings={advancedSettings}
              isAdvanced={true}
            >
              <FormLabel>{translate('SearchStudioFormat')}</FormLabel>

              <FormInputGroup
                type={inputTypes.SELECT}
                name="searchStudioFormat"
                values={searchStudioFormatOptions}
                helpText={translate('SearchStudioFormatHelpText')}
                onChange={onInputChange}
                {...settings.searchStudioFormat}
              />
            </FormGroup>
          </Form>
      }
    </FieldSet>
  );
}

IndexerOptions.propTypes = {
  advancedSettings: PropTypes.bool.isRequired,
  isFetching: PropTypes.bool.isRequired,
  error: PropTypes.object,
  settings: PropTypes.object.isRequired,
  hasSettings: PropTypes.bool.isRequired,
  onInputChange: PropTypes.func.isRequired,
  onWhitelistedSubtitleChange: PropTypes.func.isRequired
};

export default IndexerOptions;
