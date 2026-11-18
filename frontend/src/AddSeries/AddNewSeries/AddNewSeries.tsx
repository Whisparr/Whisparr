import React, { useCallback, useEffect, useRef, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { AddSeries } from 'App/State/AddSeriesAppState';
import AppState from 'App/State/AppState';
import Alert from 'Components/Alert';
import TextInput from 'Components/Form/TextInput';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import Link from 'Components/Link/Link';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import useApiQuery from 'Helpers/Hooks/useApiQuery';
import useDebounce from 'Helpers/Hooks/useDebounce';
import useQueryParams from 'Helpers/Hooks/useQueryParams';
import { icons, kinds } from 'Helpers/Props';
import { setAddSeriesDefault } from 'Store/Actions/addSeriesActions';
import { InputChanged } from 'typings/inputs';
import getErrorMessage from 'Utilities/Object/getErrorMessage';
import translate from 'Utilities/String/translate';
import AddNewSeriesSearchResult from './AddNewSeriesSearchResult';
import styles from './AddNewSeries.css';

interface AddNewSeriesProps {
  defaultSeriesType?: string;
}

function AddNewSeries({ defaultSeriesType }: AddNewSeriesProps) {
  const { term: initialTerm = '' } = useQueryParams<{ term: string }>();
  const dispatch = useDispatch();

  useEffect(() => {
    if (defaultSeriesType) {
      dispatch(setAddSeriesDefault({ seriesType: defaultSeriesType }));
    }
  }, [defaultSeriesType, dispatch]);

  const seriesCount = useSelector(
    (state: AppState) => state.series.items.length
  );

  const [term, setTerm] = useState(initialTerm);
  const searchInputRef = useRef<HTMLInputElement>(null);
  const [isFetching, setIsFetching] = useState(false);
  const query = useDebounce(term, term ? 300 : 0);

  const handleSearchInputChange = useCallback(
    ({ value }: InputChanged<string>) => {
      setTerm(value);
      setIsFetching(!!value.trim());
    },
    []
  );

  const handleClearSeriesLookupPress = useCallback(() => {
    setTerm('');
    setIsFetching(false);
    searchInputRef.current?.focus();
  }, []);

  const {
    isFetching: isFetchingApi,
    error,
    data = [],
  } = useApiQuery<AddSeries[]>({
    path: `/series/lookup?term=${query}`,
    queryOptions: {
      enabled: !!query,
    },
  });

  useEffect(() => {
    setIsFetching(isFetchingApi);
  }, [isFetchingApi]);

  useEffect(() => {
    setTerm(initialTerm);
  }, [initialTerm]);

  return (
    <PageContent title={translate('AddNewSite')}>
      <PageContentBody>
        <div className={styles.searchContainer}>
          <div className={styles.searchIconContainer}>
            <Icon name={icons.SEARCH} size={20} />
          </div>

          <TextInput
            ref={searchInputRef}
            className={styles.searchInput}
            name="seriesLookup"
            value={term}
            placeholder="eg. Brazzers, tpdb:####"
            autoFocus={true}
            onChange={handleSearchInputChange}
          />

          <Button
            className={styles.clearLookupButton}
            onPress={handleClearSeriesLookupPress}
          >
            <Icon name={icons.REMOVE} size={20} />
          </Button>
        </div>

        {isFetching ? <LoadingIndicator /> : null}

        {!isFetching && !!error ? (
          <div className={styles.message}>
            <div className={styles.helpText}>
              {translate('AddNewSiteError')}
            </div>

            <Alert kind={kinds.DANGER}>{getErrorMessage(error)}</Alert>
          </div>
        ) : null}

        {!isFetching && !error && !!data.length ? (
          <div className={styles.searchResults}>
            {data.map((item) => {
              return <AddNewSeriesSearchResult key={item.tvdbId} {...item} />;
            })}
          </div>
        ) : null}

        {!isFetching && !error && !data.length && term ? (
          <div className={styles.message}>
            <div className={styles.noResults}>
              {translate('CouldNotFindResults', { term })}
            </div>
            <div>{translate('SearchByTpdbId')}</div>
            <div>
              <Link to="https://wiki.servarr.com/whisparr/faq#why-cant-i-add-a-new-site-when-i-know-the-tpdb-id">
                {translate('WhyCantIFindMySite')}
              </Link>
            </div>
          </div>
        ) : null}

        {term ? null : (
          <div className={styles.message}>
            <div className={styles.helpText}>
              {translate('AddNewSiteHelpText')}
            </div>
            <div>{translate('SearchByTpdbId')}</div>
          </div>
        )}

        {!term && !seriesCount ? (
          <div className={styles.message}>
            <div className={styles.noSeriesText}>
              {translate('NoSitesHaveBeenAdded')}
            </div>
            <div>
              <Button to="/add/import" kind={kinds.PRIMARY}>
                {translate('ImportExistingSites')}
              </Button>
            </div>
          </div>
        ) : null}

        <div />
      </PageContentBody>
    </PageContent>
  );
}

export default AddNewSeries;
