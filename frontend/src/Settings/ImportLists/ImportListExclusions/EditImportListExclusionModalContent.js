import PropTypes from 'prop-types';
import React from 'react';
import Alert from 'Components/Alert';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import Button from 'Components/Link/Button';
import SpinnerErrorButton from 'Components/Link/SpinnerErrorButton';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { inputTypes, kinds } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import styles from './EditImportListExclusionModalContent.css';

function EditImportListExclusionModalContent(props) {
  const {
    id,
    isFetching,
    error,
    isSaving,
    saveError,
    item,
    onInputChange,
    onSavePress,
    onModalClose,
    onDeleteImportExclusionPress,
    ...otherProps
  } = props;

  const {
    movieTitle = '',
    foreignId = '',
    type = 'scene',
    movieYear
  } = item;

  const typeOptions = [
    { key: 'movie', value: translate('Movie') },
    { key: 'scene', value: translate('Scene') },
    { key: 'studio', value: translate('Studio') },
    { key: 'performer', value: translate('Performer') },
    { key: 'tag', value: translate('Tag') }
  ];

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {id ? translate('EditImportListExclusion') : translate('AddImportListExclusion')}
      </ModalHeader>

      <ModalBody className={styles.body}>
        {
          isFetching &&
            <LoadingIndicator />
        }

        {
          !isFetching && !!error &&
            <Alert kind={kinds.DANGER}>
              {translate('AddImportListExclusionError')}
            </Alert>
        }

        {
          !isFetching && !error &&
            <Form
              {...otherProps}
            >
              <FormGroup>
                <FormLabel>{translate('ForiegnId')}</FormLabel>

                <FormInputGroup
                  type={inputTypes.TEXT}
                  name="foreignId"
                  helpText={translate('ForiegnIdHelpText')}
                  {...foreignId}
                  onChange={onInputChange}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>{translate('ExclusionTitle')}</FormLabel>

                <FormInputGroup
                  type={inputTypes.TEXT}
                  name="movieTitle"
                  helpText={translate('ExclusionTitleHelpText')}
                  {...movieTitle}
                  onChange={onInputChange}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>{translate('ExclusionType')}</FormLabel>

                <FormInputGroup
                  type={inputTypes.SELECT}
                  name="type"
                  {...type}
                  values={typeOptions}
                  helpText={translate('ExclusionTypeHelpText')}
                  onChange={onInputChange}
                />

              </FormGroup>

              <FormGroup>
                <FormLabel>{translate('MovieYear')}</FormLabel>

                <FormInputGroup
                  type={inputTypes.NUMBER}
                  name="movieYear"
                  helpText={translate('MovieYearHelpText')}
                  {...movieYear}
                  onChange={onInputChange}
                />
              </FormGroup>

            </Form>
        }
      </ModalBody>

      <ModalFooter>
        {
          id &&
            <Button
              className={styles.deleteButton}
              kind={kinds.DANGER}
              onPress={onDeleteImportExclusionPress}
            >
              {translate('Delete')}
            </Button>
        }

        <Button
          onPress={onModalClose}
        >
          {translate('Cancel')}
        </Button>

        <SpinnerErrorButton
          isSpinning={isSaving}
          error={saveError}
          onPress={onSavePress}
        >
          {translate('Save')}
        </SpinnerErrorButton>
      </ModalFooter>
    </ModalContent>
  );
}

EditImportListExclusionModalContent.propTypes = {
  id: PropTypes.number,
  isFetching: PropTypes.bool.isRequired,
  error: PropTypes.object,
  isSaving: PropTypes.bool.isRequired,
  saveError: PropTypes.object,
  item: PropTypes.object.isRequired,
  onInputChange: PropTypes.func.isRequired,
  onSavePress: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired,
  onDeleteImportExclusionPress: PropTypes.func
};

export default EditImportListExclusionModalContent;
