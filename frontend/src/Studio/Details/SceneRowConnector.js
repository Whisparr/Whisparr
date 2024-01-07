/* eslint max-params: 0 */
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createMovieFileSelector from 'Store/Selectors/createMovieFileSelector';
import createMovieSelector from 'Store/Selectors/createMovieSelector';
import SceneRow from './SceneRow';

function createMapStateToProps() {
  return createSelector(
    createMovieSelector(),
    createMovieFileSelector(),
    (state) => state.settings.ui.item.movieRuntimeFormat,
    (movie = {}, movieFile, movieRuntimeFormat) => {
      return {
        movieMonitored: movie.monitored,
        itemType: movie.itemType,
        movieFilePath: movieFile ? movieFile.path : null,
        movieFileRelativePath: movieFile ? movieFile.relativePath : null,
        movieFileSize: movieFile ? movieFile.size : null,
        releaseGroup: movieFile ? movieFile.releaseGroup : null,
        customFormats: movieFile ? movieFile.customFormats : [],
        customFormatScore: movieFile ? movieFile.customFormatScore : 0,
        movieRuntimeFormat
      };
    }
  );
}
export default connect(createMapStateToProps)(SceneRow);
