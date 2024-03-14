import PropTypes from 'prop-types';
import React from 'react';
import Label from 'Components/Label';
import RelativeDateCellConnector from 'Components/Table/Cells/RelativeDateCellConnector';
import { kinds } from 'Helpers/Props';
import MoviePoster from 'Movie/MoviePoster';
import ScenePoster from 'Scene/ScenePoster';
import translate from 'Utilities/String/translate';
import styles from './MovieSearchResult.css';

function MovieSearchResult(props) {
  const {
    match,
    title,
    year,
    images,
    alternateTitles,
    tmdbId,
    imdbId,
    studioTitle,
    itemType,
    releaseDate,
    tags,
    safeForWorkMode
  } = props;

  let alternateTitle = null;
  let tag = null;
  const elementStyle = {
    width: '80px'
  };
  if (match.key === 'alternateTitles.title') {
    alternateTitle = alternateTitles[match.refIndex];
  } else if (match.key === 'tags.label') {
    tag = tags[match.refIndex];
  }

  if (itemType === 'movie') {
    return (
      <div className={styles.result}>
        <MoviePoster
          className={styles.poster}
          images={images}
          blur={safeForWorkMode}
          size={250}
          lazy={false}
          overflow={true}
        />

        <div className={styles.titles}>
          <div className={styles.title}>
            {title} {year > 0 ? `(${year})` : ''}
          </div>

          {
            alternateTitle ?
              <div className={styles.alternateTitle}>
                {alternateTitle.title}
              </div> :
              null
          }

          {itemType !== null &&
            <div className={styles.alternateTitle}>
              <Label
                children={translate('Movie')}
                kind={kinds.PRIMARY}
              />
            </div>
          }

          {
            match.key === 'tmdbId' && tmdbId ?
              <div className={styles.alternateTitle}>
                TmdbId: {tmdbId}
              </div> :
              null
          }

          {
            match.key === 'imdbId' && imdbId ?
              <div className={styles.alternateTitle}>
                ImdbId: {imdbId}
              </div> :
              null
          }

          {
            tag ?
              <div className={styles.tagContainer}>
                <Label
                  key={tag.id}
                  kind={kinds.INFO}
                >
                  {tag.label}
                </Label>
              </div> :
              null
          }
        </div>
      </div>
    );
  }
  if (itemType === 'scene') {
    return (
      <div className={styles.result}>
        <ScenePoster
          blur={safeForWorkMode}
          className={styles.poster}
          style={elementStyle}
          images={images}
          size={250}
          lazy={false}
          overflow={true}
        />

        <div className={styles.titles}>
          <div className={styles.titleContainer}>
            <div className={styles.title}
              title={title}
              children={title}
            />
          </div>

          <div className={styles.studioTitle}
            title={translate('Studio')}
            children={studioTitle}
          />

          {itemType !== null &&
            <div className={styles.itemTypeConatiner}>
              <Label className={styles.itemType}
                children={translate('Scene')}
                kind={kinds.SUCCESS}
                title={translate('Type')}
              />
            </div>
          }

          <RelativeDateCellConnector
            key={'releaseDare'}
            className={styles.releaseDate}
            date={releaseDate}
          />

        </div>
      </div>
    );
  }
}
MovieSearchResult.propTypes = {
  title: PropTypes.string.isRequired,
  year: PropTypes.number.isRequired,
  images: PropTypes.arrayOf(PropTypes.object).isRequired,
  alternateTitles: PropTypes.arrayOf(PropTypes.object).isRequired,
  tmdbId: PropTypes.number,
  imdbId: PropTypes.string,
  stashId: PropTypes.string,
  itemType: PropTypes.string,
  releaseDate: PropTypes.string,
  studioTitle: PropTypes.string,
  tags: PropTypes.arrayOf(PropTypes.object).isRequired,
  match: PropTypes.object.isRequired,
  safeForWorkMode: PropTypes.bool
};

export default MovieSearchResult;
