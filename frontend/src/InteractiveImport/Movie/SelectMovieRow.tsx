import React from 'react';
import RelativeDateCell from 'Components/Table/Cells/RelativeDateCell';
import VirtualTableRowCell from 'Components/Table/Cells/VirtualTableRowCell';
import styles from './SelectMovieRow.css';

interface SelectMovieRowProps {
  title: string;
  studioTitle: string;
  releaseDate: string;
  performers: string;
}

function SelectMovieRow({
  studioTitle,
  title,
  releaseDate,
  performers,
}: SelectMovieRowProps) {
  return (
    <>
      <VirtualTableRowCell className={styles.studioTitle}>
        {studioTitle}
      </VirtualTableRowCell>

      <VirtualTableRowCell className={styles.title}>
        {title}
      </VirtualTableRowCell>

      <VirtualTableRowCell className={styles.performers}>
        {performers}
      </VirtualTableRowCell>

      <VirtualTableRowCell className={styles.releaseDate}>
        <RelativeDateCell
          key={title}
          date={releaseDate}
          className={styles.releaseDate}
        />
      </VirtualTableRowCell>
    </>
  );
}

export default SelectMovieRow;
