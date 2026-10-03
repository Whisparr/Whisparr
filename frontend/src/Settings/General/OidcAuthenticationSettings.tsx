import React from 'react';
import FormInput from 'Components/Form/FormInput';
import FormInputHelpText from 'Components/Form/FormInputHelpText';
import FormLabel from 'Components/Form/FormLabel';
import FormRow from 'Components/Form/FormRow';
import { inputTypes } from 'Helpers/Props';
import { InputChanged } from 'typings/inputs';
import { PendingSection } from 'typings/pending';
import General from 'typings/Settings/General';
import translate from 'Utilities/String/translate';

interface OidcAuthenticationSettingsProps {
  oidcAuthority: PendingSection<General>['oidcAuthority'];
  oidcClientId: PendingSection<General>['oidcClientId'];
  oidcClientSecret: PendingSection<General>['oidcClientSecret'];
  oidcUserIdentifier: PendingSection<General>['oidcUserIdentifier'];
  oidcScopes: PendingSection<General>['oidcScopes'];
  showValidationWarnings?: boolean;
  onInputChange: (change: InputChanged) => void;
}

function OidcAuthenticationSettings({
  oidcAuthority,
  oidcClientId,
  oidcClientSecret,
  oidcUserIdentifier,
  oidcScopes,
  showValidationWarnings = false,
  onInputChange,
}: OidcAuthenticationSettingsProps) {
  return (
    <>
      <FormRow>
        <FormLabel>{translate('OidcAuthority')}</FormLabel>
        <FormInputHelpText text={translate('OidcAuthorityHelpText')} />
        <FormInputHelpText
          text={
            showValidationWarnings && !oidcAuthority?.value
              ? translate('AuthenticationRequiredOidcAuthorityHelpTextWarning')
              : undefined
          }
          isWarning={true}
        />
        <FormInput
          type={inputTypes.TEXT}
          name="oidcAuthority"
          onChange={onInputChange}
          {...oidcAuthority}
        />
      </FormRow>

      <FormRow>
        <FormLabel>{translate('OidcClientId')}</FormLabel>
        <FormInputHelpText text={translate('OidcClientIdHelpText')} />
        <FormInputHelpText
          text={
            showValidationWarnings && !oidcClientId?.value
              ? translate('AuthenticationRequiredOidcClientIdHelpTextWarning')
              : undefined
          }
          isWarning={true}
        />
        <FormInput
          type={inputTypes.TEXT}
          name="oidcClientId"
          onChange={onInputChange}
          {...oidcClientId}
        />
      </FormRow>

      <FormRow>
        <FormLabel>{translate('OidcClientSecret')}</FormLabel>
        <FormInputHelpText text={translate('OidcClientSecretHelpText')} />
        <FormInputHelpText
          text={
            showValidationWarnings && !oidcClientSecret?.value
              ? translate(
                  'AuthenticationRequiredOidcClientSecretHelpTextWarning'
                )
              : undefined
          }
          isWarning={true}
        />
        <FormInput
          type={inputTypes.PASSWORD}
          name="oidcClientSecret"
          onChange={onInputChange}
          {...oidcClientSecret}
        />
      </FormRow>

      <FormRow>
        <FormLabel>{translate('OidcUserIdentifier')}</FormLabel>
        <FormInputHelpText text={translate('OidcUserIdentifierHelpText')} />
        <FormInputHelpText
          text={
            showValidationWarnings && !oidcUserIdentifier?.value
              ? translate(
                  'AuthenticationRequiredOidcUserIdentifierHelpTextWarning'
                )
              : undefined
          }
          isWarning={true}
        />
        <FormInput
          type={inputTypes.TEXT}
          name="oidcUserIdentifier"
          onChange={onInputChange}
          {...oidcUserIdentifier}
        />
      </FormRow>

      <FormRow>
        <FormLabel>{translate('OidcScopes')}</FormLabel>
        <FormInputHelpText text={translate('OidcScopesHelpText')} />
        <FormInput
          type={inputTypes.TEXT}
          name="oidcScopes"
          onChange={onInputChange}
          {...oidcScopes}
        />
      </FormRow>
    </>
  );
}

export default OidcAuthenticationSettings;
