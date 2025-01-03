import React, { useCallback, useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';
import Alert from 'Components/Alert';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import Button from 'Components/Link/Button';
import SpinnerErrorButton from 'Components/Link/SpinnerErrorButton';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import usePrevious from 'Helpers/Hooks/usePrevious';
import { inputTypes, kinds } from 'Helpers/Props';
import {
  saveImportListExclusion,
  setImportListExclusionValue,
} from 'Store/Actions/settingsActions';
import selectSettings from 'Store/Selectors/selectSettings';
import ImportListExclusion from 'typings/ImportListExclusion';
import { InputChanged } from 'typings/inputs';
import { PendingSection } from 'typings/pending';
import translate from 'Utilities/String/translate';
import styles from './EditImportListExclusionModalContent.css';

const newImportListExclusion = {
  movieTitle: '',
  movieYear: 0,
  type: 'scene',
  foreignId: '',
};

const typeOptions = [
  { key: 'movie', value: translate('Movie') },
  { key: 'scene', value: translate('Scene') },
  { key: 'studio', value: translate('Studio') },
  { key: 'performer', value: translate('Performer') },
  { key: 'tag', value: translate('Tag') },
];

interface EditImportListExclusionModalContentProps {
  id?: number;
  onModalClose: () => void;
  onDeleteImportListExclusionPress?: () => void;
}

function createImportListExclusionSelector(id?: number) {
  return createSelector(
    (state: AppState) => state.settings.importListExclusions,
    (importListExclusions) => {
      const { isFetching, error, isSaving, saveError, pendingChanges, items } =
        importListExclusions;

      const mapping = id
        ? items.find((i) => i.id === id)
        : newImportListExclusion;
      const settings = selectSettings(mapping, pendingChanges, saveError);

      return {
        id,
        isFetching,
        error,
        isSaving,
        saveError,
        item: settings.settings as PendingSection<ImportListExclusion>,
        ...settings,
      };
    }
  );
}

function EditImportListExclusionModalContent({
  id,
  onModalClose,
  onDeleteImportListExclusionPress,
}: EditImportListExclusionModalContentProps) {
  const { isFetching, isSaving, item, error, saveError, ...otherProps } =
    useSelector(createImportListExclusionSelector(id));

  const { movieTitle, movieYear, foreignId, type } = item;

  const dispatch = useDispatch();
  const previousIsSaving = usePrevious(isSaving);

  const dispatchSetImportListExclusionValue = (payload: {
    name: string;
    value: string | number;
  }) => {
    // @ts-expect-error 'setImportListExclusionValue' isn't typed yet
    dispatch(setImportListExclusionValue(payload));
  };

  useEffect(() => {
    if (!id) {
      Object.entries(newImportListExclusion).forEach(([name, value]) => {
        dispatchSetImportListExclusionValue({ name, value });
      });
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  useEffect(() => {
    if (previousIsSaving && !isSaving && !saveError) {
      onModalClose();
    }
  }, [previousIsSaving, isSaving, saveError, onModalClose]);

  const onSavePress = useCallback(() => {
    dispatch(saveImportListExclusion({ id }));
  }, [dispatch, id]);

  const onInputChange = useCallback(
    (change: InputChanged) => {
      // @ts-expect-error 'setImportListExclusionValue' isn't typed yet
      dispatch(setImportListExclusionValue(change));
    },
    [dispatch]
  );

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {id
          ? translate('EditImportListExclusion')
          : translate('AddImportListExclusion')}
      </ModalHeader>

      <ModalBody className={styles.body}>
        {isFetching && <LoadingIndicator />}

        {!isFetching && !!error && (
          <Alert kind={kinds.DANGER}>
            {translate('AddImportListExclusionError')}
          </Alert>
        )}

        {!isFetching && !error && (
          <Form {...otherProps}>
            <FormGroup>
              <FormLabel>{translate('ForeignId')}</FormLabel>

              <FormInputGroup
                type={inputTypes.TEXT}
                name="foreignId"
                helpText={translate('ForiegnIdHelpText')}
                {...foreignId}
                onChange={onInputChange}
              />
            </FormGroup>

            <FormGroup>
              <FormLabel>{translate('Title')}</FormLabel>

              <FormInputGroup
                type={inputTypes.TEXT}
                name="movieTitle"
                helpText={translate('MovieTitleToExcludeHelpText')}
                {...movieTitle}
                onChange={onInputChange}
              />
            </FormGroup>

            <FormGroup>
              <FormLabel>{translate('ExclusionType')}</FormLabel>

              <FormInputGroup
                type={inputTypes.SELECT}
                name="type"
                {...type}
                values={typeOptions}
                helpText={translate('ExclusionTypeHelpText')}
                onChange={onInputChange}
              />
            </FormGroup>

            <FormGroup>
              <FormLabel>{translate('Year')}</FormLabel>

              <FormInputGroup
                type={inputTypes.NUMBER}
                name="movieYear"
                helpText={translate('MovieYearToExcludeHelpText')}
                {...movieYear}
                onChange={onInputChange}
              />
            </FormGroup>
          </Form>
        )}
      </ModalBody>

      <ModalFooter>
        {foreignId && (
          <Button
            className={styles.deleteButton}
            kind={kinds.DANGER}
            onPress={onDeleteImportListExclusionPress}
          >
            {translate('Delete')}
          </Button>
        )}

        <Button onPress={onModalClose}>{translate('Cancel')}</Button>

        <SpinnerErrorButton
          isSpinning={isSaving}
          error={saveError}
          onPress={onSavePress}
        >
          {translate('Save')}
        </SpinnerErrorButton>
      </ModalFooter>
    </ModalContent>
  );
}

export default EditImportListExclusionModalContent;
