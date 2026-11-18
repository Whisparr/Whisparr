import { MonitorNewItems, SeriesMonitor } from 'Series/Series';
import Provider from './Provider';

interface ImportList extends Provider {
  enable: boolean;
  enableAutomaticAdd: boolean;
  searchForMissingEpisodes: boolean;
  qualityProfileId: number;
  rootFolderPath: string;
  shouldMonitor: SeriesMonitor;
  monitorNewItems: MonitorNewItems;
  siteMonitorType: SeriesMonitor;
  listType: string;
  listOrder: number;
  minRefreshInterval: string;
  name: string;
  tags: number[];
}

export default ImportList;
