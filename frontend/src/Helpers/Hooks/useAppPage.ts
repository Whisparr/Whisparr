import { useEffect, useMemo } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';
import { useTranslations } from 'App/useTranslations';
import { useInitializeLanguage } from 'Language/useLanguageName';
import { useLanguages } from 'Language/useLanguages';
import useIndexerFlags from 'Settings/Indexers/useIndexerFlags';
import { fetchCustomFilters } from 'Store/Actions/customFilterActions';
import { fetchSeries } from 'Store/Actions/seriesActions';
import {
  fetchImportLists,
  fetchQualityProfiles,
  fetchUISettings,
} from 'Store/Actions/settingsActions';
import { fetchStatus } from 'Store/Actions/systemActions';
import { fetchTags } from 'Store/Actions/tagActions';
import { ApiError } from 'Utilities/Fetch/fetchJson';

const createErrorsSelector = ({
  indexerFlagsError,
  languagesError,
  translationsError,
}: {
  indexerFlagsError: ApiError | null;
  languagesError: ApiError | null;
  translationsError: ApiError | null;
}) =>
  createSelector(
    (state: AppState) => state.series.error,
    (state: AppState) => state.customFilters.error,
    (state: AppState) => state.tags.error,
    (state: AppState) => state.settings.ui.error,
    (state: AppState) => state.settings.qualityProfiles.error,
    (state: AppState) => state.settings.importLists.error,
    (state: AppState) => state.system.status.error,
    (
      seriesError,
      customFiltersError,
      tagsError,
      uiSettingsError,
      qualityProfilesError,
      importListsError,
      systemStatusError
    ) => {
      const hasError = !!(
        seriesError ||
        customFiltersError ||
        tagsError ||
        uiSettingsError ||
        qualityProfilesError ||
        languagesError ||
        importListsError ||
        indexerFlagsError ||
        systemStatusError ||
        translationsError
      );

      return {
        hasError,
        errors: {
          seriesError,
          customFiltersError,
          tagsError,
          uiSettingsError,
          qualityProfilesError,
          languagesError,
          importListsError,
          indexerFlagsError,
          systemStatusError,
          translationsError,
        },
      };
    }
  );

const useAppPage = () => {
  useInitializeLanguage();

  const dispatch = useDispatch();

  const isReduxPopulated = useSelector(
    (state: AppState) =>
      state.series.isPopulated &&
      state.customFilters.isPopulated &&
      state.tags.isPopulated &&
      state.settings.ui.isPopulated &&
      state.settings.qualityProfiles.isPopulated &&
      state.settings.importLists.isPopulated &&
      state.system.status.isPopulated
  );

  const { isFetched: isIndexerFlagsFetched, error: indexerFlagsError } =
    useIndexerFlags();

  const { isFetched: isLanguagesFetched, error: languagesError } =
    useLanguages();

  const { isFetched: isTranslationsFetched, error: translationsError } =
    useTranslations();

  const isPopulated =
    isReduxPopulated &&
    isIndexerFlagsFetched &&
    isLanguagesFetched &&
    isTranslationsFetched;

  const { hasError, errors } = useSelector(
    createErrorsSelector({
      indexerFlagsError,
      languagesError,
      translationsError,
    })
  );

  const isLocalStorageSupported = useMemo(() => {
    const key = 'sonarrTest';

    try {
      localStorage.setItem(key, key);
      localStorage.removeItem(key);

      return true;
    } catch {
      return false;
    }
  }, []);

  useEffect(() => {
    dispatch(fetchSeries());
    dispatch(fetchCustomFilters());
    dispatch(fetchTags());
    dispatch(fetchQualityProfiles());
    dispatch(fetchImportLists());
    dispatch(fetchUISettings());
    dispatch(fetchStatus());
  }, [dispatch]);

  return useMemo(() => {
    return { errors, hasError, isLocalStorageSupported, isPopulated };
  }, [errors, hasError, isLocalStorageSupported, isPopulated]);
};

export default useAppPage;
