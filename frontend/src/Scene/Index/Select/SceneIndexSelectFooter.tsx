import React, { useCallback, useEffect, useMemo, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import { useSelect } from 'App/SelectContext';
import AppState from 'App/State/AppState';
import { RENAME_MOVIE } from 'Commands/commandNames';
import SpinnerButton from 'Components/Link/SpinnerButton';
import PageContentFooter from 'Components/Page/PageContentFooter';
import usePrevious from 'Helpers/Hooks/usePrevious';
import { kinds } from 'Helpers/Props';
import { saveMovieEditor } from 'Store/Actions/movieActions';
import { fetchRootFolders } from 'Store/Actions/rootFolderActions';
import createCommandExecutingSelector from 'Store/Selectors/createCommandExecutingSelector';
import translate from 'Utilities/String/translate';
import getSelectedIds from 'Utilities/Table/getSelectedIds';
import DeleteSceneModal from './Delete/DeleteSceneModal';
import EditScenesModal from './Edit/EditScenesModal';
import OrganizeScenesModal from './Organize/OrganizeScenesModal';
import TagsModal from './Tags/TagsModal';
import styles from './SceneIndexSelectFooter.css';

interface SavePayload {
  monitored?: boolean;
  qualityProfileId?: number;
  rootFolderPath?: string;
  moveFiles?: boolean;
}

const sceneEditorSelector = createSelector(
  (state: AppState) => state.movies,
  (scenes) => {
    const { isSaving, isDeleting, deleteError } = scenes;

    return {
      isSaving,
      isDeleting,
      deleteError,
    };
  }
);

function SceneIndexSelectFooter() {
  const { isSaving, isDeleting, deleteError } =
    useSelector(sceneEditorSelector);

  const isOrganizingScenes = useSelector(
    createCommandExecutingSelector(RENAME_MOVIE)
  );

  const dispatch = useDispatch();

  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [isOrganizeModalOpen, setIsOrganizeModalOpen] = useState(false);
  const [isTagsModalOpen, setIsTagsModalOpen] = useState(false);
  const [isDeleteModalOpen, setIsDeleteModalOpen] = useState(false);
  const [isSavingScenes, setIsSavingScenes] = useState(false);
  const [isSavingTags, setIsSavingTags] = useState(false);
  const previousIsDeleting = usePrevious(isDeleting);

  const [selectState, selectDispatch] = useSelect();
  const { selectedState } = selectState;

  const sceneIds = useMemo(() => {
    return getSelectedIds(selectedState);
  }, [selectedState]);

  const selectedCount = sceneIds.length ? sceneIds.length : 0;

  const onEditPress = useCallback(() => {
    setIsEditModalOpen(true);
  }, [setIsEditModalOpen]);

  const onEditModalClose = useCallback(() => {
    setIsEditModalOpen(false);
  }, [setIsEditModalOpen]);

  const onSavePress = useCallback(
    (payload: SavePayload) => {
      setIsSavingScenes(true);
      setIsEditModalOpen(false);

      dispatch(
        saveMovieEditor({
          ...payload,
          movieIds: sceneIds,
        })
      );
    },
    [sceneIds, dispatch]
  );

  const onOrganizePress = useCallback(() => {
    setIsOrganizeModalOpen(true);
  }, [setIsOrganizeModalOpen]);

  const onOrganizeModalClose = useCallback(() => {
    setIsOrganizeModalOpen(false);
  }, [setIsOrganizeModalOpen]);

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
        saveMovieEditor({
          movieIds: sceneIds,
          tags,
          applyTags,
        })
      );
    },
    [sceneIds, dispatch]
  );

  const onDeletePress = useCallback(() => {
    setIsDeleteModalOpen(true);
  }, [setIsDeleteModalOpen]);

  const onDeleteModalClose = useCallback(() => {
    setIsDeleteModalOpen(false);
  }, []);

  useEffect(() => {
    if (!isSaving) {
      setIsSavingScenes(false);
      setIsSavingTags(false);
    }
  }, [isSaving]);

  useEffect(() => {
    if (previousIsDeleting && !isDeleting && !deleteError) {
      selectDispatch({ type: 'unselectAll' });
    }
  }, [previousIsDeleting, isDeleting, deleteError, selectDispatch]);

  useEffect(() => {
    dispatch(fetchRootFolders());
  }, [dispatch]);

  const anySelected = selectedCount > 0;

  return (
    <PageContentFooter className={styles.footer}>
      <div className={styles.buttons}>
        <div className={styles.actionButtons}>
          <SpinnerButton
            isSpinning={isSaving && isSavingScenes}
            isDisabled={!anySelected || isOrganizingScenes}
            onPress={onEditPress}
          >
            {translate('Edit')}
          </SpinnerButton>

          <SpinnerButton
            kind={kinds.WARNING}
            isSpinning={isOrganizingScenes}
            isDisabled={!anySelected || isOrganizingScenes}
            onPress={onOrganizePress}
          >
            {translate('RenameFiles')}
          </SpinnerButton>

          <SpinnerButton
            isSpinning={isSaving && isSavingTags}
            isDisabled={!anySelected || isOrganizingScenes}
            onPress={onTagsPress}
          >
            {translate('SetTags')}
          </SpinnerButton>
        </div>

        <div className={styles.deleteButtons}>
          <SpinnerButton
            kind={kinds.DANGER}
            isSpinning={isDeleting}
            isDisabled={!anySelected || isDeleting}
            onPress={onDeletePress}
          >
            {translate('Delete')}
          </SpinnerButton>
        </div>
      </div>

      <div className={styles.selected}>
        {translate('ScenesSelectedInterp', { count: selectedCount })}
      </div>

      <EditScenesModal
        isOpen={isEditModalOpen}
        sceneIds={sceneIds}
        onSavePress={onSavePress}
        onModalClose={onEditModalClose}
      />

      <TagsModal
        isOpen={isTagsModalOpen}
        sceneIds={sceneIds}
        onApplyTagsPress={onApplyTagsPress}
        onModalClose={onTagsModalClose}
      />

      <OrganizeScenesModal
        isOpen={isOrganizeModalOpen}
        sceneIds={sceneIds}
        onModalClose={onOrganizeModalClose}
      />

      <DeleteSceneModal
        isOpen={isDeleteModalOpen}
        sceneIds={sceneIds}
        onModalClose={onDeleteModalClose}
      />
    </PageContentFooter>
  );
}

export default SceneIndexSelectFooter;
