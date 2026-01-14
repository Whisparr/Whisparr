import PropTypes from 'prop-types';
import React, { Component } from 'react';
import CheckInput from 'Components/Form/CheckInput';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import SpinnerButton from 'Components/Link/SpinnerButton';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { inputTypes, kinds } from 'Helpers/Props';
import MovieHeadshot from 'Movie/MovieHeadshot';
import translate from 'Utilities/String/translate';
import styles from './AddNewPerformerModalContent.css';

class AddNewPerformerModalContent extends Component {

  //
  // Listeners

  onQualityProfileIdChange = ({ value }) => {
    this.props.onInputChange({ name: 'qualityProfileId', value: parseInt(value) });
  };

  onAddPerformerPress = () => {
    this.props.onAddPerformerPress();
  };

  //
  // Render

  render() {
    const {
      fullName,
      images,
      isAdding,
      rootFolderPath,
      monitored,
      moviesMonitored,
      qualityProfileId,
      searchForMovie,
      tags,
      isSmallScreen,
      safeForWorkMode,
      isWindows,
      onModalClose,
      onInputChange
    } = this.props;

    return (
      <ModalContent onModalClose={onModalClose}>
        <ModalHeader>
          {fullName}
        </ModalHeader>

        <ModalBody>
          <div className={styles.container}>
            {
              !isSmallScreen &&
                <div className={styles.poster}>
                  <MovieHeadshot
                    safeForWorkMode={safeForWorkMode}
                    className={styles.poster}
                    images={images}
                    size={250}
                    overflow={true}
                    lazy={true}
                  />
                </div>
            }

            <div className={styles.info}>

              <Form>
                <FormGroup>
                  <FormLabel>{translate('RootFolder')}</FormLabel>

                  <FormInputGroup
                    type={inputTypes.ROOT_FOLDER_SELECT}
                    name="rootFolderPath"
                    valueOptions={{
                      isWindows
                    }}
                    selectedValueOptions={{
                      isWindows
                    }}
                    errors={rootFolderPath.errors}
                    onChange={onInputChange}
                    {...rootFolderPath}
                  />
                </FormGroup>

                <FormGroup>
                  <FormLabel>{translate('MonitoredScene')}</FormLabel>

                  <FormInputGroup
                    type={inputTypes.CHECK}
                    name="monitored"
                    helpText={translate('MonitoredPerformerHelpText')}
                    {...monitored}
                    errors={monitored.errors}
                    onChange={onInputChange}
                  />
                </FormGroup>

                <FormGroup>
                  <FormLabel>{translate('MonitoredMovie')}</FormLabel>

                  <FormInputGroup
                    type={inputTypes.CHECK}
                    name="moviesMonitored"
                    helpText={translate('MonitoredPerformerMovieHelpText')}
                    {...moviesMonitored}
                    error={moviesMonitored.errors}
                    onChange={onInputChange}
                  />
                </FormGroup>

                <FormGroup>
                  <FormLabel>{translate('QualityProfile')}</FormLabel>

                  <FormInputGroup
                    type={inputTypes.QUALITY_PROFILE_SELECT}
                    name="qualityProfileId"
                    onChange={this.onQualityProfileIdChange}
                    errors={qualityProfileId.errors}
                    {...qualityProfileId}
                  />
                </FormGroup>

                <FormGroup>
                  <FormLabel>{translate('Tags')}</FormLabel>

                  <FormInputGroup
                    type={inputTypes.TAG}
                    name="tags"
                    onChange={onInputChange}
                    errors={tags.errors}
                    {...tags}
                  />
                </FormGroup>
              </Form>
            </div>
          </div>
        </ModalBody>

        <ModalFooter className={styles.modalFooter}>
          <label className={styles.searchForMissingMovieLabelContainer}>
            <span className={styles.searchForMissingMovieLabel}>
              {translate('SearchOnAddPerformerHelpText')}
            </span>

            <CheckInput
              containerClassName={styles.searchForMissingMovieContainer}
              className={styles.searchForMissingMovieInput}
              name="searchForMovie"
              onChange={onInputChange}
              {...searchForMovie}
            />
          </label>

          <SpinnerButton
            className={styles.addButton}
            kind={kinds.SUCCESS}
            isSpinning={isAdding}
            onPress={this.onAddPerformerPress}
          >
            {translate('AddPerformer')}
          </SpinnerButton>
        </ModalFooter>
      </ModalContent>
    );
  }
}

AddNewPerformerModalContent.propTypes = {
  fullName: PropTypes.string.isRequired,
  images: PropTypes.arrayOf(PropTypes.object).isRequired,
  isAdding: PropTypes.bool.isRequired,
  addError: PropTypes.object,
  rootFolderPath: PropTypes.object,
  monitored: PropTypes.bool.isRequired,
  moviesMonitored: PropTypes.bool,
  qualityProfileId: PropTypes.object,
  searchForMovie: PropTypes.object.isRequired,
  tags: PropTypes.object.isRequired,
  isSmallScreen: PropTypes.bool.isRequired,
  isWindows: PropTypes.bool.isRequired,
  onModalClose: PropTypes.func.isRequired,
  onInputChange: PropTypes.func.isRequired,
  onAddPerformerPress: PropTypes.func.isRequired,
  safeForWorkMode: PropTypes.bool
};

export default AddNewPerformerModalContent;
