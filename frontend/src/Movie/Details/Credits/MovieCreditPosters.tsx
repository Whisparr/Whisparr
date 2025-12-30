import React, { useCallback, useMemo } from 'react';
import { Navigation, type Swiper as SwiperType } from 'swiper';
import { Swiper, SwiperSlide } from 'swiper/react';
import Performer from 'Performer/Performer';
import dimensions from 'Styles/Variables/dimensions';
import MovieCreditPosterConnector from './MovieCreditPosterConnector';
import styles from './MovieCreditPosters.css';

import 'swiper/css';
import 'swiper/css/navigation';

const columnPadding = parseFloat(dimensions.movieIndexColumnPadding as string);
const columnPaddingSmallScreen = parseFloat(
  dimensions.movieIndexColumnPaddingSmallScreen as string
);

function calculateRowHeight(posterHeight: number, isSmallScreen: boolean) {
  const titleHeight = 19;
  const characterHeight = 19;

  const heights = [
    posterHeight,
    titleHeight,
    characterHeight,
    isSmallScreen ? columnPaddingSmallScreen : columnPadding,
  ];

  return heights.reduce((acc, height) => acc + height, 0);
}

interface Credit {
  creditForeignId?: string;
  performer: Performer;
  job?: string;
}

interface Props {
  items: Credit[];
  itemComponent: React.ElementType;
  isSmallScreen: boolean;
}

function MovieCreditPosters({ items, itemComponent, isSmallScreen }: Props) {
  const { posterWidth, posterHeight, rowHeight } = useMemo(() => {
    const posterWidth = 162;
    const posterHeight = 238;
    const rowHeight = calculateRowHeight(posterHeight, isSmallScreen);

    return { posterWidth, posterHeight, rowHeight };
  }, [isSmallScreen]);

  const handleInit = useCallback((swiper: SwiperType) => {
    swiper.navigation.init();
    swiper.navigation.update();
  }, []);

  return (
    <div className={styles.sliderContainer}>
      <Swiper
        slidesPerView="auto"
        spaceBetween={10}
        slidesPerGroup={isSmallScreen ? 1 : 3}
        navigation={true}
        loop={false}
        loopFillGroupWithBlank={true}
        className="mySwiper"
        modules={[Navigation]}
        onInit={handleInit}
      >
        {items.map((credit, index) => {
          const slideKey =
            credit.creditForeignId ||
            `${credit.performer?.foreignId}-${credit.job || 'job'}-${index}`;

          return (
            <SwiperSlide
              key={slideKey}
              style={{ width: posterWidth, height: rowHeight }}
            >
              <MovieCreditPosterConnector
                component={itemComponent}
                posterWidth={posterWidth}
                posterHeight={posterHeight}
                performerForeignId={credit.performer.foreignId}
                performer={credit.performer}
                job={credit.job}
                character={credit.performer.fullName}
              />
            </SwiperSlide>
          );
        })}
      </Swiper>
    </div>
  );
}

export default MovieCreditPosters;
