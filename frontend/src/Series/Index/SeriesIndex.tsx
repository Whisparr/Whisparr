import React, {
  useCallback,
  useEffect,
  useMemo,
  useRef,
  useState,
} from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { RouteComponentProps } from 'react-router-dom';
import { SelectProvider } from 'App/SelectContext';
import ClientSideCollectionAppState from 'App/State/ClientSideCollectionAppState';
import SeriesAppState, { SeriesIndexAppState } from 'App/State/SeriesAppState';
import { RSS_SYNC } from 'Commands/commandNames';
import Alert from 'Components/Alert';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import PageJumpBar from 'Components/Page/PageJumpBar';
import PageToolbar from 'Components/Page/Toolbar/PageToolbar';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
import PageToolbarSection from 'Components/Page/Toolbar/PageToolbarSection';
import PageToolbarSeparator from 'Components/Page/Toolbar/PageToolbarSeparator';
import TableOptionsModalWrapper from 'Components/Table/TableOptions/TableOptionsModalWrapper';
import { align, icons, kinds } from 'Helpers/Props';
import SortDirection from 'Helpers/Props/SortDirection';
import ParseToolbarButton from 'Parse/ParseToolbarButton';
import NoSeries from 'Series/NoSeries';
import { executeCommand } from 'Store/Actions/commandActions';
import {
  setJavSeriesFilter,
  setJavSeriesSort,
  setJavSeriesTableOption,
  setJavSeriesView,
} from 'Store/Actions/javSeriesIndexActions';
import { fetchQueueDetails } from 'Store/Actions/queueActions';
import { fetchSeries } from 'Store/Actions/seriesActions';
import {
  setSeriesFilter,
  setSeriesSort,
  setSeriesTableOption,
  setSeriesView,
} from 'Store/Actions/seriesIndexActions';
import scrollPositions from 'Store/scrollPositions';
import createCommandExecutingSelector from 'Store/Selectors/createCommandExecutingSelector';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import createSeriesClientSideCollectionItemsSelector from 'Store/Selectors/createSeriesClientSideCollectionItemsSelector';
import translate from 'Utilities/String/translate';
import SeriesIndexFilterMenu from './Menus/SeriesIndexFilterMenu';
import SeriesIndexSortMenu from './Menus/SeriesIndexSortMenu';
import SeriesIndexViewMenu from './Menus/SeriesIndexViewMenu';
import SeriesIndexOverviewOptionsModal from './Overview/Options/SeriesIndexOverviewOptionsModal';
import SeriesIndexOverviews from './Overview/SeriesIndexOverviews';
import SeriesIndexPosterOptionsModal from './Posters/Options/SeriesIndexPosterOptionsModal';
import SeriesIndexPosters from './Posters/SeriesIndexPosters';
import SeriesIndexSelectAllButton from './Select/SeriesIndexSelectAllButton';
import SeriesIndexSelectAllMenuItem from './Select/SeriesIndexSelectAllMenuItem';
import SeriesIndexSelectFooter from './Select/SeriesIndexSelectFooter';
import SeriesIndexSelectModeButton from './Select/SeriesIndexSelectModeButton';
import SeriesIndexSelectModeMenuItem from './Select/SeriesIndexSelectModeMenuItem';
import SeriesIndexFooter from './SeriesIndexFooter';
import SeriesIndexRefreshSeriesButton from './SeriesIndexRefreshSeriesButton';
import SeriesIndexTable from './Table/SeriesIndexTable';
import SeriesIndexTableOptions from './Table/SeriesIndexTableOptions';
import styles from './SeriesIndex.css';

function getViewComponent(view: string) {
  if (view === 'posters') {
    return SeriesIndexPosters;
  }

  if (view === 'overview') {
    return SeriesIndexOverviews;
  }

  return SeriesIndexTable;
}

interface SeriesIndexProps extends RouteComponentProps {
  seriesType?: string;
}

function getActions(seriesType: string) {
  if (seriesType === 'jav') {
    return {
      dispatchSetSort: setJavSeriesSort,
      dispatchSetFilter: setJavSeriesFilter,
      dispatchSetView: setJavSeriesView,
      dispatchSetTableOption: setJavSeriesTableOption,
    };
  }

  return {
    dispatchSetSort: setSeriesSort,
    dispatchSetFilter: setSeriesFilter,
    dispatchSetView: setSeriesView,
    dispatchSetTableOption: setSeriesTableOption,
  };
}

