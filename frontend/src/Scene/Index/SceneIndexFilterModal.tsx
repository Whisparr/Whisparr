import React, { useCallback } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';
import FilterModal from 'Components/Filter/FilterModal';
import { setSceneFilter } from 'Store/Actions/sceneIndexActions';

function createSceneSelector() {
  return createSelector(
    (state: AppState) => state.movies.items,
    (scenes) => {
      return scenes;
    }
  );
}

function createFilterBuilderPropsSelector() {
  return createSelector(
    (state: AppState) => state.sceneIndex.filterBuilderProps,
    (filterBuilderProps) => {
      return filterBuilderProps;
    }
  );
}

interface SceneIndexFilterModalProps {
  isOpen: boolean;
}

export default function SceneIndexFilterModal(
  props: SceneIndexFilterModalProps
) {
  const sectionItems = useSelector(createSceneSelector());
  const filterBuilderProps = useSelector(createFilterBuilderPropsSelector());
  const customFilterType = 'sceneIndex';

  const dispatch = useDispatch();

  const dispatchSetFilter = useCallback(
    (payload: unknown) => {
      dispatch(setSceneFilter(payload));
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
