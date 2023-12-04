import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import Button from 'Components/Link/Button';
import SpinnerButton from 'Components/Link/SpinnerButton';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { inputTypes } from 'Helpers/Props';
import MovieHeadshot from 'Movie/MovieHeadshot';
import translate from 'Utilities/String/translate';
import styles from './EditPerformerModalContent.css';

class EditPerformerModalContent extends Component {

  //
  // Listeners

  onSavePress = () => {
    const {
      onSavePress
    } = this.props;

    onSavePress(false);
  };

  //
  // Render

  render() {
    const {
      fullName,
      images,
      item,
      isSaving,
      onInputChange,
      onModalClose,
      isSmallScreen,
      ...otherProps
    } = this.props;

    const {
      monitored,
      qualityProfileId,
      // Id,
      rootFolderPath,
      tags,
      searchOnAdd
    } = item;

    return (
      <ModalContent onModalClose={onModalClose}>
        <ModalHeader>
          {translate('Edit')} - {fullName}
        </ModalHeader>

        <ModalBody>
          <div className={styles.container}>
            {
              !isSmallScreen &&
                <div className={styles.poster}>
                  <MovieHeadshot
                    className={styles.poster}
                    images={images}
                    size={250}
                  />
                </div>
            }

            <div className={styles.info}>
              <Form
                {...otherProps}
              >
                <FormGroup>
                  <FormLabel>{translate('Monitored')}</FormLabel>

                  <FormInputGroup
                    type={inputTypes.CHECK}
                    name="monitored"
                    helpText={translate('MonitoredPerformerHelpText')}
                    {...monitored}
                    onChange={onInputChange}
                  />
                </FormGroup>

                <FormGroup>
                  <FormLabel>{translate('QualityProfile')}</FormLabel>

                  <FormInputGroup
                    type={inputTypes.QUALITY_PROFILE_SELECT}
                    name="qualityProfileId"
                    {...qualityProfileId}
                    onChange={onInputChange}
                  />
                </FormGroup>

                <FormGroup>
                  <FormLabel>{translate('RootFolder')}</FormLabel>

                  <FormInputGroup
                    type={inputTypes.ROOT_FOLDER_SELECT}
                    name="rootFolderPath"
                    {...rootFolderPath}
                    includeMissingValue={true}
                    onChange={onInputChange}
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

                <FormGroup>
                  <FormLabel>{translate('SearchOnAdd')}</FormLabel>

                  <FormInputGroup
                    type={inputTypes.CHECK}
                    name="searchOnAdd"
                    helpText={translate('SearchOnAddPerformerHelpText')}
                    {...searchOnAdd}
                    onChange={onInputChange}
                  />
                </FormGroup>
              </Form>
            </div>
          </div>
        </ModalBody>

        <ModalFooter>
          <Button
            onPress={onModalClose}
          >
            {translate('Cancel')}
          </Button>

          <SpinnerButton
            isSpinning={isSaving}
            onPress={this.onSavePress}
          >
            {translate('Save')}
          </SpinnerButton>
        </ModalFooter>
      </ModalContent>
    );
  }
}

EditPerformerModalContent.propTypes = {
  performerId: PropTypes.number.isRequired,
  fullName: PropTypes.string.isRequired,
  images: PropTypes.arrayOf(PropTypes.object).isRequired,
  item: PropTypes.object.isRequired,
  isSaving: PropTypes.bool.isRequired,
  isPathChanging: PropTypes.bool.isRequired,
  isSmallScreen: PropTypes.bool.isRequired,
  onInputChange: PropTypes.func.isRequired,
  onSavePress: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default EditPerformerModalContent;
