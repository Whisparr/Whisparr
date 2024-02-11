import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Alert from 'Components/Alert';
import CheckInput from 'Components/Form/CheckInput';
import Button from 'Components/Link/Button';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import InlineMarkdown from 'Components/Markdown/InlineMarkdown';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { kinds } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import getSelectedIds from 'Utilities/Table/getSelectedIds';
import selectAll from 'Utilities/Table/selectAll';
import toggleSelected from 'Utilities/Table/toggleSelected';
import OrganizePreviewRow from './OrganizePreviewRow';
import styles from './OrganizePreviewModalContent.css';

function getValue(allSelected, allUnselected) {
  if (allSelected) {
    return true;
  } else if (allUnselected) {
    return false;
  }

  return null;
}

class OrganizePreviewModalContent extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      allSelected: false,
      allUnselected: false,
      lastToggled: null,
      selectedState: {}
    };
  }

  //
  // Control

  getSelectedIds = () => {
    return getSelectedIds(this.state.selectedState);
  };

  //
  // Listeners

  onSelectAllChange = ({ value }) => {
    this.setState(selectAll(this.state.selectedState, value));
  };

  onSelectedChange = ({ id, value, shiftKey = false }) => {
    this.setState((state) => {
      return toggleSelected(state, this.props.items, id, value, shiftKey);
    });
  };

  onOrganizePress = () => {
    this.props.onOrganizePress(this.getSelectedIds());
  };

  //
  // Render

  render() {
    const {
      isFetching,
      isPopulated,
      error,
      items,
      renameMovies,
      renameScenes,
      standardMovieFormat,
      standardSceneFormat,
      path,
      itemType,
      onModalClose
    } = this.props;

    const {
      allSelected,
      allUnselected,
      selectedState
    } = this.state;

    const selectAllValue = getValue(allSelected, allUnselected);
    const renameEnabled = (itemType === 'movie' && renameMovies) || (itemType === 'scene' && renameScenes);

    return (
      <ModalContent onModalClose={onModalClose}>
        <ModalHeader>
          {translate('OrganizeModalHeader')}
        </ModalHeader>

        <ModalBody>
          {
            isFetching &&
              <LoadingIndicator />
          }

          {
            !isFetching && error &&
              <div>{translate('OrganizeLoadError')}</div>
          }

          {
            !isFetching && isPopulated && !items.length &&
              <div>
                {
                  renameEnabled ?
                    <div>{translate('OrganizeNothingToRename')}</div> :
                    <div>{translate('OrganizeRenamingDisabled')}</div>
                }
              </div>
          }

          {
            !isFetching && isPopulated && !!items.length &&
              <div>
                <Alert>
                  <div>
                    <InlineMarkdown data={translate('OrganizeRelativePaths', { path })} blockClassName={styles.path} />
                  </div>

                  <div>
                    {itemType === 'movie' ?
                      <InlineMarkdown data={translate('OrganizeMovieNamingPattern', { standardMovieFormat })} blockClassName={styles.standardMovieFormat} /> :
                      <InlineMarkdown data={translate('OrganizeSceneNamingPattern', { standardSceneFormat })} blockClassName={styles.standardMovieFormat} />
                    }
                  </div>
                </Alert>

                <div className={styles.previews}>
                  {
                    items.map((item) => {
                      return (
                        <OrganizePreviewRow
                          key={item.movieFileId}
                          id={item.movieFileId}
                          existingPath={item.existingPath}
                          newPath={item.newPath}
                          isSelected={selectedState[item.movieFileId]}
                          onSelectedChange={this.onSelectedChange}
                        />
                      );
                    })
                  }
                </div>
              </div>
          }
        </ModalBody>

        <ModalFooter>
          {
            isPopulated && !!items.length &&
              <CheckInput
                className={styles.selectAllInput}
                containerClassName={styles.selectAllInputContainer}
                name="selectAll"
                value={selectAllValue}
                onChange={this.onSelectAllChange}
              />
          }

          <Button
            onPress={onModalClose}
          >
            {translate('Cancel')}
          </Button>

          <Button
            kind={kinds.PRIMARY}
            onPress={this.onOrganizePress}
          >
            {translate('Organize')}
          </Button>
        </ModalFooter>
      </ModalContent>
    );
  }
}

OrganizePreviewModalContent.propTypes = {
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  error: PropTypes.object,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  path: PropTypes.string.isRequired,
  itemType: PropTypes.string.isRequired,
  renameMovies: PropTypes.bool,
  renameScenes: PropTypes.bool,
  standardMovieFormat: PropTypes.string,
  standardSceneFormat: PropTypes.string,
  onOrganizePress: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default OrganizePreviewModalContent;
