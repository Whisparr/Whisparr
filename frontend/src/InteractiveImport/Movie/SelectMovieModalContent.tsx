import { throttle } from 'lodash';
import React, {
  useCallback,
  useEffect,
  useMemo,
  useRef,
  useState,
} from 'react';
import { useSelector } from 'react-redux';
import { FixedSizeList as List, ListChildComponentProps } from 'react-window';
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
import createAllItemsSelector from 'Store/Selectors/createAllItemsSelector';
import dimensions from 'Styles/Variables/dimensions';
import sortByProp from 'Utilities/Array/sortByProp';
import translate from 'Utilities/String/translate';
import SelectMovieModalTableHeader from './SelectMovieModalTableHeader';
import SelectMovieRow from './SelectMovieRow';
import styles from './SelectMovieModalContent.css';

const columns = [
  {
    name: 'studioTitle',
    label: () => translate('Studio'),
    isVisible: true,
  },
  {
    name: 'title',
    label: () => translate('Title'),
    isVisible: true,
  },
  {
    name: 'performers',
    label: () => translate('Performers'),
    isVisible: true,
  },
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

  if (movie == null) {
    return null;
  }

  return (
    <VirtualTableRowButton
      style={{
        display: 'flex',
        'align-items': 'center',
        'border-top': '1px solid #858585',
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

  const listRef = useRef<List<RowItemData>>(null);
  const scrollerRef = useRef<HTMLDivElement>(null);
  const allMovies: Movie[] = useSelector(createAllItemsSelector());
  const [filter, setFilter] = useState('');
  const [size, setSize] = useState({ width: 0, height: 0 });
  const windowHeight = window.innerHeight;

  useEffect(() => {
    const current = scrollerRef?.current as HTMLElement;

    if (current) {
      const width = current.clientWidth;
      const height = current.clientHeight;
      const padding = bodyPadding - 5;

      setSize({
        width: width - padding * 2,
        height: height + padding,
      });
    }
  }, [windowHeight, scrollerRef]);

  useEffect(() => {
    const currentScrollerRef = scrollerRef.current as HTMLElement;
    const currentScrollListener = currentScrollerRef;

    const handleScroll = throttle(() => {
      const { offsetTop = 0 } = currentScrollerRef;
      const scrollTop = currentScrollerRef.scrollTop - offsetTop;

      listRef.current?.scrollTo(scrollTop);
    }, 10);

    currentScrollListener.addEventListener('scroll', handleScroll);

    return () => {
      handleScroll.cancel();

      if (currentScrollListener) {
        currentScrollListener.removeEventListener('scroll', handleScroll);
      }
    };
  }, [listRef, scrollerRef]);

  const onFilterChange = useCallback(
    ({ value }: { value: string }) => {
      setFilter(value);
    },
    [setFilter]
  );

  const onMovieSelectWrapper = useCallback(
    (movieId: number) => {
      const movie = allMovies.find((s) => s.id === movieId) as Movie;

      onMovieSelect(movie);
    },
    [allMovies, onMovieSelect]
  );

  function normalize(input: string): string {
    const stopWords = new Set(['a', 'an', 'the', 'and', 'or', 'of']);
    return input
      .toLowerCase()
      .split(/[^a-z0-9]+/) // split on non-alphanumeric
      .filter((word) => word && !stopWords.has(word))
      .join('');
  }

  const items = useMemo(() => {
    const sorted = [...allMovies].sort(sortByProp('sortTitle'));

    const normalizedFilter = normalize(filter);

    return sorted.filter(
      (item) =>
        normalize(item.cleanTitle).includes(normalizedFilter) ||
        item.stashId === filter ||
        item.tmdbId.toString() === filter
    );
  }, [allMovies, filter]);

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
            style={{
              width: '100%',
              height: '100%',
              overflow: 'none',
            }}
            width={size.width}
            height={size.height}
            itemCount={items.length}
            itemSize={38}
            itemData={{
              items,
              columns,
              onMovieSelect: onMovieSelectWrapper,
            }}
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
