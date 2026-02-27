import React from 'react';
import { SeriesType } from 'Series/Series';
import EpisodeNumber, { EpisodeNumberProps } from './EpisodeNumber';

interface SeasonEpisodeNumberProps extends EpisodeNumberProps {
  seriesType?: SeriesType;
}

function SeasonEpisodeNumber(props: SeasonEpisodeNumberProps) {
  const { seriesType, ...otherProps } = props;

  // Note: Whisparr doesn't have daily series type

  return (
    <EpisodeNumber
      seriesType={seriesType}
      showSeasonNumber={true}
      {...otherProps}
    />
  );
}

export default SeasonEpisodeNumber;
