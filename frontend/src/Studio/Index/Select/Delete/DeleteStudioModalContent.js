import PropTypes from 'prop-types';
import React, { Component } from 'react';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import InfoLabel from 'Components/InfoLabel';
import Button from 'Components/Link/Button';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { inputTypes, kinds, sizes } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import styles from './DeleteStudioModal.css';

class DeleteStudioModalContent extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      deleteFiles: false,
      addImportExclusion: false
    };
  }

  //
  // Listeners
  onDeleteOptionChange = ({ value }) => {
    this.setState({ addImportExclusion: value });
  };

  onDeleteFilesChange = ({ value }) => {
    this.setState({ deleteFiles: value });
  };

  onDeleteStudioConfirmed = () => {
    const deleteFiles = this.state.deleteFiles;
    const addImportExclusion = this.state.addImportExclusion;

    this.props.onDeletePress(deleteFiles, addImportExclusion);
  };

  //
  // Render

  render() {
    const {
      onModalClose
    } = this.props;

    const deleteFiles = this.state.deleteFiles;
    const addImportExclusion = this.state.addImportExclusion;

    return (
      <ModalContent
        onModalClose={onModalClose}
      >
        <ModalHeader>
          {translate('DeleteStudiosModalHeader')}
        </ModalHeader>

        <ModalBody>
          <FormGroup>
            <InfoLabel size={sizes.LARGE} className={styles.warningText}>
              {translate('DeleteStudiosModalWarning')}
            </InfoLabel>
          </FormGroup>
          <FormGroup>
            <FormLabel>
              {translate('AddListExclusion')}
            </FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="addImportExclusion"
              value={addImportExclusion}
              helpText={translate('AddImportExclusionHelpText')}
              kind={kinds.DANGER}
              onChange={this.onDeleteOptionChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>{translate('DeleteFilesLabel', [translate('All')])}</FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="deleteFiles"
              value={deleteFiles}
              helpText={translate('DeleteFilesHelpText')}
              kind={kinds.DANGER}
              onChange={this.onDeleteFilesChange}
            />
          </FormGroup>

        </ModalBody>

        <ModalFooter>
          <Button onPress={onModalClose}>
            {translate('Close')}
          </Button>

          <Button
            kind={kinds.DANGER}
            onPress={this.onDeleteStudioConfirmed}
          >
            {translate('Delete')}
          </Button>
        </ModalFooter>
      </ModalContent>
    );
  }
}

DeleteStudioModalContent.propTypes = {
  studioIds: PropTypes.arrayOf(PropTypes.number),
  deleteOptions: PropTypes.object.isRequired,
  onDeleteOptionChange: PropTypes.func.isRequired,
  onDeleteFilesChange: PropTypes.func.isRequired,
  onDeletePress: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

DeleteStudioModalContent.defaultProps = {
  statistics: {}
};

export default DeleteStudioModalContent;
