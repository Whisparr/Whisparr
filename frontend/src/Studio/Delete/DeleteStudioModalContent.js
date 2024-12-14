import PropTypes from 'prop-types';
import React, { Component } from 'react';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import Button from 'Components/Link/Button';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { inputTypes, kinds } from 'Helpers/Props';
import translate from 'Utilities/String/translate';

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

  onDeleteFilesChange = ({ value }) => {
    this.setState({ deleteFiles: value });
  };

  onDeleteStudioConfirmed = () => {
    const deleteFiles = this.state.deleteFiles;
    const addImportExclusion = this.props.deleteOptions.addImportExclusion;

    this.setState({ deleteFiles: false });
    this.props.onDeletePress(deleteFiles, addImportExclusion);
  };

  //
  // Render

  render() {
    const {
      title,
      deleteOptions,
      onModalClose,
      onDeleteOptionChange
    } = this.props;

    const deleteFiles = this.state.deleteFiles;
    const addImportExclusion = deleteOptions.addImportExclusion;

    return (
      <ModalContent
        onModalClose={onModalClose}
      >
        <ModalHeader>
          {translate('DeleteHeader', [title])}
        </ModalHeader>

        <ModalBody>
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
              onChange={onDeleteOptionChange}
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
  title: PropTypes.string.isRequired,
  deleteOptions: PropTypes.object.isRequired,
  onDeleteOptionChange: PropTypes.func.isRequired,
  onDeletePress: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

DeleteStudioModalContent.defaultProps = {
  statistics: {}
};

export default DeleteStudioModalContent;
