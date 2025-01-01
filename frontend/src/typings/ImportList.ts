import ModelBase from 'App/ModelBase';
import MovieMonitor from 'Movie/Movie';

interface ImportList extends ModelBase {
  enable: boolean;
  enabled: boolean;
  enableAuto: boolean;
  qualityProfileId: number;
  rootFolderPath: string;
  monitor: MovieMonitor;
  searchOnAdd: boolean;
  listType: string;
  listOrder: number;
  minRefreshInterval: string;
  name: string;
  tags: number[];
}

export default ImportList;
