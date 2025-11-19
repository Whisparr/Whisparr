import PropTypes from 'prop-types';
import React from 'react';
import FieldSet from 'Components/FieldSet';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import { inputTypes } from 'Helpers/Props';
import translate from 'Utilities/String/translate';

function WhisparrSettings(props) {
  const {
    advancedSettings,
    settings,
    onInputChange
  } = props;

  const {
    whisparrAutoMatchOnDate,
    whisparrCacheMovieAPI,
    whisparrCachePerformerAPI,
    whisparrValidateRuntime,
    whisparrValidateRuntimeLimit
  } = settings;

  if (!advancedSettings) {
    return null;
  }

  return (
    <FieldSet legend={translate('Whisparr')}>
      <FormGroup
        advancedSettings={advancedSettings}
        isAdvanced={true}
      >

        <FormLabel>{translate('WhisparrCacheMovieAPI')}</FormLabel>

        <FormInputGroup
          type={inputTypes.CHECK}
          name="whisparrCacheMovieAPI"
          helpText={translate('WhisparrCacheMovieAPIHelpText')}
          onChange={onInputChange}
          {...whisparrCacheMovieAPI}
        />
      </FormGroup>

      <FormGroup
        advancedSettings={advancedSettings}
        isAdvanced={true}
      >

        <FormLabel>{translate('WhisparrCachePerformerAPI')}</FormLabel>

        <FormInputGroup
          type={inputTypes.CHECK}
          name="whisparrCachePerformerAPI"
          helpText={translate('WhisparrCachePerformerAPIHelpText')}
          onChange={onInputChange}
          {...whisparrCachePerformerAPI}
        />
      </FormGroup>

      <FormGroup
        advancedSettings={advancedSettings}
        isAdvanced={true}
      >
        <FormLabel>{translate('WhisparrAutoMatchOnDate')}</FormLabel>

        <FormInputGroup
          type={inputTypes.CHECK}
          name="whisparrAutoMatchOnDate"
          helpText={translate('WhisparrAutoMatchOnDateHelpText')}
          onChange={onInputChange}
          {...whisparrAutoMatchOnDate}
        />
      </FormGroup>
      <FormGroup
        advancedSettings={advancedSettings}
        isAdvanced={true}
      >
        <FormLabel>{translate('WhisparrValidateRuntime')}</FormLabel>

        <FormInputGroup
          type={inputTypes.CHECK}
          name="whisparrValidateRuntime"
          helpText={translate('WhisparrValidateRuntimeHelpText')}
          onChange={onInputChange}
          {...whisparrValidateRuntime}
        />
      </FormGroup>
      <FormGroup
        advancedSettings={advancedSettings}
        isAdvanced={true}
      >
        <FormLabel>{translate('WhisparrValidateRuntimeLimit')}</FormLabel>

        <FormInputGroup
          type={inputTypes.NUMBER}
          name="whisparrValidateRuntimeLimit"
          helpText={translate('WhisparrValidateRuntimeLimittHelpText')}
          onChange={onInputChange}
          {...whisparrValidateRuntimeLimit}
        />
      </FormGroup>
    </FieldSet>
  );
}

WhisparrSettings.propTypes = {
  advancedSettings: PropTypes.bool.isRequired,
  settings: PropTypes.object.isRequired,
  onInputChange: PropTypes.func.isRequired
};

export default WhisparrSettings;
