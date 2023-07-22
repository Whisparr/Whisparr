import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Alert from 'Components/Alert';
import FieldSet from 'Components/FieldSet';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputButton from 'Components/Form/FormInputButton';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import { inputTypes, kinds, sizes } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import NamingModal from './NamingModal';
import styles from './Naming.css';

class Naming extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isNamingModalOpen: false,
      namingModalOptions: null
    };
  }

  //
  // Listeners

  onStandardNamingModalOpenClick = () => {
    this.setState({
      isNamingModalOpen: true,
      namingModalOptions: {
        name: 'standardEpisodeFormat',
        season: true,
        episode: true,
        additional: true
      }
    });
  };

  onSeriesFolderNamingModalOpenClick = () => {
    this.setState({
      isNamingModalOpen: true,
      namingModalOptions: {
        name: 'seriesFolderFormat'
      }
    });
  };

  onNamingModalClose = () => {
    this.setState({ isNamingModalOpen: false });
  };

  //
  // Render

  render() {
    const {
      advancedSettings,
      isFetching,
      error,
      settings,
      hasSettings,
      examples,
      examplesPopulated,
      onInputChange
    } = this.props;

    const {
      isNamingModalOpen,
      namingModalOptions
    } = this.state;

    const renameEpisodes = hasSettings && settings.renameEpisodes.value;
    const replaceIllegalCharacters = hasSettings && settings.replaceIllegalCharacters.value;

    const multiEpisodeStyleOptions = [
      { key: 0, value: translate('Extend'), hint: 'S01E01-02-03' },
      { key: 1, value: translate('Duplicate'), hint: 'S01E01.S01E02' },
      { key: 2, value: translate('Repeat'), hint: 'S01E01E02E03' },
      { key: 3, value: translate('Scene'), hint: 'S01E01-E02-E03' },
      { key: 4, value: translate('Range'), hint: 'S01E01-03' },
      { key: 5, value: translate('PrefixedRange'), hint: 'S01E01-E03' }
    ];

    const colonReplacementOptions = [
      { key: 0, value: translate('Delete') },
      { key: 1, value: translate('ReplaceWithDash') },
      { key: 2, value: translate('ReplaceWithSpaceDash') },
      { key: 3, value: translate('ReplaceWithSpaceDashSpace') },
      { key: 4, value: translate('SmartReplace'), hint: translate('SmartReplaceHint') }
    ];

    const standardEpisodeFormatHelpTexts = [];
    const standardEpisodeFormatErrors = [];
    const seriesFolderFormatHelpTexts = [];
    const seriesFolderFormatErrors = [];

    if (examplesPopulated) {
      if (examples.singleEpisodeExample) {
        standardEpisodeFormatHelpTexts.push(`${translate('SingleEpisode')}: ${examples.singleEpisodeExample}`);
      } else {
        standardEpisodeFormatErrors.push({ message: translate('SingleEpisodeInvalidFormat') });
      }

      if (examples.seriesFolderExample) {
        seriesFolderFormatHelpTexts.push(`${translate('Example')}: ${examples.seriesFolderExample}`);
      } else {
        seriesFolderFormatErrors.push({ message: translate('InvalidFormat') });
      }
    }

    return (
      <FieldSet legend={translate('EpisodeNaming')}>
        {
          isFetching &&
            <LoadingIndicator />
        }

        {
          !isFetching && error &&
            <Alert kind={kinds.DANGER}>
              {translate('NamingSettingsLoadError')}
            </Alert>
        }

        {
          hasSettings && !isFetching && !error &&
            <Form>
              <FormGroup size={sizes.MEDIUM}>
                <FormLabel>{translate('RenameEpisodes')}</FormLabel>

                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="renameEpisodes"
                  helpText={translate('RenameEpisodesHelpText')}
                  onChange={onInputChange}
                  {...settings.renameEpisodes}
                />
              </FormGroup>

              <FormGroup size={sizes.MEDIUM}>
                <FormLabel>{translate('ReplaceIllegalCharacters')}</FormLabel>

                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="replaceIllegalCharacters"
                  helpText={translate('ReplaceIllegalCharactersHelpText')}
                  onChange={onInputChange}
                  {...settings.replaceIllegalCharacters}
                />
              </FormGroup>

              {
                replaceIllegalCharacters ?
                  <FormGroup>
                    <FormLabel>{translate('ColonReplacement')}</FormLabel>

                    <FormInputGroup
                      type={inputTypes.SELECT}
                      name="colonReplacementFormat"
                      values={colonReplacementOptions}
                      helpText={translate('ColonReplacementFormatHelpText')}
                      onChange={onInputChange}
                      {...settings.colonReplacementFormat}
                    />
                  </FormGroup> :
                  null
              }

              {
                renameEpisodes &&
                  <div>
                    <FormGroup size={sizes.LARGE}>
                      <FormLabel>{translate('StandardEpisodeFormat')}</FormLabel>

                      <FormInputGroup
                        inputClassName={styles.namingInput}
                        type={inputTypes.TEXT}
                        name="standardEpisodeFormat"
                        buttons={<FormInputButton onPress={this.onStandardNamingModalOpenClick}>?</FormInputButton>}
                        onChange={onInputChange}
                        {...settings.standardEpisodeFormat}
                        helpTexts={standardEpisodeFormatHelpTexts}
                        errors={[...standardEpisodeFormatErrors, ...settings.standardEpisodeFormat.errors]}
                      />
                    </FormGroup>
                  </div>
              }

              <FormGroup
                advancedSettings={advancedSettings}
                isAdvanced={true}
              >
                <FormLabel>{translate('SiteFolderFormat')}</FormLabel>

                <FormInputGroup
                  inputClassName={styles.namingInput}
                  type={inputTypes.TEXT}
                  name="seriesFolderFormat"
                  buttons={<FormInputButton onPress={this.onSeriesFolderNamingModalOpenClick}>?</FormInputButton>}
                  onChange={onInputChange}
                  {...settings.seriesFolderFormat}
                  helpTexts={[translate('SiteFolderFormatHelpText'), ...seriesFolderFormatHelpTexts]}
                  errors={[...seriesFolderFormatErrors, ...settings.seriesFolderFormat.errors]}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>{translate('MultiEpisodeStyle')}</FormLabel>

                <FormInputGroup
                  type={inputTypes.SELECT}
                  name="multiEpisodeStyle"
                  values={multiEpisodeStyleOptions}
                  onChange={onInputChange}
                  {...settings.multiEpisodeStyle}
                />
              </FormGroup>

              {
                namingModalOptions &&
                  <NamingModal
                    isOpen={isNamingModalOpen}
                    advancedSettings={advancedSettings}
                    {...namingModalOptions}
                    value={settings[namingModalOptions.name].value}
                    onInputChange={onInputChange}
                    onModalClose={this.onNamingModalClose}
                  />
              }
            </Form>
        }
      </FieldSet>
    );
  }

}

Naming.propTypes = {
  advancedSettings: PropTypes.bool.isRequired,
  isFetching: PropTypes.bool.isRequired,
  error: PropTypes.object,
  settings: PropTypes.object.isRequired,
  hasSettings: PropTypes.bool.isRequired,
  examples: PropTypes.object.isRequired,
  examplesPopulated: PropTypes.bool.isRequired,
  onInputChange: PropTypes.func.isRequired
};

export default Naming;
