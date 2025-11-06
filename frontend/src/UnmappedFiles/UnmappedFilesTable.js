/* eslint-disable indent */
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Alert from 'Components/Alert';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import PageToolbar from 'Components/Page/Toolbar/PageToolbar';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
import PageToolbarSection from 'Components/Page/Toolbar/PageToolbarSection';
import TableOptionsModalWrapper from 'Components/Table/TableOptions/TableOptionsModalWrapper';
import VirtualTable from 'Components/Table/VirtualTable';
import VirtualTableRow from 'Components/Table/VirtualTableRow';
import { align, icons, kinds } from 'Helpers/Props';
import hasDifferentItemsOrOrder from 'Utilities/Object/hasDifferentItemsOrOrder';
import translate from 'Utilities/String/translate';
import getSelectedIds from 'Utilities/Table/getSelectedIds';
import selectAll from 'Utilities/Table/selectAll';
import toggleSelected from 'Utilities/Table/toggleSelected';
import UnmappedFilesTableHeader from './UnmappedFilesTableHeader';
import UnmappedFilesTableRow from './UnmappedFilesTableRow';

class UnmappedFilesTable extends Component {
  constructor(props, context) {
    super(props, context);

    this.scrollerRef = React.createRef();

    this.state = {
      allSelected: false,
      allUnselected: false,
      lastToggled: null,
      selectedState: {},
      sortKey: null,
      sortDirection: 'asc'
    };
  }

  componentDidMount() {
    this.setSelectedState();
  }

  componentDidUpdate(prevProps) {
    const { items, isDeleting, deleteError } = this.props;

    if (hasDifferentItemsOrOrder(prevProps.items, items)) {
      this.setSelectedState();
    }

    const hasFinishedDeleting =
      prevProps.isDeleting && !isDeleting && !deleteError;

    if (hasFinishedDeleting) {
      this.onSelectAllChange({ value: false });
    }
  }

  onSortPress = (column) => {
    this.setState((prevState) => {
      const isSame = prevState.sortKey === column;
      const direction =
        isSame && prevState.sortDirection === 'asc' ? 'desc' : 'asc';
      console.log('Sort pressed: ', column);
      return {
        sortKey: column,
        sortDirection: direction
      };
    });
  };

  getSortedItems() {
    const { items } = this.props;
    const { sortKey, sortDirection } = this.state;

    if (!sortKey) {
      return items;
    }

    return [...items].sort((a, b) => {
      let valA = a[sortKey];
      let valB = b[sortKey];

      // Handle nested quality.name
      if (sortKey === 'quality') {
        valA = valA?.quality?.name?.toLowerCase() ?? '';
        valB = valB?.quality?.name?.toLowerCase() ?? '';
      }
      if (sortKey === 'path') {
        valA = a.originalFilePath;
        valB = b.originalFilePath;
        valA = valA?.toLowerCase() ?? '';
        valB = valB?.toLowerCase() ?? '';
        console.log('valA: ', valA);
        console.log('valB: ', valB);
      } else if (sortKey.toLowerCase().includes('date')) {
        valA = new Date(valA).getTime();
        valB = new Date(valB).getTime();
      } else if (sortKey === 'size') {
        valA = typeof valA === 'number' ? valA : parseFloat(valA) || 0;
        valB = typeof valB === 'number' ? valB : parseFloat(valB) || 0;
      } else {
        valA = valA?.toString().toLowerCase() ?? '';
        valB = valB?.toString().toLowerCase() ?? '';
      }

      if (valA < valB) {
        return sortDirection === 'asc' ? -1 : 1;
      }
      if (valA > valB) {
        return sortDirection === 'asc' ? 1 : -1;
      }
      return 0;
    });
  }

  getSelectedIds = () => {
    if (this.state.allUnselected) {
      return [];
    }
    return getSelectedIds(this.state.selectedState);
  };

  setSelectedState() {
    const { items } = this.props;
    const { selectedState } = this.state;

    const newSelectedState = {};
    items.forEach((file) => {
      newSelectedState[file.id] = selectedState[file.id] || false;
    });

    const selectedCount = getSelectedIds(newSelectedState).length;
    const totalCount = items.length;

    this.setState({
      selectedState: newSelectedState,
      allSelected: selectedCount === totalCount,
      allUnselected: selectedCount === 0
    });
  }

  onSelectAllChange = ({ value }) => {
    this.setState(selectAll(this.state.selectedState, value));
  };

  onSelectAllPress = () => {
    this.onSelectAllChange({ value: !this.state.allSelected });
  };

