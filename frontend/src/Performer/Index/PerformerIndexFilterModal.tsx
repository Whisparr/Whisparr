import React, { useCallback } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';
import FilterModal from 'Components/Filter/FilterModal';
import { setPerformerFilter } from 'Store/Actions/performerActions';

function createPerformerSelector() {
  return createSelector(
    (state: AppState) => state.performers.items,
    (performers) => {
      // Ensure we always return an array: if items is an object map, convert to values,
      // if it's already an array return it, otherwise return an empty array.
      if (Array.isArray(performers)) return performers;
      if (performers && typeof performers === 'object')
        return Object.values(performers);
      return [];
    }
  );
}

function createFilterBuilderPropsSelector() {
  return createSelector(
    (state: AppState) => state.performers.filterBuilderProps,
    (filterBuilderProps) => {
      return filterBuilderProps;
    }
  );
}

interface PerformerIndexFilterModalProps {
  isOpen: boolean;
}

export default function PerformerIndexFilterModal(
  props: PerformerIndexFilterModalProps
) {
  const sectionItems = useSelector(createPerformerSelector());
  const filterBuilderProps = useSelector(createFilterBuilderPropsSelector());
  const customFilterType = 'performers';

  const dispatch = useDispatch();

  const dispatchSetFilter = useCallback(
    (payload: unknown) => {
      dispatch(setPerformerFilter(payload));
    },
    [dispatch]
  );

  return (
    <FilterModal
      // TODO: Don't spread all the props
      {...props}
      sectionItems={sectionItems}
      filterBuilderProps={filterBuilderProps}
      customFilterType={customFilterType}
      dispatchSetFilter={dispatchSetFilter}
    />
  );
}
