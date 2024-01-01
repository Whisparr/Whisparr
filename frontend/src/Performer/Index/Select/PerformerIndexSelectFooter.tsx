import React, { useCallback, useEffect, useMemo, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import { useSelect } from 'App/SelectContext';
import AppState from 'App/State/AppState';
import SpinnerButton from 'Components/Link/SpinnerButton';
import PageContentFooter from 'Components/Page/PageContentFooter';
import { savePerformers } from 'Store/Actions/performerActions';
import { fetchRootFolders } from 'Store/Actions/rootFolderActions';
import translate from 'Utilities/String/translate';
import getSelectedIds from 'Utilities/Table/getSelectedIds';
import EditPerformersModal from './Edit/EditPerformersModal';
import TagsModal from './Tags/TagsModal';
import styles from './PerformerIndexSelectFooter.css';

interface SavePayload {
  monitored?: boolean;
  qualityProfileId?: number;
  rootFolderPath?: string;
  moveFiles?: boolean;
}

const sceneEditorSelector = createSelector(
  (state: AppState) => state.performers,
  (performers) => {
    const { isSaving } = performers;

    return {
      isSaving,
    };
  }
);

function PerformerIndexSelectFooter() {
  const { isSaving } = useSelector(sceneEditorSelector);

  const dispatch = useDispatch();

  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [isTagsModalOpen, setIsTagsModalOpen] = useState(false);
  const [isSavingPerformers, setIsSavingPerformers] = useState(false);
  const [isSavingTags, setIsSavingTags] = useState(false);

  const [selectState] = useSelect();
  const { selectedState } = selectState;

  const performerIds = useMemo(() => {
    return getSelectedIds(selectedState);
  }, [selectedState]);

  const selectedCount = performerIds.length ? performerIds.length : 0;

  const onEditPress = useCallback(() => {
    setIsEditModalOpen(true);
  }, [setIsEditModalOpen]);

  const onEditModalClose = useCallback(() => {
    setIsEditModalOpen(false);
  }, [setIsEditModalOpen]);

  const onSavePress = useCallback(
    (payload: SavePayload) => {
      setIsSavingPerformers(true);
      setIsEditModalOpen(false);

      dispatch(
        savePerformers({
          ...payload,
          performerIds,
        })
      );
    },
    [performerIds, dispatch]
  );

  const onTagsPress = useCallback(() => {
    setIsTagsModalOpen(true);
  }, [setIsTagsModalOpen]);

  const onTagsModalClose = useCallback(() => {
    setIsTagsModalOpen(false);
  }, [setIsTagsModalOpen]);

  const onApplyTagsPress = useCallback(
    (tags: number[], applyTags: string) => {
      setIsSavingTags(true);
      setIsTagsModalOpen(false);

      dispatch(
        savePerformers({
          performerIds,
          tags,
          applyTags,
        })
      );
    },
    [performerIds, dispatch]
  );

  useEffect(() => {
    if (!isSaving) {
      setIsSavingPerformers(false);
      setIsSavingTags(false);
    }
  }, [isSaving]);

  useEffect(() => {
    dispatch(fetchRootFolders());
  }, [dispatch]);

  const anySelected = selectedCount > 0;

  return (
    <PageContentFooter className={styles.footer}>
      <div className={styles.buttons}>
        <div className={styles.actionButtons}>
          <SpinnerButton
            isSpinning={isSaving && isSavingPerformers}
            isDisabled={!anySelected}
            onPress={onEditPress}
          >
            {translate('Edit')}
          </SpinnerButton>

          <SpinnerButton
            isSpinning={isSaving && isSavingTags}
            isDisabled={!anySelected}
            onPress={onTagsPress}
          >
            {translate('SetTags')}
          </SpinnerButton>
        </div>
      </div>

      <div className={styles.selected}>
        {translate('PerformersSelectedInterp', { count: selectedCount })}
      </div>

      <EditPerformersModal
        isOpen={isEditModalOpen}
        performerIds={performerIds}
        onSavePress={onSavePress}
        onModalClose={onEditModalClose}
      />

      <TagsModal
        isOpen={isTagsModalOpen}
        performerIds={performerIds}
        onApplyTagsPress={onApplyTagsPress}
        onModalClose={onTagsModalClose}
      />
    </PageContentFooter>
  );
}

export default PerformerIndexSelectFooter;
