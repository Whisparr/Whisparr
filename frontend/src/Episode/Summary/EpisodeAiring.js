import moment from 'moment';
import PropTypes from 'prop-types';
import React from 'react';
import Label from 'Components/Label';
import { kinds, sizes } from 'Helpers/Props';
import isInNextWeek from 'Utilities/Date/isInNextWeek';
import isToday from 'Utilities/Date/isToday';
import isTomorrow from 'Utilities/Date/isTomorrow';

function EpisodeAiring(props) {
  const {
    releaseDate,
    network,
    shortDateFormat,
    showRelativeDates
  } = props;

  const networkLabel = (
    <Label
      kind={kinds.INFO}
      size={sizes.MEDIUM}
    >
      {network}
    </Label>
  );

  if (!releaseDate) {
    return (
      <span>
        TBA on {networkLabel}
      </span>
    );
  }

  if (!showRelativeDates) {
    return (
      <span>
        {moment(releaseDate).format(shortDateFormat)} on {networkLabel}
      </span>
    );
  }

  if (isToday(releaseDate)) {
    return (
      <span>
        Today on {networkLabel}
      </span>
    );
  }

  if (isTomorrow(releaseDate)) {
    return (
      <span>
        Tomorrow on {networkLabel}
      </span>
    );
  }

  if (isInNextWeek(releaseDate)) {
    return (
      <span>
        {moment(releaseDate).format('dddd')} on {networkLabel}
      </span>
    );
  }

  return (
    <span>
      {moment(releaseDate).format(shortDateFormat)} on {networkLabel}
    </span>
  );
}

EpisodeAiring.propTypes = {
  releaseDate: PropTypes.string.isRequired,
  network: PropTypes.string.isRequired,
  shortDateFormat: PropTypes.string.isRequired,
  showRelativeDates: PropTypes.bool.isRequired
};

export default EpisodeAiring;
