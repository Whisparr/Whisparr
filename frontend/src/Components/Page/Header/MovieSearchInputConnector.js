import { push } from 'connected-react-router';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createAllItemsSelector from 'Store/Selectors/createAllItemsSelector';
import createDeepEqualSelector from 'Store/Selectors/createDeepEqualSelector';
import createTagsSelector from 'Store/Selectors/createTagsSelector';
import MovieSearchInput from './MovieSearchInput';

function createCleanMovieSelector() {
  return createSelector(
    createAllItemsSelector(),
    createTagsSelector(),
    (state) => state.settings.safeForWorkMode,
    (allMovies, allTags, safeForWorkMode) => {
      return allMovies.map((movie) => {
        const {
          title,
          titleSlug,
          sortTitle,
          year,
          images,
          alternateTitles = [],
          tmdbId,
          imdbId,
          stashId,
          studioTitle,
          releaseDate,
          itemType,
          tags = []
        } = movie;

        return {
          title,
          titleSlug,
          sortTitle,
          year,
          images,
          alternateTitles,
          tmdbId,
          imdbId,
          stashId,
          studioTitle,
          releaseDate,
          itemType,
          safeForWorkMode,
          firstCharacter: title.charAt(0).toLowerCase(),
          tags: tags.reduce((acc, id) => {
            const matchingTag = allTags.find((tag) => tag.id === id);

            if (matchingTag) {
              acc.push(matchingTag);
            }

            return acc;
          }, [])
        };
      });
    }
  );
}

function createMapStateToProps() {
  return createDeepEqualSelector(
    createCleanMovieSelector(),
    (movies) => {
      return {
        movies
      };
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    onGoToMovie(titleSlug) {
      dispatch(push(`${window.Whisparr.urlBase}/movie/${titleSlug}`));
    },

    onGoToAddNewMovie(query) {
      dispatch(push(`${window.Whisparr.urlBase}/add/new/movie?term=${encodeURIComponent(query)}`));
    },

    onGoToAddNewScene(query) {
      dispatch(push(`${window.Whisparr.urlBase}/add/new/scene?term=${encodeURIComponent(query)}`));
    }
  };
}

export default connect(createMapStateToProps, createMapDispatchToProps)(MovieSearchInput);
