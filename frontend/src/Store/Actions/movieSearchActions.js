import { createThunk, handleThunks } from 'Store/thunks';
import createAjaxRequest from 'Utilities/createAjaxRequest';
import { set, update } from './baseActions';
import createHandleActions from './Creators/createHandleActions';

export const section = 'movieSearch';

export const defaultState = {
  isFetching: false,
  isPopulated: false,
  error: null,
  items: []
};

export const SEARCH_MOVIES_MODAL = 'movieSearch/searchMovies';
export const searchMoviesModal = createThunk(SEARCH_MOVIES_MODAL);

export const actionHandlers = handleThunks({
  [SEARCH_MOVIES_MODAL]: (getState, payload, dispatch) => {
    if (getState().movieSearch.isFetching) {
      return;
    }

    dispatch(set({ section, isFetching: true }));

    const { request, abortRequest } = createAjaxRequest({
      url: '/movie/search',
      data: { query: payload },
      traditional: true
    });

    request
      .done((data) => {
        dispatch(update({ section, data }));
        dispatch(set({ section, isFetching: false, isPopulated: true, error: null }));
      })
      .fail((xhr) => {
        dispatch(set({ section, isFetching: false, isPopulated: false, error: xhr }));
      });

    return abortRequest;
  }
});

export const reducers = createHandleActions({}, defaultState, section);
