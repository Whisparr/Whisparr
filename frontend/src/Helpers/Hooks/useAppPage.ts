import { useEffect, useMemo } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';
import { fetchTranslations } from 'Store/Actions/appActions';
import { fetchCustomFilters } from 'Store/Actions/customFilterActions';
import { fetchMovies } from 'Store/Actions/movieActions';
import { fetchPerformers } from 'Store/Actions/performerActions';
import {
  fetchImportLists,
  fetchIndexerFlags,
  fetchLanguages,
  fetchQualityProfiles,
  fetchUISettings,
} from 'Store/Actions/settingsActions';
import { fetchStudios } from 'Store/Actions/studioActions';
import { fetchStatus } from 'Store/Actions/systemActions';
import { fetchTags } from 'Store/Actions/tagActions';

const createErrorsSelector = () =>
  createSelector(
    (state: AppState) => state.movies.error,
    // movie collections are not part of Whisparr
    (state: AppState) => state.customFilters.error,
    (state: AppState) => state.performers.error,
    (state: AppState) => state.studios.error,
    (state: AppState) => state.tags.error,
    (state: AppState) => state.settings.ui.error,
    (state: AppState) => state.settings.qualityProfiles.error,
    (state: AppState) => state.settings.languages.error,
    (state: AppState) => state.settings.importLists.error,
    (state: AppState) => state.settings.indexerFlags.error,
    (state: AppState) => state.system.status.error,
    (state: AppState) => state.app.translations.error,
    (
      moviesError,
      // movieCollectionsError,
      customFiltersError,
      performersError,
      studiosError,
      tagsError,
      uiSettingsError,
      qualityProfilesError,
      languagesError,
      importListsError,
      indexerFlagsError,
      systemStatusError,
      translationsError
    ) => {
      const hasError = !!(
        moviesError ||
        customFiltersError ||
        performersError ||
        studiosError ||
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
          moviesError,
          // movieCollectionsError,
          customFiltersError,
          performersError,
          studiosError,
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
  const dispatch = useDispatch();

  const isPopulated = useSelector(
    (state: AppState) =>
      state.movies.isPopulated &&
      state.customFilters.isPopulated &&
      state.performers.isPopulated &&
      state.studios.isPopulated &&
      state.tags.isPopulated &&
      state.settings.ui.isPopulated &&
      state.settings.qualityProfiles.isPopulated &&
      state.settings.languages.isPopulated &&
      state.settings.importLists.isPopulated &&
      state.settings.indexerFlags.isPopulated &&
      state.system.status.isPopulated &&
      state.app.translations.isPopulated
  );

  const { hasError, errors } = useSelector(createErrorsSelector());

  const isLocalStorageSupported = useMemo(() => {
    const key = 'whisparrTest';

    try {
      localStorage.setItem(key, key);
      localStorage.removeItem(key);

      return true;
    } catch {
      return false;
    }
  }, []);

  useEffect(() => {
    dispatch(fetchMovies());
    dispatch(fetchCustomFilters());
    dispatch(fetchPerformers());
    dispatch(fetchStudios());
    dispatch(fetchTags());
    dispatch(fetchQualityProfiles());
    dispatch(fetchLanguages());
    dispatch(fetchImportLists());
    dispatch(fetchIndexerFlags());
    dispatch(fetchUISettings());
    dispatch(fetchStatus());
    dispatch(fetchTranslations());
  }, [dispatch]);

  return useMemo(() => {
    return { errors, hasError, isLocalStorageSupported, isPopulated };
  }, [errors, hasError, isLocalStorageSupported, isPopulated]);
};

export default useAppPage;
