import React, { useCallback, useMemo } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { useSelect } from 'App/SelectContext';
import ClientSideCollectionAppState from 'App/State/ClientSideCollectionAppState';
import MoviesAppState, { MovieIndexAppState } from 'App/State/MoviesAppState';
import { MOVIE_SEARCH } from 'Commands/commandNames';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
import { icons } from 'Helpers/Props';
import { executeCommand } from 'Store/Actions/commandActions';
import createCommandExecutingSelector from 'Store/Selectors/createCommandExecutingSelector';
import createMovieClientSideCollectionItemsSelector from 'Store/Selectors/createMovieClientSideCollectionItemsSelector';
import translate from 'Utilities/String/translate';
import getSelectedIds from 'Utilities/Table/getSelectedIds';

interface SceneIndexSearchButtonProps {
  isSelectMode: boolean;
  selectedFilterKey: string;
}

function SceneIndexSearchButton(props: SceneIndexSearchButtonProps) {
  const isSearching = useSelector(createCommandExecutingSelector(MOVIE_SEARCH));
  const {
    items,
  }: MoviesAppState & MovieIndexAppState & ClientSideCollectionAppState =
    useSelector(
      createMovieClientSideCollectionItemsSelector('sceneIndex', 'scene')
    );

  const dispatch = useDispatch();
  const { isSelectMode, selectedFilterKey } = props;
  const [selectState] = useSelect();
  const { selectedState } = selectState;

  const selectedSceneIds = useMemo(() => {
    return getSelectedIds(selectedState);
  }, [selectedState]);

  const scenesToSearch =
    isSelectMode && selectedSceneIds.length > 0
      ? selectedSceneIds
      : items.map((m) => m.id);

  const searchIndexLabel =
    selectedFilterKey === 'all'
      ? translate('SearchAll')
      : translate('SearchFiltered');

  const searchSelectLabel =
    selectedSceneIds.length > 0
      ? translate('SearchSelected')
      : translate('SearchAll');

  const onPress = useCallback(() => {
    dispatch(
      executeCommand({
        name: MOVIE_SEARCH,
        movieIds: scenesToSearch,
      })
    );
  }, [dispatch, scenesToSearch]);

  return (
    <PageToolbarButton
      label={isSelectMode ? searchSelectLabel : searchIndexLabel}
      isSpinning={isSearching}
      isDisabled={!items.length}
      iconName={icons.SEARCH}
      onPress={onPress}
    />
  );
}

export default SceneIndexSearchButton;
