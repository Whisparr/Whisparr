import translate from 'Utilities/String/translate';

const monitorOptions = [
  {
    key: 'movieOnly',
    get value() {
      return translate('MovieOnly');
    }
  },
  {
    key: 'movieAndScene',
    get value() {
      return translate('MonitorOptionsMovieAndScene');
    }
  },
  {
    key: 'sceneOnly',
    get value() {
      return translate('MonitorOptionsSceneOnly');
    }
  },
  {
    key: 'none',
    get value() {
      return translate('None');
    }
  }
];

export default monitorOptions;
