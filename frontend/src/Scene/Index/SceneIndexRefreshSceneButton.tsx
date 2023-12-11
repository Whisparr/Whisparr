import React, { useCallback, useMemo } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { useSelect } from 'App/SelectContext';
import ClientSideCollectionAppState from 'App/State/ClientSideCollectionAppState';
import MoviesAppState, { MovieIndexAppState } from 'App/State/MoviesAppState';
import { REFRESH_MOVIE } from 'Commands/commandNames';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
import { icons } from 'Helpers/Props';
import { executeCommand } from 'Store/Actions/commandActions';
import createCommandExecutingSelector from 'Store/Selectors/createCommandExecutingSelector';
import createMovieClientSideCollectionItemsSelector from 'Store/Selectors/createMovieClientSideCollectionItemsSelector';
import translate from 'Utilities/String/translate';
import getSelectedIds from 'Utilities/Table/getSelectedIds';

interface SceneIndexRefreshSceneButtonProps {
  isSelectMode: boolean;
  selectedFilterKey: string;
}

function SceneIndexRefreshSceneButton(
  props: SceneIndexRefreshSceneButtonProps
) {
  const isRefreshing = useSelector(
    createCommandExecutingSelector(REFRESH_MOVIE)
  );
  const {
    items,
    totalItems,
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

  const scenesToRefresh =
    isSelectMode && selectedSceneIds.length > 0
      ? selectedSceneIds
      : items.map((m) => m.id);

  const refreshIndexLabel =
    selectedFilterKey === 'all'
      ? translate('UpdateAll')
      : translate('UpdateFiltered');

  const refreshSelectLabel =
    selectedSceneIds.length > 0
      ? translate('UpdateSelected')
      : translate('UpdateAll');

  const onPress = useCallback(() => {
    dispatch(
      executeCommand({
        name: REFRESH_MOVIE,
        movieIds: scenesToRefresh,
      })
    );
  }, [dispatch, scenesToRefresh]);

  return (
    <PageToolbarButton
      label={isSelectMode ? refreshSelectLabel : refreshIndexLabel}
      isSpinning={isRefreshing}
      isDisabled={!totalItems}
      iconName={icons.REFRESH}
      onPress={onPress}
    />
  );
}

export default SceneIndexRefreshSceneButton;
