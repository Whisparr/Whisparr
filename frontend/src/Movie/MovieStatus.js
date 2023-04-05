import { icons } from 'Helpers/Props';
import translate from 'Utilities/String/translate';

export function getMovieStatusDetails(status) {

  let statusDetails = {
    icon: icons.ANNOUNCED,
    title: translate('Upcoming'),
    message: translate('UpcomingMsg')
  };

  if (status === 'deleted') {
    statusDetails = {
      icon: icons.SERIES_DELETED,
      title: translate('Deleted'),
      message: translate('DeletedMsg')
    };
  } else if (status === 'released') {
    statusDetails = {
      icon: icons.EPISODE_FILE,
      title: translate('Released'),
      message: translate('ReleasedMsg')
    };
  }

  return statusDetails;
}
