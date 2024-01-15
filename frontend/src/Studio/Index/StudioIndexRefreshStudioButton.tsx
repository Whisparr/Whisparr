import React, { useCallback, useMemo } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { useSelect } from 'App/SelectContext';
import ClientSideCollectionAppState from 'App/State/ClientSideCollectionAppState';
import StudiosAppState from 'App/State/StudiosAppState';
import { REFRESH_STUDIO } from 'Commands/commandNames';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
import { icons } from 'Helpers/Props';
import { executeCommand } from 'Store/Actions/commandActions';
import createCommandExecutingSelector from 'Store/Selectors/createCommandExecutingSelector';
import createStudioClientSideCollectionItemsSelector from 'Store/Selectors/createStudioClientSideCollectionItemsSelector';
import translate from 'Utilities/String/translate';
import getSelectedIds from 'Utilities/Table/getSelectedIds';

interface StudioIndexRefreshStudioButtonProps {
  isSelectMode: boolean;
  selectedFilterKey: string;
}

function StudioIndexRefreshStudioButton(
  props: StudioIndexRefreshStudioButtonProps
) {
  const isRefreshing = useSelector(
    createCommandExecutingSelector(REFRESH_STUDIO)
  );
  const { items, totalItems }: StudiosAppState & ClientSideCollectionAppState =
    useSelector(createStudioClientSideCollectionItemsSelector('studios'));

  const dispatch = useDispatch();
  const { isSelectMode, selectedFilterKey } = props;
  const [selectState] = useSelect();
  const { selectedState } = selectState;

  const selectedStudioIds = useMemo(() => {
    return getSelectedIds(selectedState);
  }, [selectedState]);

  const studiosToRefresh =
    isSelectMode && selectedStudioIds.length > 0
      ? selectedStudioIds
      : items.map((m) => m.id);

  const refreshIndexLabel =
    selectedFilterKey === 'all'
      ? translate('UpdateAll')
      : translate('UpdateFiltered');

  const refreshSelectLabel =
    selectedStudioIds.length > 0
      ? translate('UpdateSelected')
      : translate('UpdateAll');

  const onPress = useCallback(() => {
    dispatch(
      executeCommand({
        name: REFRESH_STUDIO,
        studioIds: studiosToRefresh,
      })
    );
  }, [dispatch, studiosToRefresh]);

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

export default StudioIndexRefreshStudioButton;
