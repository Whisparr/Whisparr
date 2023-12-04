import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { deleteMovie, setDeleteOption } from 'Store/Actions/movieActions';
import createMovieSelector from 'Store/Selectors/createMovieSelector';
import DeleteSceneModalContent from './DeleteSceneModalContent';

function createMapStateToProps() {
  return createSelector(
    (state) => state.movies.deleteOptions,
    createMovieSelector(),
    (deleteOptions, scene) => {
      return {
        ...scene,
        deleteOptions
      };
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    onDeleteOptionChange(option) {
      dispatch(
        setDeleteOption({
          [option.name]: option.value
        })
      );
    },

    onDeletePress(deleteFiles, addImportExclusion) {
      dispatch(
        deleteMovie({
          id: props.sceneId,
          collectionTmdbId: this.collection?.tmdbId,
          deleteFiles,
          addImportExclusion
        })
      );

      props.onModalClose(true);
    }
  };
}

export default connect(createMapStateToProps, createMapDispatchToProps)(DeleteSceneModalContent);
