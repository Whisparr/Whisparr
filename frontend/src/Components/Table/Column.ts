import React from 'react';
import { all as SortDirection } from 'Helpers/Props/sortDirections';

type PropertyFunction<T> = () => T;

// TODO: Convert to generic so `name` can be a type
interface Column {
  name: string;
  label: string | PropertyFunction<string> | React.ReactNode;
  className?: string;
  columnLabel?: string;
  isSortable?: boolean;
  // Accept either a single sort direction string or the full set
  // (some places pass `sortDirections.ASCENDING` which is a string)
  fixedSortDirection?: string | typeof SortDirection;
  isVisible: boolean;
  isModifiable?: boolean;
}

export default Column;
