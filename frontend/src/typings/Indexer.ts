import ModelBase from 'App/ModelBase';
import DownloadProtocol from 'DownloadClient/DownloadProtocol';

export interface Field {
  order: number;
  name: string;
  label: string;
  value: boolean | number | string;
  type: string;
  advanced: boolean;
  privacy: string;
}

interface Indexer extends ModelBase {
  enableRss: boolean;
  enableAutomaticSearch: boolean;
  enableInteractiveSearch: boolean;
  supportsRss: boolean;
  supportsSearch: boolean;
  protocol: DownloadProtocol;
  priority: number;
  name: string;
  fields: Field[];
  implementationName: string;
  implementation: string;
  configContract: string;
  infoLink: string;
  downloadClientId: number;
  tags: number[];
}

export default Indexer;
