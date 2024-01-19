import _ from 'lodash';

export default function migrateMonitorToEnum(persistedState) {
  const addMovie = _.get(persistedState, 'addMovie.movieDefaults.monitor');
  const discoverMovie = _.get(persistedState, 'discoverMovie.movieDefaults.monitor');

  if (addMovie) {
    if (addMovie === 'true') {
      persistedState.addMovie.movieDefaults.monitor = 'movieOnly';
    }

    if (addMovie === 'false') {
      persistedState.addMovie.movieDefaults.monitor = 'none';
    }
  }

  if (discoverMovie) {
    if (discoverMovie === 'true') {
      persistedState.discoverMovie.movieDefaults.monitor = 'movieOnly';
    }

    if (discoverMovie === 'false') {
      persistedState.discoverMovie.movieDefaults.monitor = 'none';
    }
  }
}
