import React, { useCallback, useMemo } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { useSelect } from 'App/SelectContext';
import ClientSideCollectionAppState from 'App/State/ClientSideCollectionAppState';
import PerformersAppState from 'App/State/PerformersAppState';
import { REFRESH_PERFORMER } from 'Commands/commandNames';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
import { icons } from 'Helpers/Props';
import { executeCommand } from 'Store/Actions/commandActions';
import createCommandExecutingSelector from 'Store/Selectors/createCommandExecutingSelector';
import createPerformerClientSideCollectionItemsSelector from 'Store/Selectors/createPerformerClientSideCollectionItemsSelector';
import translate from 'Utilities/String/translate';
import getSelectedIds from 'Utilities/Table/getSelectedIds';

interface PerformerIndexRefreshPerformerButtonProps {
  isSelectMode: boolean;
  selectedFilterKey: string;
}

function PerformerIndexRefreshPerformerButton(
  props: PerformerIndexRefreshPerformerButtonProps
) {
  const isRefreshing = useSelector(
    createCommandExecutingSelector(REFRESH_PERFORMER)
  );
  const {
    items,
    totalItems,
  }: PerformersAppState & ClientSideCollectionAppState = useSelector(
    createPerformerClientSideCollectionItemsSelector('performers')
  );

  const dispatch = useDispatch();
  const { isSelectMode, selectedFilterKey } = props;
  const [selectState] = useSelect();
  const { selectedState } = selectState;

  const selectedPerformerIds = useMemo(() => {
    return getSelectedIds(selectedState);
  }, [selectedState]);

  const performersToRefresh =
    isSelectMode && selectedPerformerIds.length > 0
      ? selectedPerformerIds
      : items.map((m) => m.id);

  const refreshIndexLabel =
    selectedFilterKey === 'all'
      ? translate('UpdateAll')
      : translate('UpdateFiltered');

  const refreshSelectLabel =
    selectedPerformerIds.length > 0
      ? translate('UpdateSelected')
      : translate('UpdateAll');

  const onPress = useCallback(() => {
    dispatch(
      executeCommand({
        name: REFRESH_PERFORMER,
        performerIds: performersToRefresh,
      })
    );
  }, [dispatch, performersToRefresh]);

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

export default PerformerIndexRefreshPerformerButton;
