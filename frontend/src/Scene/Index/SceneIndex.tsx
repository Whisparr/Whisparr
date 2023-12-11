import React, {
  useCallback,
  useEffect,
  useMemo,
  useRef,
  useState,
} from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { SelectProvider } from 'App/SelectContext';
import ClientSideCollectionAppState from 'App/State/ClientSideCollectionAppState';
import MoviesAppState, { MovieIndexAppState } from 'App/State/MoviesAppState';
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
import withScrollPosition from 'Components/withScrollPosition';
import { align, icons, kinds } from 'Helpers/Props';
import SortDirection from 'Helpers/Props/SortDirection';
import InteractiveImportModal from 'InteractiveImport/InteractiveImportModal';
import NoScene from 'Scene/NoScene';
import { executeCommand } from 'Store/Actions/commandActions';
import { fetchQueueDetails } from 'Store/Actions/queueActions';
import {
  setSceneFilter,
  setSceneSort,
  setSceneTableOption,
  setSceneView,
} from 'Store/Actions/sceneIndexActions';
import scrollPositions from 'Store/scrollPositions';
import createCommandExecutingSelector from 'Store/Selectors/createCommandExecutingSelector';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import createMovieClientSideCollectionItemsSelector from 'Store/Selectors/createMovieClientSideCollectionItemsSelector';
import translate from 'Utilities/String/translate';
import SceneIndexFilterMenu from './Menus/SceneIndexFilterMenu';
import SceneIndexSortMenu from './Menus/SceneIndexSortMenu';
import SceneIndexViewMenu from './Menus/SceneIndexViewMenu';
import SceneIndexOverviewOptionsModal from './Overview/Options/SceneIndexOverviewOptionsModal';
import SceneIndexOverviews from './Overview/SceneIndexOverviews';
import SceneIndexPosterOptionsModal from './Posters/Options/SceneIndexPosterOptionsModal';
import SceneIndexPosters from './Posters/SceneIndexPosters';
import SceneIndexFooter from './SceneIndexFooter';
import SceneIndexRefreshSceneButton from './SceneIndexRefreshSceneButton';
import SceneIndexSearchButton from './SceneIndexSearchButton';
import SceneIndexSelectAllButton from './Select/SceneIndexSelectAllButton';
import SceneIndexSelectAllMenuItem from './Select/SceneIndexSelectAllMenuItem';
import SceneIndexSelectFooter from './Select/SceneIndexSelectFooter';
import SceneIndexSelectModeButton from './Select/SceneIndexSelectModeButton';
import SceneIndexSelectModeMenuItem from './Select/SceneIndexSelectModeMenuItem';
import SceneIndexTable from './Table/SceneIndexTable';
import SceneIndexTableOptions from './Table/SceneIndexTableOptions';
import styles from './SceneIndex.css';

function getViewComponent(view: string) {
  if (view === 'posters') {
    return SceneIndexPosters;
  }

  if (view === 'overview') {
    return SceneIndexOverviews;
  }

  return SceneIndexTable;
}

interface SceneIndexProps {
  initialScrollTop?: number;
}

