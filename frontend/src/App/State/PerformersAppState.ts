import SortDirection from 'Helpers/Props/SortDirection';
import Performer from 'Performer/Performer';
import AppSectionState, { AppSectionSaveState } from './AppSectionState';
import { Filter, FilterBuilderProp } from './AppState';

interface PerformersAppState
  extends AppSectionSaveState,
    AppSectionState<Performer> {
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
  filterBuilderProps: FilterBuilderProp<Performer>[];
  filters: Filter[];
}

export default PerformersAppState;
