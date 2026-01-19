import translate from 'Utilities/String/translate';

const seriesTypeOptions = [
  {
    key: 'standard',
    get value() {
      return translate('Standard');
    }
  },
  {
    key: 'jav',
    get value() {
      return translate('Jav');
    }
  }
];

export default seriesTypeOptions;
