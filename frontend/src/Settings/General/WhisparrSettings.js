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
    whisparrAlwaysExcludePerformers,
    whisparrAlwaysExcludePerformersTag,
    whisparrAlwaysExcludeStudios,
    whisparrAlwaysExcludeStudiosTag,
    whisparrAlwaysExcludeStudiosAfterTag,
    whisparrAlwaysExcludeTags,
    whisparrAlwaysExcludeTagsTag,
    whisparrAutoMatchOnDate,
    whisparrCacheExclusionAPI,
    whisparrCacheMovieAPI,
    whisparrCachePerformerAPI,
    whisparrCacheStudioAPI,
    whisparrValidateRuntime,
    whisparrValidateRuntimeLimit
  } = settings;

  return (
    <FieldSet legend={translate('Whisparr')}>
      <FormGroup
        advancedSettings={advancedSettings}
        isAdvanced={true}
      >

        <FormLabel>{translate('WhisparrCacheExclusionAPI')}</FormLabel>

        <FormInputGroup
          type={inputTypes.CHECK}
          name="whisparrCacheExclusionAPI"
          helpText={translate('WhisparrCacheExclusionAPIHelpText')}
          onChange={onInputChange}
          {...whisparrCacheExclusionAPI}
        />
      </FormGroup>

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

        <FormLabel>{translate('WhisparrCacheStudioAPI')}</FormLabel>

        <FormInputGroup
          type={inputTypes.CHECK}
          name="whisparrCacheStudioAPI"
          helpText={translate('WhisparrCacheStudioAPIHelpText')}
          onChange={onInputChange}
          {...whisparrCacheStudioAPI}
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
          helpText={translate('WhisparrValidateRuntimeLimitHelpText')}
          onChange={onInputChange}
          {...whisparrValidateRuntimeLimit}
        />
      </FormGroup>

      <FormGroup>

        <FormLabel>{translate('WhisparrAlwaysExcludePerformers')}</FormLabel>

        <FormInputGroup
          type={inputTypes.CHECK}
          name="whisparrAlwaysExcludePerformers"
          helpText={translate('WhisparrAlwaysExcludePerformersHelpText')}
          onChange={onInputChange}
          {...whisparrAlwaysExcludePerformers}
        />
      </FormGroup>

      <FormGroup>

        <FormLabel>{translate('WhisparrAlwaysExcludePerformersTag')}</FormLabel>

        <FormInputGroup
          type={inputTypes.TEXT}
          name="whisparrAlwaysExcludePerformersTag"
          helpText={translate('WhisparrAlwaysExcludePerformersTagHelpText')}
          onChange={onInputChange}
          {...whisparrAlwaysExcludePerformersTag}
        />
      </FormGroup>

      <FormGroup>

        <FormLabel>{translate('WhisparrAlwaysExcludeStudios')}</FormLabel>

        <FormInputGroup
          type={inputTypes.CHECK}
          name="whisparrAlwaysExcludeStudios"
          helpText={translate('WhisparrAlwaysExcludeStudiosHelpText')}
          onChange={onInputChange}
          {...whisparrAlwaysExcludeStudios}
        />
      </FormGroup>

      <FormGroup>

        <FormLabel>{translate('WhisparrAlwaysExcludeStudiosTag')}</FormLabel>

        <FormInputGroup
          type={inputTypes.TEXT}
          name="whisparrAlwaysExcludeStudiosTag"
          helpText={translate('WhisparrAlwaysExcludeStudiosTagHelpText')}
          onChange={onInputChange}
          {...whisparrAlwaysExcludeStudiosTag}
        />
      </FormGroup>

      <FormGroup>

        <FormLabel>{translate('WhisparrAlwaysExcludeStudiosAfterTag')}</FormLabel>

        <FormInputGroup
          type={inputTypes.TEXT}
          name="whisparrAlwaysExcludeStudiosAfterTag"
          helpText={translate('WhisparrAlwaysExcludeStudiosAfterTagHelpText')}
          onChange={onInputChange}
          {...whisparrAlwaysExcludeStudiosAfterTag}
        />
      </FormGroup>

      <FormGroup>

        <FormLabel>{translate('WhisparrAlwaysExcludeTags')}</FormLabel>

        <FormInputGroup
          type={inputTypes.CHECK}
          name="whisparrAlwaysExcludeTags"
          helpText={translate('WhisparrAlwaysExcludeTagsHelpText')}
          onChange={onInputChange}
          {...whisparrAlwaysExcludeTags}
        />
      </FormGroup>

      <FormGroup>

        <FormLabel>{translate('WhisparrAlwaysExcludeTagsTag')}</FormLabel>

        <FormInputGroup
          type={inputTypes.TEXT}
          name="whisparrAlwaysExcludeTagsTag"
          helpText={translate('WhisparrAlwaysExcludeTagsTagHelpText')}
          onChange={onInputChange}
          {...whisparrAlwaysExcludeTagsTag}
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
