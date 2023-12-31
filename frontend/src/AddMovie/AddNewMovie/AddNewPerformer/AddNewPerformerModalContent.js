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
import MoviePoster from 'Movie/MoviePoster';
import translate from 'Utilities/String/translate';
import styles from './AddNewPerformerModalContent.css';

class AddNewPerformerModalContent extends Component {

  //
  // Listeners

  onQualityProfileIdChange = ({ value }) => {
    this.props.onInputChange({ name: 'qualityProfileId', value: parseInt(value) });
  };

  onAddMoviePress = () => {
    this.props.onAddMoviePress();
  };

  //
  // Render

  render() {
    const {
      name,
      images,
      isAdding,
      rootFolderPath,
      monitor,
      qualityProfileId,
      searchForMovie,
      folder,
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
          {name}
        </ModalHeader>

        <ModalBody>
          <div className={styles.container}>
            {
              !isSmallScreen &&
                <div className={styles.poster}>
                  <MoviePoster
                    blur={safeForWorkMode}
                    className={styles.poster}
                    images={images}
                    size={250}
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
                      movieFolder: folder,
                      isWindows
                    }}
                    selectedValueOptions={{
                      movieFolder: folder,
                      isWindows
                    }}
                    helpText={translate('SubfolderWillBeCreatedAutomaticallyInterp', [folder])}
                    onChange={onInputChange}
                    {...rootFolderPath}
                  />
                </FormGroup>

                <FormGroup>
                  <FormLabel>
                    {translate('Monitor')}
                  </FormLabel>

                  <FormInputGroup
                    type={inputTypes.MOVIE_MONITORED_SELECT}
                    name="monitor"
                    onChange={onInputChange}
                    {...monitor}
                  />
                </FormGroup>

                <FormGroup>
                  <FormLabel>{translate('QualityProfile')}</FormLabel>

                  <FormInputGroup
                    type={inputTypes.QUALITY_PROFILE_SELECT}
                    name="qualityProfileId"
                    onChange={this.onQualityProfileIdChange}
                    {...qualityProfileId}
                  />
                </FormGroup>

                <FormGroup>
                  <FormLabel>{translate('Tags')}</FormLabel>

                  <FormInputGroup
                    type={inputTypes.TAG}
                    name="tags"
                    onChange={onInputChange}
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
              {translate('StartSearchForMissingMovie')}
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
            onPress={this.onAddMoviePress}
          >
            {translate('AddPerformer')}
          </SpinnerButton>
        </ModalFooter>
      </ModalContent>
    );
  }
}

AddNewPerformerModalContent.propTypes = {
  name: PropTypes.string.isRequired,
  images: PropTypes.arrayOf(PropTypes.object).isRequired,
  isAdding: PropTypes.bool.isRequired,
  addError: PropTypes.object,
  rootFolderPath: PropTypes.object,
  monitor: PropTypes.object.isRequired,
  qualityProfileId: PropTypes.object,
  searchForMovie: PropTypes.object.isRequired,
  folder: PropTypes.string.isRequired,
  tags: PropTypes.object.isRequired,
  isSmallScreen: PropTypes.bool.isRequired,
  isWindows: PropTypes.bool.isRequired,
  onModalClose: PropTypes.func.isRequired,
  onInputChange: PropTypes.func.isRequired,
  onAddMoviePress: PropTypes.func.isRequired,
  safeForWorkMode: PropTypes.bool
};

export default AddNewPerformerModalContent;
