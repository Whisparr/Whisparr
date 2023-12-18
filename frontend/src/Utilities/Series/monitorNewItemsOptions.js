import translate from 'Utilities/String/translate';

const monitorNewItemsOptions = [
  {
    key: 'all',
    get value() {
      return translate('MonitorAllScenes');
    }
  },
  {
    key: 'none',
    get value() {
      return translate('MonitorNone');
    }
  }
];

export default monitorNewItemsOptions;
