
import { icons } from 'Helpers/Props';
import translate from 'Utilities/String/translate';

export function getSeriesStatusDetails(status) {

  let statusDetails = {
    icon: icons.SERIES_CONTINUING,
    title: translate('Continuing'),
    message: translate('ContinuingSiteDescription')
  };

  if (status === 'deleted') {
    statusDetails = {
      icon: icons.SERIES_DELETED,
      title: translate('Deleted'),
      message: translate('DeletedSiteDescription')
    };
  } else if (status === 'ended') {
    statusDetails = {
      icon: icons.SERIES_ENDED,
      title: translate('Ended'),
      message: translate('EndedSiteDescription')
    };
  } else if (status === 'upcoming') {
    statusDetails = {
      icon: icons.SERIES_CONTINUING,
      title: translate('Upcoming'),
      message: translate('UpcomingSiteDescription')
    };
  }

  return statusDetails;
}
