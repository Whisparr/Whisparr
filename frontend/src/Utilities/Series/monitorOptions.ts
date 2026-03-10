import translate from 'Utilities/String/translate';

const monitorOptions = [
  {
    key: 'all',
    get value() {
      return translate('MonitorAllScenes');
    },
  },
  {
    key: 'future',
    get value() {
      return translate('MonitorFutureScenes');
    },
  },
  {
    key: 'missing',
    get value() {
      return translate('MonitorMissingScenes');
    },
  },
  {
    key: 'existing',
    get value() {
      return translate('MonitorExistingScenes');
    },
  },
  {
    key: 'recent',
    get value() {
      return translate('MonitorRecentEpisodes');
    },
  },
  {
    key: 'pilot',
    get value() {
      return translate('MonitorPilotEpisode');
    },
  },
  {
    key: 'firstSeason',
    get value() {
      return translate('MonitorFirstYear');
    },
  },
  {
    key: 'latestSeason',
    get value() {
      return translate('MonitorLatestYear');
    },
  },
  {
    key: 'monitorSpecials',
    get value() {
      return translate('MonitorSpecialEpisodes');
    },
  },
  {
    key: 'unmonitorSpecials',
    get value() {
      return translate('UnmonitorSpecialEpisodes');
    },
  },
  {
    key: 'none',
    get value() {
      return translate('MonitorNone');
    },
  },
];

export default monitorOptions;
