import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import { icons, kinds } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import EditImportListExclusionModalConnector from './EditImportListExclusionModalConnector';
import styles from './ImportListExclusions.css';

class ImportListExclusion extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isEditImportExclusionModalOpen: false,
      isDeleteImportExclusionModalOpen: false
    };
  }

  //
  // Listeners

  onEditImportExclusionPress = () => {
    this.setState({ isEditImportExclusionModalOpen: true });
  };

  onEditImportExclusionModalClose = () => {
    this.setState({ isEditImportExclusionModalOpen: false });
  };

  onDeleteImportExclusionPress = () => {
    this.setState({
      isEditImportExclusionModalOpen: false,
      isDeleteImportExclusionModalOpen: true
    });
  };

  onDeleteImportExclusionModalClose = () => {
    this.setState({ isDeleteImportExclusionModalOpen: false });
  };

  onConfirmDeleteImportExclusion = () => {
    this.props.onConfirmDeleteImportExclusion(this.props.id);
  };

  //
  // Render

  render() {
    const {
      id,
      movieTitle,
      foreignId,
      type
    } = this.props;

    return (
      <div className={styles.importExclusionInfoContainer}>
        <div className={styles.type}>{type}</div>
        <div className={styles.foreignId}>{foreignId}</div>
        <div className={styles.title} title={movieTitle}><span>{movieTitle}</span></div>

        <div className={styles.actions}>
          <Link
            onPress={this.onEditImportExclusionPress}
          >
            <Icon name={icons.EDIT} />
          </Link>
        </div>

        <EditImportListExclusionModalConnector
          id={id}
          isOpen={this.state.isEditImportExclusionModalOpen}
          onModalClose={this.onEditImportExclusionModalClose}
          onDeleteImportExclusionPress={this.onDeleteImportExclusionPress}
        />

        <ConfirmModal
          isOpen={this.state.isDeleteImportExclusionModalOpen}
          kind={kinds.DANGER}
          title={translate('DeleteImportListExclusion')}
          message={translate('DeleteImportListExclusionMessageText')}
          confirmLabel={translate('Delete')}
          onConfirm={this.onConfirmDeleteImportExclusion}
          onCancel={this.onDeleteImportExclusionModalClose}
        />
      </div>
    );
  }
}

ImportListExclusion.propTypes = {
  id: PropTypes.number.isRequired,
  movieTitle: PropTypes.string.isRequired,
  foreignId: PropTypes.string.isRequired,
  type: PropTypes.string.isRequired,
  movieYear: PropTypes.number,
  onConfirmDeleteImportExclusion: PropTypes.func.isRequired
};

ImportListExclusion.defaultProps = {
  // The drag preview will not connect the drag handle.
  connectDragSource: (node) => node
};

export default ImportListExclusion;
