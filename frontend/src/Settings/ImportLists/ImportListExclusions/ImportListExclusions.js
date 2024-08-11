import PropTypes from 'prop-types';
import React, { Component } from 'react';
import FieldSet from 'Components/FieldSet';
import CheckInput from 'Components/Form/CheckInput';
import FormInputGroup from 'Components/Form/FormInputGroup';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import SpinnerButton from 'Components/Link/SpinnerButton';
import PageSectionContent from 'Components/Page/PageSectionContent';
import { icons, inputTypes, kinds } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import EditImportListExclusionModalConnector from './EditImportListExclusionModalConnector';
import ImportListExclusion from './ImportListExclusion';
import styles from './ImportListExclusions.css';

class ImportListExclusions extends Component {
  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isAddImportListExclusionModalOpen: false,
      selectedExclusionType: 'all',
      selectedExclusions: new Set(),
      sortColumn: null, // 'type', 'foreignId', or 'title'
      sortDirection: 'asc' // or 'desc'
    };
  }

  //
  // Listeners

  onAddImportListExclusionPress = () => {
    this.setState({ isAddImportListExclusionModalOpen: true });
  };

  onModalClose = () => {
    this.setState({ isAddImportListExclusionModalOpen: false });
  };

  onInputChange = (option) => {
    // If option is { key: 'scene', value: 'Scene' }
    this.setState({ selectedExclusionType: option.value });
  };

  onCheckboxChange = (id, checked) => {
    this.setState((prevState) => {
      const selected = new Set(prevState.selectedExclusions);
      if (checked) {
        selected.add(id);
      } else {
        selected.delete(id);
      }
      return { selectedExclusions: selected };
    });
  };

  onSelectAllChange = ({ value }) => {
    const { filteredItems } = this;
    if (value) {
      // Select all
      this.setState({
        selectedExclusions: new Set(filteredItems.map((item) => item.id))
      });
    } else {
      // Deselect all
      this.setState({ selectedExclusions: new Set() });
    }
  };

  onDeleteSelected = () => {
    const { onConfirmDeleteImportListExclusion } = this.props;
    const selectedIds = Array.from(this.state.selectedExclusions);
    if (selectedIds.length > 0) {
      selectedIds.forEach((id) => {
        onConfirmDeleteImportListExclusion(id);
      });
      // Optionally clear selection after delete:
      this.setState({ selectedExclusions: new Set() });
    }
  };

  onSortColumn = (column) => {
    this.setState((prevState) => {
      const isSameColumn = prevState.sortColumn === column;
      const direction =
        isSameColumn && prevState.sortDirection === 'asc' ? 'desc' : 'asc';
      return {
        sortColumn: column,
        sortDirection: direction
      };
    });
  };

  renderSortIndicator = (column) => {
    const { sortColumn, sortDirection } = this.state;
    if (sortColumn !== column) {
      return null;
    }
    return sortDirection === 'asc' ? ' ↑' : ' ↓';
  };
  //
  // Render

  render() {
    const { items, onConfirmDeleteImportListExclusion, ...otherProps } = this.props;
    const { selectedExclusionType, selectedExclusions } = this.state;

    let filteredItems =
      selectedExclusionType === 'all' ?
        items :
        items.filter((item) => item.type === selectedExclusionType);

    // Apply sorting
    const { sortColumn, sortDirection } = this.state;
    if (sortColumn) {
      filteredItems = [...filteredItems].sort((a, b) => {
        const valA = a[sortColumn]?.toLowerCase?.() ?? '';
        const valB = b[sortColumn]?.toLowerCase?.() ?? '';
        if (valA < valB) {
          return sortDirection === 'asc' ? -1 : 1;
        }
        if (valA > valB) {
          return sortDirection === 'asc' ? 1 : -1;
        }
        return 0;
      });
    }
    this.filteredItems = filteredItems;

    const exclusionTypes = [
      { key: 'all', value: translate('All') },
      { key: 'scene', value: translate('Scene') },
      { key: 'movie', value: translate('Movie') },
      { key: 'studio', value: translate('Studio') },
      { key: 'performer', value: translate('Performer') },
      { key: 'tag', value: translate('Tag') }
    ];

    const allIds = filteredItems.map((item) => item.id);
    const allSelected =
      allIds.length > 0 && allIds.every((id) => selectedExclusions.has(id));
    const noneSelected = allIds.every((id) => !selectedExclusions.has(id));
    const someSelected = !allSelected && !noneSelected;
    return (
      <FieldSet
        className={styles.importExclusionConatiner}
        legend={translate('ImportListExclusions')}
      >
        <PageSectionContent
          errorMessage={translate('ImportListExclusionsLoadError')}
          {...otherProps}
        >
          <div className={styles.importExclusionFilterForm}>
            <div className={styles.importExclusionLabel}>
              {translate('FilterByExclusionType')}
            </div>
            <div className={styles.importExclusionDropdownContainer}>
              <FormInputGroup
                type={inputTypes.SELECT}
                name="exclusionType"
                values={exclusionTypes}
                value={this.state.selectedExclusionType}
                onChange={this.onInputChange}
              />
            </div>
            <SpinnerButton
              type="button"
              isSpinning={this.props.isFetching}
              isDisabled={this.state.selectedExclusions.size === 0}
              kind={kinds.DANGER}
              onPress={this.onDeleteSelected}
            >
              {translate('DeleteSelected')}
            </SpinnerButton>
          </div>
          <div className={styles.importListExclusionsHeader}>
            <div className={styles.checkboxContainer}>
              <CheckInput
                {...otherProps}
                className={styles.checkbox}
                name="import-exclusion-selectall"
                value={allSelected}
                checkedValue={true}
                uncheckedValue={false}
                isDisabled={false}
                indeterminate={someSelected}
                onChange={this.onSelectAllChange}
              />
            </div>
            <div
              className={styles.type}
              onClick={() => this.onSortColumn('type')}
              style={{ cursor: 'pointer' }}
            >
              {translate('ExclusionType')}
              {this.renderSortIndicator('type')}
            </div>
            <div
              className={styles.foreignId}
              onClick={() => this.onSortColumn('foreignId')}
              style={{ cursor: 'pointer' }}
            >
              {translate('ForeignId')}
              {this.renderSortIndicator('foreignId')}
            </div>
            <div
              className={styles.title}
              onClick={() => this.onSortColumn('movieTitle')}
              style={{ cursor: 'pointer' }}
            >
              {translate('ExclusionTitle')}
              {this.renderSortIndicator('movieTitle')}
            </div>
            <div className={styles.actions}>
              <Link
                className={styles.addButton}
                onPress={this.onAddImportListExclusionPress}
              >
                <Icon name={icons.ADD} />
              </Link>
            </div>
          </div>

          <div>
            {filteredItems.map((item, index) => (
              <div key={item.id} className={styles.importListExclusionRow}>
                <div className={styles.checkboxContainer}>
                  <CheckInput
                    {...otherProps}
                    name={`import-exclusion-checkbox-${item.id}`}
                    style={{ margin: 'none' }}
                    value={this.state.selectedExclusions.has(item.id)}
                    checkedValue={true}
                    uncheckedValue={false}
                    onChange={({ value }) =>
                      this.onCheckboxChange(item.id, value)
                    }
                    isDisabled={false}
                  />
                </div>
                <ImportListExclusion
                  {...item}
                  {...otherProps}
                  index={index}
                  onConfirmDeleteImportListExclusion={
                    onConfirmDeleteImportListExclusion
                  }
                />
              </div>
            ))}
          </div>

          <EditImportListExclusionModalConnector
            isOpen={this.state.isAddImportListExclusionModalOpen}
            onModalClose={this.onModalClose}
          />
        </PageSectionContent>
      </FieldSet>
    );
  }
}

ImportListExclusions.propTypes = {
  isFetching: PropTypes.bool.isRequired,
  error: PropTypes.object,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  onConfirmDeleteImportListExclusion: PropTypes.func.isRequired
};

export default ImportListExclusions;
