import React, { useCallback } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';
import FilterModal from 'Components/Filter/FilterModal';
import { setStudioFilter } from 'Store/Actions/studioActions';

function createStudioSelector() {
  return createSelector(
    (state: AppState) => state.studios.items,
    (studios) => {
      return studios;
    }
  );
}

function createFilterBuilderPropsSelector() {
  return createSelector(
    (state: AppState) => state.studios.filterBuilderProps,
    (filterBuilderProps) => {
      return filterBuilderProps;
    }
  );
}

interface StudioIndexFilterModalProps {
  isOpen: boolean;
}

export default function StudioIndexFilterModal(
  props: StudioIndexFilterModalProps
) {
  const sectionItems = useSelector(createStudioSelector());
  const filterBuilderProps = useSelector(createFilterBuilderPropsSelector());
  const customFilterType = 'studios';

  const dispatch = useDispatch();

  const dispatchSetFilter = useCallback(
    (payload: unknown) => {
      dispatch(setStudioFilter(payload));
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
