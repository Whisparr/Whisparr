import React from 'react';
import { useSelector } from 'react-redux';
import Movie from 'Movie/Movie';
import Performer from 'Performer/Performer';
import { createMovieSelectorForHook } from 'Store/Selectors/createMovieSelector';
import MovieCreditPosters from '../MovieCreditPosters';
import MovieCastPoster from './MovieCastPoster';

interface Props {
  movieId: number;
  isSmallScreen: boolean;
}

interface Credit {
  creditForeignId?: string;
  performer: Performer;
  job?: string;
  type?: string;
}

function MovieCastPostersConnector({ movieId, isSmallScreen }: Props) {
  const movie = useSelector(createMovieSelectorForHook(movieId)) as
    | Movie
    | undefined;

  const cast = ((movie?.credits || []) as unknown[]).filter(
    (c) => (c as Credit).type === 'cast'
  ) as Credit[];

  return (
    <MovieCreditPosters
      items={cast}
      itemComponent={MovieCastPoster}
      isSmallScreen={isSmallScreen}
    />
  );
}

export default MovieCastPostersConnector;
