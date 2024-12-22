import React from 'react';
import { CustomFilter, Filter } from 'App/State/AppState';
import FilterMenu from 'Components/Menu/FilterMenu';
import { align } from 'Helpers/Props';
import SceneIndexFilterModal from 'Scene/Index/SceneIndexFilterModal';

interface SceneIndexFilterMenuProps {
  selectedFilterKey: string | number;
  filters: Filter[];
  customFilters: CustomFilter[];
  isDisabled: boolean;
  onFilterSelect: (filter: number | string) => void;
}

function SceneIndexFilterMenu(props: SceneIndexFilterMenuProps) {
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
      filterModalConnectorComponent={SceneIndexFilterModal}
      onFilterSelect={onFilterSelect}
    />
  );
}

SceneIndexFilterMenu.defaultProps = {
  showCustomFilters: false,
};

export default SceneIndexFilterMenu;
