import React, { useCallback, useEffect, useMemo, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import { useSelect } from 'App/SelectContext';
import AppState from 'App/State/AppState';
import SpinnerButton from 'Components/Link/SpinnerButton';
import PageContentFooter from 'Components/Page/PageContentFooter';
import { fetchRootFolders } from 'Store/Actions/rootFolderActions';
import { saveStudios } from 'Store/Actions/studioActions';
import translate from 'Utilities/String/translate';
import getSelectedIds from 'Utilities/Table/getSelectedIds';
import EditStudiosModal from './Edit/EditStudiosModal';
import TagsModal from './Tags/TagsModal';
import styles from './StudioIndexSelectFooter.css';

interface SavePayload {
  monitored?: boolean;
  qualityProfileId?: number;
  rootFolderPath?: string;
  moveFiles?: boolean;
}

const sceneEditorSelector = createSelector(
  (state: AppState) => state.studios,
  (studios) => {
    const { isSaving } = studios;

    return {
      isSaving,
    };
  }
);

function StudioIndexSelectFooter() {
  const { isSaving } = useSelector(sceneEditorSelector);

  const dispatch = useDispatch();

  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [isTagsModalOpen, setIsTagsModalOpen] = useState(false);
  const [isSavingStudios, setIsSavingStudios] = useState(false);
  const [isSavingTags, setIsSavingTags] = useState(false);

  const [selectState] = useSelect();
  const { selectedState } = selectState;

  const studioIds = useMemo(() => {
    return getSelectedIds(selectedState);
  }, [selectedState]);

  const selectedCount = studioIds.length ? studioIds.length : 0;

  const onEditPress = useCallback(() => {
    setIsEditModalOpen(true);
  }, [setIsEditModalOpen]);

  const onEditModalClose = useCallback(() => {
    setIsEditModalOpen(false);
  }, [setIsEditModalOpen]);

  const onSavePress = useCallback(
    (payload: SavePayload) => {
      setIsSavingStudios(true);
      setIsEditModalOpen(false);

      dispatch(
        saveStudios({
          ...payload,
          studioIds,
        })
      );
    },
    [studioIds, dispatch]
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
        saveStudios({
          studioIds,
          tags,
          applyTags,
        })
      );
    },
    [studioIds, dispatch]
  );

  useEffect(() => {
    if (!isSaving) {
      setIsSavingStudios(false);
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
            isSpinning={isSaving && isSavingStudios}
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
        {translate('StudiosSelectedInterp', { count: selectedCount })}
      </div>

      <EditStudiosModal
        isOpen={isEditModalOpen}
        studioIds={studioIds}
        onSavePress={onSavePress}
        onModalClose={onEditModalClose}
      />

      <TagsModal
        isOpen={isTagsModalOpen}
        studioIds={studioIds}
        onApplyTagsPress={onApplyTagsPress}
        onModalClose={onTagsModalClose}
      />
    </PageContentFooter>
  );
}

export default StudioIndexSelectFooter;
