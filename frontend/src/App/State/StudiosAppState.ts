import SortDirection from 'Helpers/Props/SortDirection';
import Studio from 'Studio/Studio';
import AppSectionState, { AppSectionSaveState } from './AppSectionState';
import { Filter, FilterBuilderProp } from './AppState';

interface StudiosAppState extends AppSectionSaveState, AppSectionState<Studio> {
  sortKey: string;
  sortDirection: SortDirection;
  secondarySortKey: string;
  secondarySortDirection: SortDirection;
  view: string;

  posterOptions: {
    detailedProgressBar: boolean;
    size: string;
    showName: boolean;
  };

  selectedFilterKey: string;
  filterBuilderProps: FilterBuilderProp<Studio>[];
  filters: Filter[];
}

export default StudiosAppState;
