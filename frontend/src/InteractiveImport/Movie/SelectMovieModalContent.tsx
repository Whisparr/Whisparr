import { debounce } from 'lodash';
import React, {
  useCallback,
  useEffect,
  useMemo,
  useRef,
  useState,
} from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { FixedSizeList as List, ListChildComponentProps } from 'react-window';
import AppState from 'App/State/AppState';
import TextInput from 'Components/Form/TextInput';
import Button from 'Components/Link/Button';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import Scroller from 'Components/Scroller/Scroller';
import Column from 'Components/Table/Column';
import VirtualTableRowButton from 'Components/Table/VirtualTableRowButton';
import { scrollDirections } from 'Helpers/Props';
import Movie from 'Movie/Movie';
import { searchMovies } from 'Store/Actions/movieActions';
import dimensions from 'Styles/Variables/dimensions';
import translate from 'Utilities/String/translate';
import SelectMovieModalTableHeader from './SelectMovieModalTableHeader';
import SelectMovieRow from './SelectMovieRow';
import styles from './SelectMovieModalContent.css';

const columns: Column[] = [
  { name: 'studioTitle', label: () => translate('Studio'), isVisible: true },
  { name: 'title', label: () => translate('Title'), isVisible: true },
  { name: 'performers', label: () => translate('Performers'), isVisible: true },
  {
    name: 'releaseDate',
    label: () => translate('ReleaseDate'),
    isVisible: true,
  },
];

const bodyPadding = parseInt(dimensions.pageContentBodyPadding);

interface SelectMovieModalContentProps {
  modalTitle: string;
  relativePath: string;
  onMovieSelect(movie: Movie): void;
  onModalClose(): void;
}

interface RowItemData {
  items: Movie[];
  columns: Column[];
  onMovieSelect(movieId: number): void;
}

function Row({ index, style, data }: ListChildComponentProps<RowItemData>) {
  const { items, columns, onMovieSelect } = data;
  const movie = index >= items.length ? null : items[index];

  const handlePress = useCallback(() => {
    if (movie?.id) {
      onMovieSelect(movie.id);
    }
  }, [movie?.id, onMovieSelect]);

  if (!movie) return null;

  return (
    <VirtualTableRowButton
      style={{
        display: 'flex',
        alignItems: 'center',
        borderTop: '1px solid #858585',
        justifyContent: 'space-between',
        ...style,
      }}
      onPress={handlePress}
    >
      <SelectMovieRow
        id={movie.id}
        title={movie.title}
        tmdbId={movie.tmdbId}
        credits={movie.credits}
        studioTitle={movie.studioTitle}
        releaseDate={movie.releaseDate}
        columns={columns}
        onMovieSelect={onMovieSelect}
      />
    </VirtualTableRowButton>
  );
}

function SelectMovieModalContent(props: SelectMovieModalContentProps) {
  const { modalTitle, relativePath, onMovieSelect, onModalClose } = props;

  const dispatch = useDispatch();

  const debouncedDispatchSearch = useMemo(
    // only fire API call once every 300ms, for those fast typers
    () => debounce((val: string) => dispatch(searchMovies(val)), 300),
    [dispatch]
  );

  const listRef = useRef<List<RowItemData>>(null);
  const scrollerRef = useRef<HTMLDivElement>(null);

  const [filter, setFilter] = useState('');
  const [size, setSize] = useState({ width: 0, height: 0 });
  const windowHeight = window.innerHeight;

  // movies come straight from Redux after searchMovies thunk runs
  const items: Movie[] = useSelector((state: AppState) => state.movies.items);

  // measure scroller size
  useEffect(() => {
    const current = scrollerRef.current;
    if (current) {
      const width = current.clientWidth;
      const height = current.clientHeight;
      const padding = bodyPadding - 5;
      setSize({ width: width - padding * 2, height: height + padding });
    }
  }, [windowHeight]);

  // sync scroller with react-window list
  useEffect(() => {
    const currentScrollerRef = scrollerRef.current;
    if (!currentScrollerRef) return;

    const handleScroll = () => {
      const { offsetTop = 0 } = currentScrollerRef;
      const scrollTop = currentScrollerRef.scrollTop - offsetTop;
      listRef.current?.scrollTo(scrollTop);
    };

    const throttled = debounce(handleScroll, 10);
    currentScrollerRef.addEventListener('scroll', throttled);

    return () => {
      currentScrollerRef.removeEventListener('scroll', throttled);
      throttled.cancel();
    };
  }, []);

  // debounce filter input to prevent browser throttling
  const debouncedSetFilter = useMemo(
    () => debounce((val: string) => setFilter(val), 50),
    []
  );

  const onFilterChange = useCallback(
    ({ value }: { value: string }) => {
      setFilter(value); // update instantly
      if (value.length >= 3 || value.length === 0) {
        // debounce API call to avoid flooding
        debouncedDispatchSearch(value);
      }
    },
    [debouncedDispatchSearch]
  );

  // cleanup debounce on unmount
  useEffect(() => {
    return () => {
      debouncedSetFilter.cancel();
      debouncedDispatchSearch.cancel();
    };
  }, [debouncedSetFilter, debouncedDispatchSearch]);

  const onMovieSelectWrapper = useCallback(
    (movieId: number) => {
      const movie = items.find((m) => m.id === movieId);
      if (movie) {
        onMovieSelect(movie);
      }
    },
    [items, onMovieSelect]
  );

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>{modalTitle} - Select Movie</ModalHeader>

      <ModalBody
        className={styles.modalBody}
        scrollDirection={scrollDirections.NONE}
      >
        <TextInput
          className={styles.filterInput}
          placeholder="Filter movies"
          name="filter"
          value={filter}
          autoFocus={true}
          onChange={onFilterChange}
        />

        <Scroller
          ref={scrollerRef}
          className={styles.scroller}
          autoFocus={false}
        >
          <SelectMovieModalTableHeader columns={columns} />
          <List<RowItemData>
            ref={listRef}
            style={{ width: '100%', height: '100%', overflow: 'hidden' }}
            width={size.width}
            height={size.height}
            itemCount={items.length}
            itemSize={38}
            itemData={{ items, columns, onMovieSelect: onMovieSelectWrapper }}
          >
            {Row}
          </List>
        </Scroller>
      </ModalBody>

      <ModalFooter className={styles.footer}>
        <div className={styles.path}>{relativePath}</div>
        <Button onPress={onModalClose}>Cancel</Button>
      </ModalFooter>
    </ModalContent>
  );
}

export default SelectMovieModalContent;
