import BlocklistAppState from './BlocklistAppState';
import CalendarAppState from './CalendarAppState';
import CommandAppState from './CommandAppState';
import HistoryAppState, { MovieHistoryAppState } from './HistoryAppState';
import InteractiveImportAppState from './InteractiveImportAppState';
import MovieBlocklistAppState from './MovieBlocklistAppState';
import MovieCollectionAppState from './MovieCollectionAppState';
import MovieFilesAppState from './MovieFilesAppState';
import MoviesAppState, { MovieIndexAppState } from './MoviesAppState';
import MovieSearchAppState from './MovieSearchAppState';
import OrganizePreviewAppState from './OrganizePreviewAppState';
import ParseAppState from './ParseAppState';
import PerformersAppState from './PerformersAppState';
import QueueAppState from './QueueAppState';
import ReleasesAppState from './ReleasesAppState';
import RootFolderAppState from './RootFolderAppState';
import SettingsAppState from './SettingsAppState';
import StudiosAppState from './StudiosAppState';
import SystemAppState from './SystemAppState';
import TagsAppState from './TagsAppState';

interface FilterBuilderPropOption {
  id: string;
  name: string;
}

export interface FilterBuilderProp<T> {
  name: string;
  label: string;
  type: string;
  valueType?: string;
  optionsSelector?: (items: T[]) => FilterBuilderPropOption[];
}

export interface PropertyFilter {
  key: string;
  value: boolean | string | number | string[] | number[];
  type: string;
}

export interface Filter {
  key: string;
  label: string;
  filers: PropertyFilter[];
}

export interface CustomFilter {
  id: number;
  type: string;
  label: string;
  filers: PropertyFilter[];
}

export interface AppSectionState {
  isConnected: boolean;
  isReconnecting: boolean;
  version: string;
  prevVersion?: string;
  dimensions: {
    isSmallScreen: boolean;
    width: number;
    height: number;
  };
}

interface AppState {
  app: AppSectionState;
  blocklist: BlocklistAppState;
  calendar: CalendarAppState;
  commands: CommandAppState;
  history: HistoryAppState;
  interactiveImport: InteractiveImportAppState;
  movieBlocklist: MovieBlocklistAppState;
  movieCollections: MovieCollectionAppState;
  movieFiles: MovieFilesAppState;
  movieHistory: MovieHistoryAppState;
  movieIndex: MovieIndexAppState;
  movieSearch: MovieSearchAppState;
  sceneIndex: MovieIndexAppState;
  performers: PerformersAppState;
  studios: StudiosAppState;
  movies: MoviesAppState;
  organizePreview: OrganizePreviewAppState;
  parse: ParseAppState;
  queue: QueueAppState;
  releases: ReleasesAppState;
  rootFolders: RootFolderAppState;
  settings: SettingsAppState;
  system: SystemAppState;
  tags: TagsAppState;
}

export default AppState;
