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
      isEditImportListExclusionModalOpen: false,
      isDeleteImportListExclusionModalOpen: false
    };
  }

  //
  // Listeners

  onEditImportListExclusionPress = () => {
    this.setState({ isEditImportListExclusionModalOpen: true });
  };

  onEditImportListExclusionModalClose = () => {
    this.setState({ isEditImportListExclusionModalOpen: false });
  };

  onDeleteImportListExclusionPress = () => {
    this.setState({
      isEditImportListExclusionModalOpen: false,
      isDeleteImportListExclusionModalOpen: true
    });
  };

  onDeleteImportListExclusionModalClose = () => {
    this.setState({ isDeleteImportListExclusionModalOpen: false });
  };

  onConfirmDeleteImportListExclusion = () => {
    this.props.onConfirmDeleteImportListExclusion(this.props.id);
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
      <div className={styles.importListExclusionInfoContainer}>
        <div className={styles.type}>{type}</div>
        <div className={styles.foreignId}>{foreignId}</div>
        <div className={styles.title} title={movieTitle}><span>{movieTitle}</span></div>

        <div className={styles.actions}>
          <Link
            onPress={this.onEditImportListExclusionPress}
          >
            <Icon name={icons.EDIT} />
          </Link>
        </div>

        <EditImportListExclusionModalConnector
          id={id}
          isOpen={this.state.isEditImportListExclusionModalOpen}
          onModalClose={this.onEditImportListExclusionModalClose}
          onDeleteImportListExclusionPress={this.onDeleteImportListExclusionPress}
        />

        <ConfirmModal
          isOpen={this.state.isDeleteImportListExclusionModalOpen}
          kind={kinds.DANGER}
          title={translate('DeleteImportListExclusion')}
          message={translate('DeleteImportListExclusionMessageText')}
          confirmLabel={translate('Delete')}
          onConfirm={this.onConfirmDeleteImportListExclusion}
          onCancel={this.onDeleteImportListExclusionModalClose}
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
  onConfirmDeleteImportListExclusion: PropTypes.func.isRequired
};

ImportListExclusion.defaultProps = {
  // The drag preview will not connect the drag handle.
  connectDragSource: (node) => node
};

export default ImportListExclusion;
