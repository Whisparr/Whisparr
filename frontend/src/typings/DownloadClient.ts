import Provider from './Provider';

export type Protocol = 'torrent' | 'usenet' | 'unknown';

interface DownloadClient extends Provider {
  enable: boolean;
  protocol: Protocol;
  priority: number;
  removeCompletedDownloads: boolean;
  removeFailedDownloads: boolean;
  name: string;
  tags: number[];
}

export default DownloadClient;