function SeriesIndex(props: SeriesIndexProps) {
  const seriesType = props.seriesType || 'standard';
  const uiSection = seriesType === 'jav' ? 'javSeriesIndex' : 'seriesIndex';
  const scrollKey = uiSection;

  const initialScrollTop =
    props.history?.action === 'POP' ? scrollPositions[scrollKey] : 0;

  const {
    dispatchSetSort,
    dispatchSetFilter,
    dispatchSetView,
    dispatchSetTableOption,
  } = getActions(seriesType);

  const {
    isFetching,
    isPopulated,
    error,
    totalItems,
    items,
    columns,
    selectedFilterKey,
    filters,
    customFilters,
    sortKey,
    sortDirection,
    view,
  }: SeriesAppState & SeriesIndexAppState & ClientSideCollectionAppState =
    useSelector(
      createSeriesClientSideCollectionItemsSelector(uiSection, seriesType)
    );

  const isRssSyncExecuting = useSelector(
    createCommandExecutingSelector(RSS_SYNC)
  );
  const { isSmallScreen } = useSelector(createDimensionsSelector());
  const dispatch = useDispatch();
  const scrollerRef = useRef<HTMLDivElement>(null);
  const [isOptionsModalOpen, setIsOptionsModalOpen] = useState(false);
  const [jumpToCharacter, setJumpToCharacter] = useState<string | undefined>(
    undefined
  );
  const [isSelectMode, setIsSelectMode] = useState(false);

  useEffect(() => {
    dispatch(fetchSeries());
    dispatch(fetchQueueDetails({ all: true }));
  }, [dispatch]);

  const onRssSyncPress = useCallback(() => {
    dispatch(
      executeCommand({
        name: RSS_SYNC,
      })
    );
  }, [dispatch]);

  const onSelectModePress = useCallback(() => {
    setIsSelectMode(!isSelectMode);
  }, [isSelectMode, setIsSelectMode]);

  const onTableOptionChange = useCallback(
    (payload: unknown) => {
      dispatch(dispatchSetTableOption(payload));
    },
    [dispatch, dispatchSetTableOption]
  );

  const onViewSelect = useCallback(
    (value: string) => {
      dispatch(dispatchSetView({ view: value }));

      if (scrollerRef.current) {
        scrollerRef.current.scrollTo(0, 0);
      }
    },
    [scrollerRef, dispatch, dispatchSetView]
  );

  const onSortSelect = useCallback(
    (value: string) => {
      dispatch(dispatchSetSort({ sortKey: value }));
    },
    [dispatch, dispatchSetSort]
  );

  const onFilterSelect = useCallback(
    (value: string) => {
      dispatch(dispatchSetFilter({ selectedFilterKey: value }));
    },
    [dispatch, dispatchSetFilter]
  );

  const onOptionsPress = useCallback(() => {
    setIsOptionsModalOpen(true);
  }, [setIsOptionsModalOpen]);

  const onOptionsModalClose = useCallback(() => {
    setIsOptionsModalOpen(false);
  }, [setIsOptionsModalOpen]);

  const onJumpBarItemPress = useCallback(
    (character: string) => {
      setJumpToCharacter(character);
    },
    [setJumpToCharacter]
  );

  const onScroll = useCallback(
    ({ scrollTop }: { scrollTop: number }) => {
      setJumpToCharacter(undefined);
      scrollPositions[scrollKey] = scrollTop;
    },
    [setJumpToCharacter, scrollKey]
  );

  const jumpBarItems = useMemo(() => {
    // Reset if not sorting by sortTitle
    if (sortKey !== 'sortTitle') {
      return {
        order: [],
      };
    }

    const characters = items.reduce((acc: Record<string, number>, item) => {
      let char = item.sortTitle.charAt(0);

      if (!isNaN(Number(char))) {
        char = '#';
      }

      if (char in acc) {
        acc[char] = acc[char] + 1;
      } else {
        acc[char] = 1;
      }

      return acc;
    }, {});

    const order = Object.keys(characters).sort();

    // Reverse if sorting descending
    if (sortDirection === SortDirection.Descending) {
      order.reverse();
    }

    return {
      characters,
      order,
    };
  }, [items, sortKey, sortDirection]);
  const ViewComponent = useMemo(() => getViewComponent(view), [view]);

  const isLoaded = !!(!error && isPopulated && items.length);
  const hasNoSeries = !totalItems;

  return (
    <SelectProvider items={items}>
      <PageContent>
        <PageToolbar>
          <PageToolbarSection>
            <SeriesIndexRefreshSeriesButton
              isSelectMode={isSelectMode}
              selectedFilterKey={selectedFilterKey}
            />

            <PageToolbarButton
              label={translate('RssSync')}
              iconName={icons.RSS}
              isSpinning={isRssSyncExecuting}
              isDisabled={hasNoSeries}
              onPress={onRssSyncPress}
            />

            <PageToolbarSeparator />

            <SeriesIndexSelectModeButton
              label={
                isSelectMode
                  ? translate('StopSelecting')
                  : translate('SelectSites')
              }
              iconName={isSelectMode ? icons.SERIES_ENDED : icons.CHECK}
              isSelectMode={isSelectMode}
              overflowComponent={SeriesIndexSelectModeMenuItem}
              onPress={onSelectModePress}
            />

            <SeriesIndexSelectAllButton
              label="SelectAll"
              isSelectMode={isSelectMode}
              overflowComponent={SeriesIndexSelectAllMenuItem}
            />

            <PageToolbarSeparator />
            <ParseToolbarButton />
          </PageToolbarSection>

          <PageToolbarSection
            alignContent={align.RIGHT}
            collapseButtons={false}
          >
            {view === 'table' ? (
              <TableOptionsModalWrapper
                columns={columns}
                optionsComponent={SeriesIndexTableOptions}
                onTableOptionChange={onTableOptionChange}
              >
                <PageToolbarButton
                  label={translate('Options')}
                  iconName={icons.TABLE}
                />
              </TableOptionsModalWrapper>
            ) : (
              <PageToolbarButton
                label={translate('Options')}
                iconName={view === 'posters' ? icons.POSTER : icons.OVERVIEW}
                isDisabled={hasNoSeries}
                onPress={onOptionsPress}
              />
            )}

            <PageToolbarSeparator />

            <SeriesIndexViewMenu
              view={view}
              isDisabled={hasNoSeries}
              onViewSelect={onViewSelect}
            />

            <SeriesIndexSortMenu
              sortKey={sortKey}
              sortDirection={sortDirection}
              isDisabled={hasNoSeries}
              onSortSelect={onSortSelect}
            />

            <SeriesIndexFilterMenu
              selectedFilterKey={selectedFilterKey}
              filters={filters}
              customFilters={customFilters}
              isDisabled={hasNoSeries}
              onFilterSelect={onFilterSelect}
            />
          </PageToolbarSection>
        </PageToolbar>
        <div className={styles.pageContentBodyWrapper}>
          <PageContentBody
            ref={scrollerRef}
            className={styles.contentBody}
            // eslint-disable-next-line @typescript-eslint/ban-ts-comment
            // @ts-ignore
            innerClassName={styles[`${view}InnerContentBody`]}
            initialScrollTop={initialScrollTop}
            onScroll={onScroll}
          >
            {isFetching && !isPopulated ? <LoadingIndicator /> : null}

            {!isFetching && !!error ? (
              <Alert kind={kinds.DANGER}>{translate('SitesLoadError')}</Alert>
            ) : null}

            {isLoaded ? (
              <div className={styles.contentBodyContainer}>
                <ViewComponent
                  scrollerRef={scrollerRef}
                  items={items}
                  sortKey={sortKey}
                  sortDirection={sortDirection}
                  jumpToCharacter={jumpToCharacter}
                  isSelectMode={isSelectMode}
                  isSmallScreen={isSmallScreen}
                />

                <SeriesIndexFooter />
              </div>
            ) : null}

            {!error && isPopulated && !items.length ? (
              <NoSeries totalItems={totalItems} seriesType={seriesType} />
            ) : null}
          </PageContentBody>
          {isLoaded && !!jumpBarItems.order.length ? (
            <PageJumpBar
              items={jumpBarItems}
              onItemPress={onJumpBarItemPress}
            />
          ) : null}
        </div>

        {isSelectMode ? <SeriesIndexSelectFooter /> : null}

        {view === 'posters' ? (
          <SeriesIndexPosterOptionsModal
            isOpen={isOptionsModalOpen}
            onModalClose={onOptionsModalClose}
          />
        ) : null}
        {view === 'overview' ? (
          <SeriesIndexOverviewOptionsModal
            isOpen={isOptionsModalOpen}
            onModalClose={onOptionsModalClose}
          />
        ) : null}
      </PageContent>
    </SelectProvider>
  );
}

export default SeriesIndex;
