import React, { useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import Button from 'Components/Link/Button';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { scrollDirections } from 'Helpers/Props';
import InteractiveSearch from 'InteractiveSearch/InteractiveSearch';
import Movie from 'Movie/Movie';
import useMovie from 'Movie/useMovie';
import { clearMovieBlocklist } from 'Store/Actions/movieBlocklistActions';
import { clearMovieHistory } from 'Store/Actions/movieHistoryActions';
import {
  cancelFetchReleases,
  clearReleases,
} from 'Store/Actions/releaseActions';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import getRelativeDate from 'Utilities/Date/getRelativeDate';
import translate from 'Utilities/String/translate';

export interface MovieInteractiveSearchModalContentProps {
  movieId: number;
  onModalClose(): void;
}

function MovieInteractiveSearchModalContent({
  movieId,
  onModalClose,
}: MovieInteractiveSearchModalContentProps) {
  const dispatch = useDispatch();

  const { title, releaseDate } = useMovie(movieId) as Movie;

  useEffect(() => {
    return () => {
      dispatch(cancelFetchReleases());
      dispatch(clearReleases());

      dispatch(clearMovieBlocklist());
      dispatch(clearMovieHistory());
    };
  }, [dispatch]);

  const { showRelativeDates, shortDateFormat } = useSelector(
    createUISettingsSelector()
  );
  const date = getRelativeDate({
    date: releaseDate,
    shortDateFormat,
    showRelativeDates,
  });
  const movieTitle = `${title}${date ? ` (${date})` : ''}`;

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {movieTitle
          ? translate('InteractiveSearchModalHeaderTitle', {
              title: movieTitle,
            })
          : translate('InteractiveSearchModalHeader')}
      </ModalHeader>

      <ModalBody scrollDirection={scrollDirections.BOTH}>
        <InteractiveSearch searchPayload={{ movieId }} />
      </ModalBody>

      <ModalFooter>
        <Button onPress={onModalClose}>{translate('Close')}</Button>
      </ModalFooter>
    </ModalContent>
  );
}

export default MovieInteractiveSearchModalContent;
