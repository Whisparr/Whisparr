import React, { useCallback, useEffect, useMemo, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import { CheckInputProps } from 'Components/Form/CheckInput';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputButton from 'Components/Form/FormInputButton';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import { QualityProfileSelectInputProps } from 'Components/Form/Select/QualityProfileSelectInput';
import { MovieTagInputProps } from 'Components/Form/Tag/MovieTagInput';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import SpinnerErrorButton from 'Components/Link/SpinnerErrorButton';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import usePrevious from 'Helpers/Hooks/usePrevious';
import { icons, inputTypes, kinds, sizes } from 'Helpers/Props';
import MoveMovieModal from 'Movie/MoveMovie/MoveMovieModal';
import useMovie from 'Movie/useMovie';
import { saveMovie, setMovieValue } from 'Store/Actions/movieActions';
import selectSettings from 'Store/Selectors/selectSettings';
import { InputChanged } from 'typings/inputs';
import { ValidationError, ValidationWarning } from 'typings/pending';
import translate from 'Utilities/String/translate';
import RootFolderModal from './RootFolder/RootFolderModal';
import { RootFolderUpdated } from './RootFolder/RootFolderModalContent';
import styles from './EditMovieModalContent.css';

export interface EditMovieModalContentProps {
  movieId: number;
  onModalClose: () => void;
  onDeleteMoviePress: () => void;
}

function EditMovieModalContent({
  movieId,
  onModalClose,
  onDeleteMoviePress,
}: EditMovieModalContentProps) {
  const dispatch = useDispatch();
  const {
    title,
    monitored,
    qualityProfileId,
    path,
    tags,
    rootFolderPath: initialRootFolderPath,
  } = useMovie(movieId)!;

  const { isSaving, saveError, pendingChanges } = useSelector(
    (state: AppState) => state.movies
  );

  const wasSaving = usePrevious(isSaving);

  const [isRootFolderModalOpen, setIsRootFolderModalOpen] = useState(false);

  const [rootFolderPath, setRootFolderPath] = useState(initialRootFolderPath);

  const isPathChanging = pendingChanges?.path && path !== pendingChanges.path;

  const [isConfirmMoveModalOpen, setIsConfirmMoveModalOpen] = useState(false);

  interface Setting {
    value?:
      | string
      | boolean
      | number
      | Array<string | number>
      | null
      | undefined;
    errors: Array<{
      message: string;
      link?: string;
      detailedMessage?: string;
    }>;
    warnings: Array<{
      message: string;
      link?: string;
      detailedMessage?: string;
    }>;
    previousValue?:
      | string
      | boolean
      | number
      | Array<string | number>
      | null
      | undefined;
    pending?: boolean;
  }

  type PathSetting = Setting & { value: string };

  interface SelectSettingsResult {
    settings: Record<string, Setting>;
    validationErrors: ValidationError[];
    validationWarnings: ValidationWarning[];
    hasPendingChanges: boolean;
    hasSettings: boolean;
    pendingChanges: unknown;
  }

  // Narrowly type the selector result to avoid using `any`.
  const memoResult = useMemo(() => {
    return selectSettings(
      {
        monitored,
        qualityProfileId,
        path,
        tags,
      },
      pendingChanges,
      saveError
    );
  }, [
    monitored,
    qualityProfileId,
    path,
    tags,
    pendingChanges,
    saveError,
  ]) as unknown as SelectSettingsResult;

  const { settings, ...otherSettings } = memoResult;

  const rawPathSetting = settings.path;

  const pathSetting = useMemo<PathSetting>(() => {
    if (!rawPathSetting) {
      return {
        value: '',
        errors: [],
        warnings: [],
      };
    }

    return {
      ...rawPathSetting,
      value:
        typeof rawPathSetting.value === 'string' ? rawPathSetting.value : '',
    };
  }, [rawPathSetting]);

  const handleInputChange = useCallback(
    ({ name, value }: InputChanged) => {
      // @ts-expect-error actions aren't typed
      dispatch(setMovieValue({ name, value }));
    },
    [dispatch]
  );

  const handleRootFolderPress = useCallback(() => {
    setIsRootFolderModalOpen(true);
  }, []);

  const handleRootFolderModalClose = useCallback(() => {
    setIsRootFolderModalOpen(false);
  }, []);

  const handleRootFolderChange = useCallback(
    ({
      path: newPath,
      rootFolderPath: newRootFolderPath,
    }: RootFolderUpdated) => {
      setIsRootFolderModalOpen(false);
      setRootFolderPath(newRootFolderPath);
      handleInputChange({ name: 'path', value: newPath });
    },
    [handleInputChange]
  );

  const handleCancelPress = useCallback(() => {
    setIsConfirmMoveModalOpen(false);
  }, []);

  const handleSavePress = useCallback(() => {
    if (isPathChanging && !isConfirmMoveModalOpen) {
      setIsConfirmMoveModalOpen(true);
    } else {
      setIsConfirmMoveModalOpen(false);

      dispatch(
        saveMovie({
          id: movieId,
          moveFiles: false,
        })
      );
    }
  }, [movieId, isPathChanging, isConfirmMoveModalOpen, dispatch]);

  const handleMoveMoviePress = useCallback(() => {
    setIsConfirmMoveModalOpen(false);

    dispatch(
      saveMovie({
        id: movieId,
        moveFiles: true,
      })
    );
  }, [movieId, dispatch]);

  useEffect(() => {
    if (!isSaving && wasSaving && !saveError) {
      onModalClose();
    }
  }, [isSaving, wasSaving, saveError, onModalClose]);

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>{translate('EditMovieModalHeader', { title })}</ModalHeader>

      <ModalBody>
        <Form {...otherSettings}>
          <FormGroup size={sizes.MEDIUM}>
            <FormLabel>{translate('Monitored')}</FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="monitored"
              helpText={translate('MonitoredMovieHelpText')}
              {...(settings.monitored as unknown as Omit<
                CheckInputProps,
                'className' | 'name'
              >)}
              onChange={handleInputChange}
            />
          </FormGroup>

          <FormGroup size={sizes.MEDIUM}>
            <FormLabel>{translate('QualityProfile')}</FormLabel>

            <FormInputGroup
              type={inputTypes.QUALITY_PROFILE_SELECT}
              name="qualityProfileId"
              {...(settings.qualityProfileId as unknown as Omit<
                QualityProfileSelectInputProps,
                'className' | 'name'
              >)}
              onChange={handleInputChange}
            />
          </FormGroup>

          <FormGroup size={sizes.MEDIUM}>
            <FormLabel>{translate('Path')}</FormLabel>

            <FormInputGroup
              type={inputTypes.PATH}
              name="path"
              {...pathSetting}
              buttons={[
                <FormInputButton
                  key="fileBrowser"
                  kind={kinds.DEFAULT}
                  title={translate('RootFolder')}
                  onPress={handleRootFolderPress}
                >
                  <Icon name={icons.ROOT_FOLDER} />
                </FormInputButton>,
              ]}
              includeFiles={false}
              onChange={handleInputChange}
            />
          </FormGroup>

          <FormGroup size={sizes.MEDIUM}>
            <FormLabel>{translate('Tags')}</FormLabel>

            <FormInputGroup
              type={inputTypes.TAG}
              name="tags"
              {...(settings.tags as unknown as Omit<
                MovieTagInputProps<string | number>,
                'className' | 'name'
              >)}
              onChange={handleInputChange}
            />
          </FormGroup>
        </Form>
      </ModalBody>

      <ModalFooter>
        <Button
          className={styles.deleteButton}
          kind={kinds.DANGER}
          onPress={onDeleteMoviePress}
        >
          {translate('Delete')}
        </Button>

        <Button onPress={onModalClose}>{translate('Cancel')}</Button>

        <SpinnerErrorButton
          error={saveError}
          isSpinning={isSaving}
          onPress={handleSavePress}
        >
          {translate('Save')}
        </SpinnerErrorButton>
      </ModalFooter>

      <RootFolderModal
        isOpen={isRootFolderModalOpen}
        movieId={movieId}
        rootFolderPath={rootFolderPath}
        onSavePress={handleRootFolderChange}
        onModalClose={handleRootFolderModalClose}
      />

      <MoveMovieModal
        originalPath={path}
        destinationPath={pendingChanges?.path as string | undefined}
        isOpen={isConfirmMoveModalOpen}
        onModalClose={handleCancelPress}
        onSavePress={handleSavePress}
        onMoveMoviePress={handleMoveMoviePress}
      />
    </ModalContent>
  );
}

export default EditMovieModalContent;
