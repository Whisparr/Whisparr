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
import StudioLogo from 'Studio/StudioLogo';
import translate from 'Utilities/String/translate';
import styles from './EditStudioModalContent.css';

class EditStudioModalContent extends Component {

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
      title,
      images,
      overview,
      item,
      isSaving,
      onInputChange,
      onModalClose,
      isSmallScreen,
      ...otherProps
    } = this.props;

    const {
      monitored,
      moviesMonitored,
      afterDate,
      qualityProfileId,
      // Id,
      rootFolderPath,
      tags,
      searchTitle,
      searchOnAdd
    } = item;

    return (
      <ModalContent onModalClose={onModalClose}>
        <ModalHeader>
          {translate('Edit')} - {title}
        </ModalHeader>

        <ModalBody>
          <div className={styles.container}>
            {
              !isSmallScreen &&
                <div className={styles.poster}>
                  <StudioLogo
                    className={styles.poster}
                    images={images}
                    size={250}
                  />
                </div>
            }

            <div className={styles.info}>
              <div className={styles.overview}>
                {overview}
              </div>

              <Form
                {...otherProps}
              >
                <FormGroup>
                  <FormLabel>{translate('MonitoredScene')}</FormLabel>

                  <FormInputGroup
                    type={inputTypes.CHECK}
                    name="monitored"
                    helpText={translate('MonitoredStudioHelpText')}
                    {...monitored}
                    onChange={onInputChange}
                  />
                </FormGroup>

                <FormGroup>
                  <FormLabel>{translate('MonitoredMovie')}</FormLabel>

                  <FormInputGroup
                    type={inputTypes.CHECK}
                    name="moviesMonitored"
                    helpText={translate('MonitoredStudioHelpText')}
                    {...moviesMonitored}
                    onChange={onInputChange}
                  />
                </FormGroup>

                <FormGroup>
                  <FormLabel>{translate('MonitorAfter')}</FormLabel>

                  <FormInputGroup
                    type={inputTypes.DATE}
                    name="afterDate"
                    helpText={translate('MonitorAfterStudioHelpText')}
                    {...afterDate}
                    onChange={onInputChange}
                  />
                </FormGroup>

                <FormGroup>
                  <FormLabel>{translate('QualityProfile')}</FormLabel>

                  <FormInputGroup
                    type={inputTypes.QUALITY_PROFILE_SELECT}
                    helpText={translate('StudioQualityProfileHelpText')}
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
                    includeMissingValue={false}
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
                  <FormLabel>{translate('SearchTitle')}</FormLabel>

                  <FormInputGroup
                    type={inputTypes.TEXT}
                    name="searchTitle"
                    onChange={onInputChange}
                    {...searchTitle}
                  />
                </FormGroup>

                <FormGroup>
                  <FormLabel>{translate('SearchOnAdd')}</FormLabel>

                  <FormInputGroup
                    type={inputTypes.CHECK}
                    name="searchOnAdd"
                    helpText={translate('SearchOnAddStudioHelpText')}
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

EditStudioModalContent.propTypes = {
  studioId: PropTypes.number.isRequired,
  title: PropTypes.string.isRequired,
  overview: PropTypes.string,
  images: PropTypes.arrayOf(PropTypes.object).isRequired,
  item: PropTypes.object.isRequired,
  isSaving: PropTypes.bool.isRequired,
  isPathChanging: PropTypes.bool.isRequired,
  isSmallScreen: PropTypes.bool.isRequired,
  onInputChange: PropTypes.func.isRequired,
  onSavePress: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default EditStudioModalContent;
