import { orderBy } from 'lodash';
import React, { useCallback, useMemo, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import Button from 'Components/Link/Button';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { inputTypes, kinds } from 'Helpers/Props';
import Movie from 'Movie/Movie';
import { bulkDeleteMovie, setDeleteOption } from 'Store/Actions/movieActions';
import createAllScenesSelector from 'Store/Selectors/createAllScenesSelector';
import { CheckInputChanged } from 'typings/inputs';
import translate from 'Utilities/String/translate';
import styles from './DeleteSceneModalContent.css';

interface DeleteSceneModalContentProps {
  sceneIds: number[];
  onModalClose(): void;
}

const selectDeleteOptions = createSelector(
  (state: AppState) => state.movies.deleteOptions,
  (deleteOptions) => deleteOptions
);

function DeleteSceneModalContent(props: DeleteSceneModalContentProps) {
  const { sceneIds, onModalClose } = props;

  const { addImportExclusion } = useSelector(selectDeleteOptions);
  const allScenes: Movie[] = useSelector(createAllScenesSelector());
  const dispatch = useDispatch();

  const [deleteFiles, setDeleteFiles] = useState(false);

  const scenes = useMemo((): Movie[] => {
    const scenes = sceneIds.map((id) => {
      return allScenes.find((s) => s.id === id);
    }) as Movie[];

    return orderBy(scenes, ['sortTitle']);
  }, [sceneIds, allScenes]);

  const onDeleteFilesChange = useCallback(
    ({ value }: CheckInputChanged) => {
      setDeleteFiles(value);
    },
    [setDeleteFiles]
  );

  const onDeleteOptionChange = useCallback(
    ({ name, value }: { name: string; value: boolean }) => {
      dispatch(
        setDeleteOption({
          [name]: value,
        })
      );
    },
    [dispatch]
  );

  const onDeleteScenesConfirmed = useCallback(() => {
    setDeleteFiles(false);

    dispatch(
      bulkDeleteMovie({
        movieIds: sceneIds,
        deleteFiles,
        addImportExclusion,
      })
    );

    onModalClose();
  }, [
    sceneIds,
    deleteFiles,
    addImportExclusion,
    setDeleteFiles,
    dispatch,
    onModalClose,
  ]);

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>{translate('DeleteSelectedScene')}</ModalHeader>

      <ModalBody>
        <div>
          <FormGroup>
            <FormLabel>{translate('AddListExclusion')}</FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="addImportExclusion"
              value={addImportExclusion}
              helpText={translate('AddImportExclusionHelpText')}
              onChange={onDeleteOptionChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>{`Delete Scene Folder${
              scenes.length > 1 ? 's' : ''
            }`}</FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="deleteFiles"
              value={deleteFiles}
              helpText={`Delete Scene Folder${
                scenes.length > 1 ? 's' : ''
              } and all contents`}
              kind={kinds.DANGER}
              onChange={onDeleteFilesChange}
            />
          </FormGroup>
        </div>

        <div className={styles.message}>
          {`Are you sure you want to delete ${scenes.length} selected scene(s)${
            deleteFiles ? ' and all contents' : ''
          }?`}
        </div>

        <ul>
          {scenes.map((s) => {
            return (
              <li key={s.title}>
                <span>{s.title}</span>

                {deleteFiles && (
                  <span className={styles.pathContainer}>
                    -<span className={styles.path}>{s.path}</span>
                  </span>
                )}
              </li>
            );
          })}
        </ul>
      </ModalBody>

      <ModalFooter>
        <Button onPress={onModalClose}>{translate('Cancel')}</Button>

        <Button kind={kinds.DANGER} onPress={onDeleteScenesConfirmed}>
          {translate('Delete')}
        </Button>
      </ModalFooter>
    </ModalContent>
  );
}

export default DeleteSceneModalContent;
