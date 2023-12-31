import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { clearAddMovie, lookupScene } from 'Store/Actions/addMovieActions';
import { clearQueueDetails, fetchQueueDetails } from 'Store/Actions/queueActions';
import { fetchRootFolders } from 'Store/Actions/rootFolderActions';
import { fetchImportExclusions } from 'Store/Actions/Settings/importExclusions';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import parseUrl from 'Utilities/String/parseUrl';
import AddNewScene from './AddNewScene';

function createMapStateToProps() {
  return createSelector(
    (state) => state.addMovie,
    (state) => state.movies.items.length,
    (state) => state.router.location,
    createUISettingsSelector(),
    (addMovie, existingMoviesCount, location, uiSettings) => {
      const { params } = parseUrl(location.search);

      return {
        ...addMovie,
        term: params.term,
        hasExistingMovies: existingMoviesCount > 0,
        colorImpairedMode: uiSettings.enableColorImpairedMode
      };
    }
  );
}

const mapDispatchToProps = {
  lookupScene,
  clearAddMovie,
  fetchRootFolders,
  fetchImportExclusions,
  fetchQueueDetails,
  clearQueueDetails
};

class AddNewSceneConnector extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this._movieLookupTimeout = null;
  }

  componentDidMount() {
    this.props.fetchRootFolders();
    this.props.fetchImportExclusions();
    this.props.fetchQueueDetails();
  }

  componentWillUnmount() {
    if (this._movieLookupTimeout) {
      clearTimeout(this._movieLookupTimeout);
    }

    this.props.clearAddMovie();
    this.props.clearQueueDetails();
  }

  //
  // Listeners

  onMovieLookupChange = (term) => {
    if (this._movieLookupTimeout) {
      clearTimeout(this._movieLookupTimeout);
    }

    if (term.trim() === '') {
      this.props.clearAddMovie();
    } else {
      this._movieLookupTimeout = setTimeout(() => {
        this.props.lookupScene({ term });
      }, 300);
    }
  };

  onClearMovieLookup = () => {
    this.props.clearAddMovie();
  };

  //
  // Render

  render() {
    const {
      term,
      ...otherProps
    } = this.props;

    return (
      <AddNewScene
        term={term}
        {...otherProps}
        onMovieLookupChange={this.onMovieLookupChange}
        onClearMovieLookup={this.onClearMovieLookup}
      />
    );
  }
}

AddNewSceneConnector.propTypes = {
  term: PropTypes.string,
  lookupScene: PropTypes.func.isRequired,
  clearAddMovie: PropTypes.func.isRequired,
  fetchRootFolders: PropTypes.func.isRequired,
  fetchImportExclusions: PropTypes.func.isRequired,
  fetchQueueDetails: PropTypes.func.isRequired,
  clearQueueDetails: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(AddNewSceneConnector);
