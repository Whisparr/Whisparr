import React from 'react';
import MenuContent from 'Components/Menu/MenuContent';
import SortMenu from 'Components/Menu/SortMenu';
import SortMenuItem from 'Components/Menu/SortMenuItem';
import { align } from 'Helpers/Props';
import SortDirection from 'Helpers/Props/SortDirection';
import translate from 'Utilities/String/translate';

interface PerformerIndexSortMenuProps {
  sortKey?: string;
  sortDirection?: SortDirection;
  isDisabled: boolean;
  onSortSelect(sortKey: string): unknown;
}

function PerformerIndexSortMenu(props: PerformerIndexSortMenuProps) {
  const { sortKey, sortDirection, isDisabled, onSortSelect } = props;

  return (
    <SortMenu isDisabled={isDisabled} alignMenu={align.RIGHT}>
      <MenuContent>
        <SortMenuItem
          name="gender"
          sortKey={sortKey}
          sortDirection={sortDirection}
          onPress={onSortSelect}
        >
          {translate('Gender')}
        </SortMenuItem>
      </MenuContent>
    </SortMenu>
  );
}

export default PerformerIndexSortMenu;
