import React from 'react';
import { CustomFilter, Filter } from 'App/State/AppState';
import FilterMenu from 'Components/Menu/FilterMenu';
import { align } from 'Helpers/Props';
import PerformerIndexFilterModal from 'Performer/Index/PerformerIndexFilterModal';

interface PerformerIndexFilterMenuProps {
  selectedFilterKey: string | number;
  filters: Filter[];
  customFilters: CustomFilter[];
  isDisabled: boolean;
  onFilterSelect: (filter: number | string) => void;
}

function PerformerIndexFilterMenu(props: PerformerIndexFilterMenuProps) {
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
      filterModalConnectorComponent={PerformerIndexFilterModal}
      onFilterSelect={onFilterSelect}
    />
  );
}

PerformerIndexFilterMenu.defaultProps = {
  showCustomFilters: false,
};

export default PerformerIndexFilterMenu;
