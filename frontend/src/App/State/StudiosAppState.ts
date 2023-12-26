import SortDirection from 'Helpers/Props/SortDirection';
import Studio from 'Studio/Studio';
import AppSectionState from './AppSectionState';
import { Filter, FilterBuilderProp } from './AppState';

interface StudiosAppState extends AppSectionState<Studio> {
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
