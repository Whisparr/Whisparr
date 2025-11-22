import React from 'react';
import Label from 'Components/Label';
import Link from 'Components/Link/Link';
import { kinds, sizes } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import styles from './MovieDetailsLinks.css';

interface MovieDetailsLinksProps {
  tmdbId?: number;
  stashId?: string;
}

function MovieDetailsLinks(props: MovieDetailsLinksProps) {
  const { tmdbId, stashId } = props;

  return (
    <div className={styles.links}>
      {!!tmdbId && (
        <Link
          className={styles.link}
          to={`https://www.themoviedb.org/movie/${tmdbId}`}
        >
          <Label
            className={styles.linkLabel}
            kind={kinds.INFO}
            size={sizes.LARGE}
          >
            {translate('TMDb')}
          </Label>
        </Link>
      )}

      {!!stashId && (
        <Link
          className={styles.link}
          to={`https://stashdb.org/scenes/${stashId}/`}
        >
          <Label
            className={styles.linkLabel}
            kind={kinds.INFO}
            size={sizes.LARGE}
          >
            {translate('StashDB')}
          </Label>
        </Link>
      )}
    </div>
  );
}

export default MovieDetailsLinks;
