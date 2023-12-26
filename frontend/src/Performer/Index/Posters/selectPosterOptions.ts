import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';

const selectPosterOptions = createSelector(
  (state: AppState) => state.performers.posterOptions,
  (posterOptions) => posterOptions
);

export default selectPosterOptions;
