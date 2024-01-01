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
import PerformersAppState from 'App/State/PerformersAppState';
import Alert from 'Components/Alert';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import PageJumpBar from 'Components/Page/PageJumpBar';
import PageToolbar from 'Components/Page/Toolbar/PageToolbar';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
import PageToolbarSection from 'Components/Page/Toolbar/PageToolbarSection';
import PageToolbarSeparator from 'Components/Page/Toolbar/PageToolbarSeparator';
import withScrollPosition from 'Components/withScrollPosition';
import { align, icons, kinds } from 'Helpers/Props';
import SortDirection from 'Helpers/Props/SortDirection';
import MovieIndexSelectAllButton from 'Movie/Index/Select/MovieIndexSelectAllButton';
import MovieIndexSelectAllMenuItem from 'Movie/Index/Select/MovieIndexSelectAllMenuItem';
import MovieIndexSelectModeButton from 'Movie/Index/Select/MovieIndexSelectModeButton';
import MovieIndexSelectModeMenuItem from 'Movie/Index/Select/MovieIndexSelectModeMenuItem';
import NoPerformer from 'Performer/NoPerformer';
import {
  setPerformerFilter,
  setPerformerSort,
} from 'Store/Actions/performerActions';
import { fetchQueueDetails } from 'Store/Actions/queueActions';
import scrollPositions from 'Store/scrollPositions';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import createPerformerClientSideCollectionItemsSelector from 'Store/Selectors/createPerformerClientSideCollectionItemsSelector';
import translate from 'Utilities/String/translate';
import PerformerIndexFilterMenu from './Menus/PerformerIndexFilterMenu';
import PerformerIndexSortMenu from './Menus/PerformerIndexSortMenu';
import PerformerIndexPosterOptionsModal from './Posters/Options/PerformerIndexPosterOptionsModal';
import PerformerIndexPosters from './Posters/PerformerIndexPosters';
import PerformerIndexSelectFooter from './Select/PerformerIndexSelectFooter';
import styles from './PerformerIndex.css';

interface PerformerIndexProps {
  initialScrollTop?: number;
}

const PerformerIndex = withScrollPosition((props: PerformerIndexProps) => {
  const {
    isFetching,
    isPopulated,
    error,
    totalItems,
    items,
    selectedFilterKey,
    filters,
    customFilters,
    sortKey,
    sortDirection,
    view,
  }: PerformersAppState & ClientSideCollectionAppState = useSelector(
    createPerformerClientSideCollectionItemsSelector('performers')
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
    dispatch(fetchQueueDetails({ all: true }));
  }, [dispatch]);

  const onSelectModePress = useCallback(() => {
    setIsSelectMode(!isSelectMode);
  }, [isSelectMode, setIsSelectMode]);

  const onSortSelect = useCallback(
    (value: string) => {
      dispatch(setPerformerSort({ sortKey: value }));
    },
    [dispatch]
  );

  const onFilterSelect = useCallback(
    (value: string) => {
      dispatch(setPerformerFilter({ selectedFilterKey: value }));
    },
    [dispatch]
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
  const ViewComponent = PerformerIndexPosters;

  const isLoaded = !!(!error && isPopulated && items.length);
  const hasNoPerformer = !totalItems;

  return (
    <SelectProvider items={items}>
      <PageContent>
        <PageToolbar>
          <PageToolbarSection>
            <MovieIndexSelectModeButton
              label={
                isSelectMode
                  ? translate('StopSelecting')
                  : translate('EditPerformers')
              }
              iconName={isSelectMode ? icons.SERIES_ENDED : icons.EDIT}
              isSelectMode={isSelectMode}
              overflowComponent={MovieIndexSelectModeMenuItem}
              onPress={onSelectModePress}
            />
            <MovieIndexSelectAllButton
              label="SelectAll"
              isSelectMode={isSelectMode}
              overflowComponent={MovieIndexSelectAllMenuItem}
            />
          </PageToolbarSection>
          <PageToolbarSection
            alignContent={align.RIGHT}
            collapseButtons={false}
          >
            <PageToolbarButton
              label={translate('Options')}
              iconName={icons.POSTER}
              isDisabled={hasNoPerformer}
              onPress={onOptionsPress}
            />

            <PageToolbarSeparator />

            <PerformerIndexSortMenu
              sortKey={sortKey}
              sortDirection={sortDirection}
              isDisabled={hasNoPerformer}
              onSortSelect={onSortSelect}
            />

            <PerformerIndexFilterMenu
              selectedFilterKey={selectedFilterKey}
              filters={filters}
              customFilters={customFilters}
              isDisabled={hasNoPerformer}
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
                {translate('UnableToLoadPerformers')}
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
                  isSmallScreen={isSmallScreen}
                  isSelectMode={isSelectMode}
                />
              </div>
            ) : null}

            {!error && isPopulated && !items.length ? (
              <NoPerformer totalItems={totalItems} />
            ) : null}
          </PageContentBody>

          {isLoaded && !!jumpBarItems.order.length ? (
            <PageJumpBar
              items={jumpBarItems}
              onItemPress={onJumpBarItemPress}
            />
          ) : null}
        </div>

        {isSelectMode ? <PerformerIndexSelectFooter /> : null}

        <PerformerIndexPosterOptionsModal
          isOpen={isOptionsModalOpen}
          onModalClose={onOptionsModalClose}
        />
      </PageContent>
    </SelectProvider>
  );
}, 'performers');

export default PerformerIndex;
