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
import { align, icons } from 'Helpers/Props';
import hasDifferentItemsOrOrder from 'Utilities/Object/hasDifferentItemsOrOrder';
import translate from 'Utilities/String/translate';
import getSelectedIds from 'Utilities/Table/getSelectedIds';
import selectAll from 'Utilities/Table/selectAll';
import toggleSelected from 'Utilities/Table/toggleSelected';
import UnmappedFilesTableHeader from './UnmappedFilesTableHeader';
import UnmappedFilesTableRow from './UnmappedFilesTableRow';
import styles from './UnmappedFilesTable.css';

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
    const { items, isDeleting, deleteError, isScanningFolders } = this.props;

    if (hasDifferentItemsOrOrder(prevProps.items, items)) {
      this.setSelectedState();
    }

    const hasFinishedDeleting = prevProps.isDeleting && !isDeleting && !deleteError;

    if (hasFinishedDeleting) {
      this.onSelectAllChange({ value: false });
    }

    // If a scan was running and just finished, refresh the unmapped files list
    const hasFinishedScanning = prevProps.isScanningFolders && !isScanningFolders;
    if (hasFinishedScanning && typeof this.props.fetchUnmappedFiles === 'function') {
      this.props.fetchUnmappedFiles();
    }
  }

  onSortColumnPress = (column) => {
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
              label={translate('ImportScenes')}
              title={translate('ImportScenesTooltip')}
              iconName={icons.ADD}
              isSpinning={isScanningFolders}
              onPress={onAddScenesPress}
            />
            <PageToolbarButton
              label={translate('CleanUnmappedFiles')}
              title={translate('CleanUnmappedFilesTooltip')}
              iconName={icons.CLEAN}
              isSpinning={isCleaningUnmappedFiles}
              onPress={onCleanUnmappedFilesPress}
              isDisabled={!isPopulated || items.length === 0}
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
            <div>
              <Alert kind="success">
                <div id="AllScannedItemsMapped">{translate('AllScannedItemsMapped')}</div>
              </Alert>
              <Alert kind="info">
                <div className={styles.sceneImportHaveMore}>{translate('SceneImportHaveMore')}</div>
                <div className={styles.sceneImportStep}>{translate('SceneImportStep1')}</div>
                <div className={styles.sceneImportStep}>{translate('SceneImportStep2')}</div>
                <div className={styles.sceneImportStep}>{translate('SceneImportStep3')}</div>
                <div className={styles.sceneImportStep}>{translate('SceneImportStep4')}</div>
                <div className={styles.sceneImportNote} >{translate('SceneImportNote')}</div>
              </Alert>
              <div>
                <Alert kind="info">
                  <div className={styles.folderStructureHeading} >
                    {translate('YourFolderStructureShouldLookLikeThis')}:
                  </div>
                  <code className={styles.folderStructure}>
                    {`${translate('RootFolder')}\n`}
                    {`├─ ${translate('SceneImportImportDropYourScenesFilesHere')}\n`}
                    {`├─ ${translate('SceneImportMovieFilesWillBeHere')}\n`}
                    {`└─ ${translate('SceneImportSceneFilesWillBeHere')}`}
                  </code>
                </Alert>
              </div>
            </div>
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
                  onSortPress={this.onSortColumnPress}
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
  fetchUnmappedFiles: PropTypes.func.isRequired,
  deleteUnmappedFile: PropTypes.func.isRequired,
  deleteUnmappedFiles: PropTypes.func.isRequired,
  isScanningFolders: PropTypes.bool.isRequired,
  isCleaningUnmappedFiles: PropTypes.bool.isRequired,
  onAddScenesPress: PropTypes.func.isRequired,
  onCleanUnmappedFilesPress: PropTypes.func.isRequired,
  onSortColumnPress: PropTypes.func.isRequired
};

export default UnmappedFilesTable;
