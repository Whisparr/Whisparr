import moment from 'moment';
import PropTypes from 'prop-types';
import React from 'react';
import Label from 'Components/Label';
import { kinds, sizes } from 'Helpers/Props';
import isInNextWeek from 'Utilities/Date/isInNextWeek';
import isToday from 'Utilities/Date/isToday';
import isTomorrow from 'Utilities/Date/isTomorrow';
import translate from 'Utilities/String/translate';

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
        {translate('AirsTbaOn', { networkLabel: '' })}{networkLabel}
      </span>
    );
  }

  if (!showRelativeDates) {
    return (
      <span>
        {translate('AirsDateAtTimeOn', { date: moment(releaseDate).format(shortDateFormat), networkLabel: '' })}{networkLabel}
      </span>
    );
  }

  if (isToday(releaseDate)) {
    return (
      <span>
        {translate('AirsTimeOn', { networkLabel: '' })}{networkLabel}
      </span>
    );
  }

  if (isTomorrow(releaseDate)) {
    return (
      <span>
        {translate('AirsTomorrowOn', { networkLabel: '' })}{networkLabel}
      </span>
    );
  }

  if (isInNextWeek(releaseDate)) {
    return (
      <span>
        {translate('AirsDateAtTimeOn', { date: moment(releaseDate).format('dddd'), networkLabel: '' })}{networkLabel}
      </span>
    );
  }

  return (
    <span>
      {translate('AirsDateAtTimeOn', { date: moment(releaseDate).format(shortDateFormat), networkLabel: '' })}{networkLabel}
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
