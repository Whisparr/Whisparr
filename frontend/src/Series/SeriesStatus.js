
import { icons } from 'Helpers/Props';

export function getSeriesStatusDetails(status) {

  let statusDetails = {
    icon: icons.SERIES_CONTINUING,
    title: 'Continuing',
    message: 'More episodes are expected'
  };

  if (status === 'deleted') {
    statusDetails = {
      icon: icons.SERIES_DELETED,
      title: 'Deleted',
      message: 'Site was deleted from TheTPDB'
    };
  } else if (status === 'ended') {
    statusDetails = {
      icon: icons.SERIES_ENDED,
      title: 'Ended',
      message: 'No additional episodes are expected'
    };
  } else if (status === 'upcoming') {
    statusDetails = {
      icon: icons.SERIES_CONTINUING,
      title: 'Upcoming',
      message: 'Site has been announced but no exact air date yet'
    };
  }

  return statusDetails;
}
