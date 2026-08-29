import Update from 'typings/Update';
import AppSectionState from './AppSectionState';
import LogsAppState from './LogsAppState';

export type UpdateAppState = AppSectionState<Update>;

interface SystemAppState {
  logs: LogsAppState;
  updates: UpdateAppState;
}

export default SystemAppState;
