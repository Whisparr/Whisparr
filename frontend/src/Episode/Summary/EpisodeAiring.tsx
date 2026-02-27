import moment from 'moment';
import React from 'react';
import { useSelector } from 'react-redux';
import Label from 'Components/Label';
import { kinds, sizes } from 'Helpers/Props';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import isInNextWeek from 'Utilities/Date/isInNextWeek';
import isToday from 'Utilities/Date/isToday';
import isTomorrow from 'Utilities/Date/isTomorrow';
import translate from 'Utilities/String/translate';

interface EpisodeAiringProps {
  releaseDate?: string;
  network: string;
}

function EpisodeAiring(props: EpisodeAiringProps) {
  const { releaseDate, network } = props;

  const { shortDateFormat, showRelativeDates } = useSelector(
    createUISettingsSelector()
  );

  const networkLabel = (
    <Label kind={kinds.INFO} size={sizes.MEDIUM}>
      {network}
    </Label>
  );

  if (!releaseDate) {
    return (
      <span>
        {translate('AirsTbaOn', { networkLabel: '' })}
        {networkLabel}
      </span>
    );
  }

  if (!showRelativeDates) {
    return (
      <span>
        {translate('AirsDateAtTimeOn', {
          date: moment(releaseDate).format(shortDateFormat),
          networkLabel: '',
        })}
        {networkLabel}
      </span>
    );
  }

  if (isToday(releaseDate)) {
    return (
      <span>
        {translate('AirsTimeOn', { networkLabel: '' })}
        {networkLabel}
      </span>
    );
  }

  if (isTomorrow(releaseDate)) {
    return (
      <span>
        {translate('AirsTomorrowOn', { networkLabel: '' })}
        {networkLabel}
      </span>
    );
  }

  if (isInNextWeek(releaseDate)) {
    return (
      <span>
        {translate('AirsDateAtTimeOn', {
          date: moment(releaseDate).format('dddd'),
          networkLabel: '',
        })}
        {networkLabel}
      </span>
    );
  }

  return (
    <span>
      {translate('AirsDateAtTimeOn', {
        date: moment(releaseDate).format(shortDateFormat),
        networkLabel: '',
      })}
      {networkLabel}
    </span>
  );
}

export default EpisodeAiring;
