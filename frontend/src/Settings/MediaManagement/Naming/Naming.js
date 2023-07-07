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
      { key: 0, value: 'Extend', hint: 'S01E01-02-03' },
      { key: 1, value: 'Duplicate', hint: 'S01E01.S01E02' },
      { key: 2, value: 'Repeat', hint: 'S01E01E02E03' },
      { key: 3, value: 'Scene', hint: 'S01E01-E02-E03' },
      { key: 4, value: 'Range', hint: 'S01E01-03' },
      { key: 5, value: 'Prefixed Range', hint: 'S01E01-E03' }
    ];

    const colonReplacementOptions = [
      { key: 0, value: 'Delete' },
      { key: 1, value: 'Replace with Dash' },
      { key: 2, value: 'Replace with Space Dash' },
      { key: 3, value: 'Replace with Space Dash Space' },
      { key: 4, value: 'Smart Replace', hint: 'Dash or Space Dash depending on name' }
    ];

    const standardEpisodeFormatHelpTexts = [];
    const standardEpisodeFormatErrors = [];
    const seriesFolderFormatHelpTexts = [];
    const seriesFolderFormatErrors = [];

    if (examplesPopulated) {
      if (examples.singleEpisodeExample) {
        standardEpisodeFormatHelpTexts.push(`Single Episode: ${examples.singleEpisodeExample}`);
      } else {
        standardEpisodeFormatErrors.push({ message: 'Single Episode: Invalid Format' });
      }

      if (examples.seriesFolderExample) {
        seriesFolderFormatHelpTexts.push(`Example: ${examples.seriesFolderExample}`);
      } else {
        seriesFolderFormatErrors.push({ message: 'Invalid Format' });
      }
    }

    return (
      <FieldSet legend="Episode Naming">
        {
          isFetching &&
            <LoadingIndicator />
        }

        {
          !isFetching && error &&
            <Alert kind={kinds.DANGER}>Unable to load Naming settings</Alert>
        }

        {
          hasSettings && !isFetching && !error &&
            <Form>
              <FormGroup size={sizes.MEDIUM}>
                <FormLabel>Rename Episodes</FormLabel>

                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="renameEpisodes"
                  helpText="Whisparr will use the existing file name if renaming is disabled"
                  onChange={onInputChange}
                  {...settings.renameEpisodes}
                />
              </FormGroup>

              <FormGroup size={sizes.MEDIUM}>
                <FormLabel>Replace Illegal Characters</FormLabel>

                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="replaceIllegalCharacters"
                  helpText="Replace illegal characters. If unchecked, Whisparr will remove them instead"
                  onChange={onInputChange}
                  {...settings.replaceIllegalCharacters}
                />
              </FormGroup>

              {
                replaceIllegalCharacters ?
                  <FormGroup>
                    <FormLabel>Colon Replacement</FormLabel>

                    <FormInputGroup
                      type={inputTypes.SELECT}
                      name="colonReplacementFormat"
                      values={colonReplacementOptions}
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
                      <FormLabel>Standard Episode Format</FormLabel>

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
                <FormLabel>Site Folder Format</FormLabel>

                <FormInputGroup
                  inputClassName={styles.namingInput}
                  type={inputTypes.TEXT}
                  name="seriesFolderFormat"
                  buttons={<FormInputButton onPress={this.onSeriesFolderNamingModalOpenClick}>?</FormInputButton>}
                  onChange={onInputChange}
                  {...settings.seriesFolderFormat}
                  helpTexts={['Used when adding a new site or moving sites via the site editor', ...seriesFolderFormatHelpTexts]}
                  errors={[...seriesFolderFormatErrors, ...settings.seriesFolderFormat.errors]}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>Multi-Episode Style</FormLabel>

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
