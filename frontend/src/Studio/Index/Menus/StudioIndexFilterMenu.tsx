import React from 'react';
import { CustomFilter } from 'App/State/AppState';
import FilterMenu from 'Components/Menu/FilterMenu';
import { align } from 'Helpers/Props';
import StudioIndexFilterModal from 'Studio/Index/StudioIndexFilterModal';

interface StudioIndexFilterMenuProps {
  selectedFilterKey: string | number;
  filters: object[];
  customFilters: CustomFilter[];
  isDisabled: boolean;
  onFilterSelect(filterName: string): unknown;
}

function StudioIndexFilterMenu(props: StudioIndexFilterMenuProps) {
  const {
    selectedFilterKey,
    filters,
    customFilters,
    isDisabled,
    onFilterSelect,
  } = props;

  return (
    <FilterMenu
      alignMenu={align.RIGHT}
      isDisabled={isDisabled}
      selectedFilterKey={selectedFilterKey}
      filters={filters}
      customFilters={customFilters}
      filterModalConnectorComponent={StudioIndexFilterModal}
      onFilterSelect={onFilterSelect}
    />
  );
}

StudioIndexFilterMenu.defaultProps = {
  showCustomFilters: false,
};

export default StudioIndexFilterMenu;
