import React from 'react';
import RelativeDateCell from 'Components/Table/Cells/RelativeDateCell';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableSelectCell from 'Components/Table/Cells/TableSelectCell';
import Column from 'Components/Table/Column';
import TableRow from 'Components/Table/TableRow';
import MovieSearchCell from 'Movie/MovieSearchCell';
import MovieStatus from 'Movie/MovieStatus';
import MovieTitleLink from 'Movie/MovieTitleLink';
import { SelectStateInputProps } from 'typings/props';
import styles from './MissingRow.css';

interface MissingRowProps {
  id: number;
  foreignId: string;
  movieFileId?: number;
  inCinemas?: string;
  digitalRelease?: string;
  physicalRelease?: string;
  lastSearchTime?: string;
  title: string;
  year: number;
  releaseDate?: string;
  isSelected?: boolean;
  columns: Column[];
  onSelectedChange: (options: SelectStateInputProps) => void;
}

function MissingRow({
  id,
  foreignId,
  movieFileId,
  releaseDate,
  lastSearchTime,
  title,
  year,
  isSelected,
  columns,
  onSelectedChange,
}: MissingRowProps) {
  if (!title) {
    return null;
  }

  return (
    <TableRow>
      <TableSelectCell
        id={id}
        isSelected={isSelected}
        onSelectedChange={onSelectedChange}
      />

      {columns.map((column) => {
        const { name, isVisible } = column;

        if (!isVisible) {
          return null;
        }

        if (name === 'movieMetadata.sortTitle') {
          return (
            <TableRowCell key={name}>
              <MovieTitleLink foreignId={foreignId} title={title} />
            </TableRowCell>
          );
        }

        if (name === 'movieMetadata.year') {
          return <TableRowCell key={name}>{year}</TableRowCell>;
        }

        if (name === 'movieMetadata.releaseDate') {
          return (
            <RelativeDateCell
              key={name}
              date={releaseDate}
              timeForToday={false}
            />
          );
        }

        if (name === 'movies.lastSearchTime') {
          return (
            <RelativeDateCell
              key={name}
              date={lastSearchTime}
              includeSeconds={true}
            />
          );
        }

        if (name === 'status') {
          return (
            <TableRowCell key={name} className={styles.status}>
              <MovieStatus
                movieId={id}
                movieFileId={movieFileId}
                movieEntity="wanted.missing"
              />
            </TableRowCell>
          );
        }

        if (name === 'actions') {
          return (
            <MovieSearchCell
              key={name}
              movieId={id}
              movieEntity="wanted.missing"
            />
          );
        }

        return null;
      })}
    </TableRow>
  );
}

export default MissingRow;
