import { icons } from 'Helpers/Props';
import translate from 'Utilities/String/translate';

export function getPerformerStatusDetails(status) {

  let statusDetails = {
    icon: icons.MOVIE_CONTINUING,
    title: translate('Active'),
    message: translate('ActivePerformerDescription')
  };

  if (status === 'inactive') {
    statusDetails = {
      icon: icons.SERIES_ENDED,
      title: translate('Inactive'),
      message: translate('InactivePerformerDescription')
    };
  }

  return statusDetails;
}
