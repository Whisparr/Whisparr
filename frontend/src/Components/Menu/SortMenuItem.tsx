import React from 'react';
import { icons, sortDirections } from 'Helpers/Props';
import SelectedMenuItem from './SelectedMenuItem';

interface SortMenuItemProps {
  name?: string;
  sortKey?: string;
  sortDirection?: sortDirections;
  children: string | React.ReactNode;
  onPress: (sortKey: string) => void;
}

function SortMenuItem({
  name,
  sortKey,
  sortDirection,
  ...otherProps
}: SortMenuItemProps) {
  const isSelected = name === sortKey;

  return (
    <SelectedMenuItem
      name={name}
      selectedIconName={
        sortDirection === 'ascending'
          ? icons.SORT_ASCENDING
          : icons.SORT_DESCENDING
      }
      isSelected={isSelected}
      {...otherProps}
    />
  );
}

export default SortMenuItem;