const SceneIndex = withScrollPosition((props: SceneIndexProps) => {
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
  }: MoviesAppState & MovieIndexAppState & ClientSideCollectionAppState =
    useSelector(
      createMovieClientSideCollectionItemsSelector('sceneIndex', 'scene')
    );

  const isRssSyncExecuting = useSelector(
    createCommandExecutingSelector(RSS_SYNC)
  );
  const { isSmallScreen } = useSelector(createDimensionsSelector());
  const dispatch = useDispatch();
  const scrollerRef = useRef<HTMLDivElement>(null);
  const [isOptionsModalOpen, setIsOptionsModalOpen] = useState(false);
  const [isInteractiveImportModalOpen, setIsInteractiveImportModalOpen] =
    useState(false);
  const [jumpToCharacter, setJumpToCharacter] = useState<string | undefined>(
    undefined
  );
  const [isSelectMode, setIsSelectMode] = useState(false);

  useEffect(() => {
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
      dispatch(setSceneTableOption(payload));
    },
    [dispatch]
  );

  const onViewSelect = useCallback(
    (value: string) => {
      dispatch(setSceneView({ view: value }));

      if (scrollerRef.current) {
        scrollerRef.current.scrollTo(0, 0);
      }
    },
    [scrollerRef, dispatch]
  );

  const onSortSelect = useCallback(
    (value: string) => {
      dispatch(setSceneSort({ sortKey: value }));
    },
    [dispatch]
  );

  const onFilterSelect = useCallback(
    (value: string) => {
      dispatch(setSceneFilter({ selectedFilterKey: value }));
    },
    [dispatch]
  );

  const onOptionsPress = useCallback(() => {
    setIsOptionsModalOpen(true);
  }, [setIsOptionsModalOpen]);

  const onOptionsModalClose = useCallback(() => {
    setIsOptionsModalOpen(false);
  }, [setIsOptionsModalOpen]);

  const onInteractiveImportPress = useCallback(() => {
    setIsInteractiveImportModalOpen(true);
  }, [setIsInteractiveImportModalOpen]);

  const onInteractiveImportModalClose = useCallback(() => {
    setIsInteractiveImportModalOpen(false);
  }, [setIsInteractiveImportModalOpen]);

  const onJumpBarItemPress = useCallback(
    (character: string) => {
      setJumpToCharacter(character);
    },
    [setJumpToCharacter]
  );

  const onScroll = useCallback(
    ({ scrollTop }: { scrollTop: number }) => {
      setJumpToCharacter(undefined);
      scrollPositions.sceneIndex = scrollTop;
    },
    [setJumpToCharacter]
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
  const hasNoScene = !totalItems;

  return (
    <SelectProvider items={items}>
      <PageContent>
        <PageToolbar>
          <PageToolbarSection>
            <SceneIndexRefreshSceneButton
              isSelectMode={isSelectMode}
              selectedFilterKey={selectedFilterKey}
            />

            <PageToolbarButton
              label={translate('RSSSync')}
              iconName={icons.RSS}
              isSpinning={isRssSyncExecuting}
              isDisabled={hasNoScene}
              onPress={onRssSyncPress}
            />

            <PageToolbarSeparator />

            <SceneIndexSearchButton
              isSelectMode={isSelectMode}
              selectedFilterKey={selectedFilterKey}
            />

            <PageToolbarButton
              label={translate('ManualImport')}
              iconName={icons.INTERACTIVE}
              isDisabled={hasNoScene}
              onPress={onInteractiveImportPress}
            />

            <PageToolbarSeparator />

            <SceneIndexSelectModeButton
              label={
                isSelectMode
                  ? translate('StopSelecting')
                  : translate('EditScenes')
              }
              iconName={isSelectMode ? icons.SERIES_ENDED : icons.EDIT}
              isSelectMode={isSelectMode}
              overflowComponent={SceneIndexSelectModeMenuItem}
              onPress={onSelectModePress}
            />

            <SceneIndexSelectAllButton
              label="SelectAll"
              isSelectMode={isSelectMode}
              overflowComponent={SceneIndexSelectAllMenuItem}
            />
          </PageToolbarSection>

          <PageToolbarSection
            alignContent={align.RIGHT}
            collapseButtons={false}
          >
            {view === 'table' ? (
              <TableOptionsModalWrapper
                columns={columns}
                optionsComponent={SceneIndexTableOptions}
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
                isDisabled={hasNoScene}
                onPress={onOptionsPress}
              />
            )}

            <PageToolbarSeparator />

            <SceneIndexViewMenu
              view={view}
              isDisabled={hasNoScene}
              onViewSelect={onViewSelect}
            />

            <SceneIndexSortMenu
              sortKey={sortKey}
              sortDirection={sortDirection}
              isDisabled={hasNoScene}
              onSortSelect={onSortSelect}
            />

            <SceneIndexFilterMenu
              selectedFilterKey={selectedFilterKey}
              filters={filters}
              customFilters={customFilters}
              isDisabled={hasNoScene}
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
            initialScrollTop={props.initialScrollTop}
            onScroll={onScroll}
          >
            {isFetching && !isPopulated ? <LoadingIndicator /> : null}

            {!isFetching && !!error ? (
              <Alert kind={kinds.DANGER}>
                {translate('UnableToLoadScenes')}
              </Alert>
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

                <SceneIndexFooter />
              </div>
            ) : null}

            {!error && isPopulated && !items.length ? (
              <NoScene totalItems={totalItems} />
            ) : null}
          </PageContentBody>

          {isLoaded && !!jumpBarItems.order.length ? (
            <PageJumpBar
              items={jumpBarItems}
              onItemPress={onJumpBarItemPress}
            />
          ) : null}
        </div>

        {isSelectMode ? <SceneIndexSelectFooter /> : null}

        <InteractiveImportModal
          isOpen={isInteractiveImportModalOpen}
          onModalClose={onInteractiveImportModalClose}
        />

        {view === 'posters' ? (
          <SceneIndexPosterOptionsModal
            isOpen={isOptionsModalOpen}
            onModalClose={onOptionsModalClose}
          />
        ) : null}
        {view === 'overview' ? (
          <SceneIndexOverviewOptionsModal
            isOpen={isOptionsModalOpen}
            onModalClose={onOptionsModalClose}
          />
        ) : null}
      </PageContent>
    </SelectProvider>
  );
}, 'sceneIndex');

export default SceneIndex;