  onSelectedChange = ({ id, value, shiftKey = false }) => {
    this.setState((state) =>
      toggleSelected(state, this.props.items, id, value, shiftKey)
    );
  };

  onDeleteUnmappedFilesPress = () => {
    const selectedIds = this.getSelectedIds();
    this.props.deleteUnmappedFiles(selectedIds);
  };

  rowRenderer = ({ key, rowIndex, style }) => {
    const { columns, deleteUnmappedFile } = this.props;
    const { selectedState } = this.state;
    const item = this.getSortedItems()[rowIndex];

    return (
      <VirtualTableRow key={key} style={style}>
        <UnmappedFilesTableRow
          key={item.id}
          columns={columns}
          isSelected={selectedState[item.id]}
          onSelectedChange={this.onSelectedChange}
          deleteUnmappedFile={deleteUnmappedFile}
          {...item}
        />
      </VirtualTableRow>
    );
  };

  render() {
    const {
      isFetching,
      isPopulated,
      isDeleting,
      error,
      items,
      columns,
      onTableOptionChange,
      isScanningFolders,
      isCleaningUnmappedFiles,
      onAddScenesPress,
      onCleanUnmappedFilesPress,
      deleteUnmappedFiles,
      ...otherProps
    } = this.props;

    const {
      allSelected,
      allUnselected,
      selectedState,
      sortKey,
      sortDirection
    } = this.state;

    const selectedTrackFileIds = this.getSelectedIds();
    const sortedItems = this.getSortedItems();

    return (
      <PageContent title={translate('UnmappedFiles')}>
        <PageToolbar>
          <PageToolbarSection>
            <PageToolbarButton
              label={translate('AddScenes')}
              iconName={icons.ADD}
              isSpinning={isScanningFolders}
              onPress={onAddScenesPress}
            />
            <PageToolbarButton
              label={translate('CleanUnmappedFiles')}
              iconName={icons.CLEAN}
              isSpinning={isCleaningUnmappedFiles}
              onPress={onCleanUnmappedFilesPress}
            />
            <PageToolbarButton
              label={translate('DeleteSelected')}
              iconName={icons.DELETE}
              isDisabled={selectedTrackFileIds.length === 0}
              isSpinning={isDeleting}
              onPress={this.onDeleteUnmappedFilesPress}
            />
          </PageToolbarSection>

          <PageToolbarSection alignContent={align.RIGHT}>
            <TableOptionsModalWrapper
              {...otherProps}
              columns={columns}
              onTableOptionChange={onTableOptionChange}
            >
              <PageToolbarButton
                label={translate('Options')}
                iconName={icons.TABLE}
              />
            </TableOptionsModalWrapper>
          </PageToolbarSection>
        </PageToolbar>

        <PageContentBody ref={this.scrollerRef}>
          {isFetching && !isPopulated && <LoadingIndicator />}
          {isPopulated && !error && !items.length && (
            <Alert kind={kinds.INFO}>
              Success! My work is done, all files on disk are matched to known
              scenes.
            </Alert>
          )}

          {isPopulated &&
          !error &&
          !!items.length &&
          this.scrollerRef.current ? (
            <VirtualTable
              header={
                <UnmappedFilesTableHeader
                  columns={columns}
                  sortKey={sortKey}
                  sortDirection={sortDirection}
                  onSortPress={this.onSortPress}
                  onTableOptionChange={onTableOptionChange}
                  allSelected={allSelected}
                  allUnselected={allUnselected}
                  onSelectAllChange={this.onSelectAllChange}
                />
                }
              items={sortedItems}
              columns={columns}
              scroller={this.scrollerRef.current}
              isSmallScreen={false}
              overscanRowCount={10}
              rowRenderer={this.rowRenderer}
              selectedState={selectedState}
            />
            ) : null}
        </PageContentBody>
      </PageContent>
    );
  }
}

UnmappedFilesTable.propTypes = {
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  isDeleting: PropTypes.bool.isRequired,
  deleteError: PropTypes.object,
  error: PropTypes.object,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  columns: PropTypes.arrayOf(PropTypes.object).isRequired,
  onTableOptionChange: PropTypes.func.isRequired,
  deleteUnmappedFile: PropTypes.func.isRequired,
  deleteUnmappedFiles: PropTypes.func.isRequired,
  isScanningFolders: PropTypes.bool.isRequired,
  isCleaningUnmappedFiles: PropTypes.bool.isRequired,
  onAddScenesPress: PropTypes.func.isRequired,
  onCleanUnmappedFilesPress: PropTypes.func.isRequired
};

export default UnmappedFilesTable;
