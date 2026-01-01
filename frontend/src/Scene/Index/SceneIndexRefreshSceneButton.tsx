import React, { useCallback, useMemo } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { useSelect } from 'App/SelectContext';
import { MovieIndexAppState } from 'App/State/MoviesAppState';
import ScenePagesAppState from 'App/State/ScenePagesAppState';
import { REFRESH_MOVIE } from 'Commands/commandNames';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
import { icons } from 'Helpers/Props';
import { executeCommand } from 'Store/Actions/commandActions';
import createCommandExecutingSelector from 'Store/Selectors/createCommandExecutingSelector';
import createScenePagedCollectionItemsSelector from 'Store/Selectors/createScenePagedCollectionItemsSelector';
import translate from 'Utilities/String/translate';
import getSelectedIds from 'Utilities/Table/getSelectedIds';

interface SceneIndexRefreshSceneButtonProps {
  isSelectMode: boolean;
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
    selectedFilterKey,
  }: ScenePagesAppState & MovieIndexAppState & { totalItems: number } =
    useSelector(createScenePagedCollectionItemsSelector);

  const dispatch = useDispatch();
  const { isSelectMode } = props;
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
