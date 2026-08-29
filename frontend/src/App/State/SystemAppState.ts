import Update from 'typings/Update';
import AppSectionState from './AppSectionState';
import BackupAppState from './BackupAppState';
import LogsAppState from './LogsAppState';

export type UpdateAppState = AppSectionState<Update>;

interface SystemAppState {
  backups: BackupAppState;
  logs: LogsAppState;
  updates: UpdateAppState;
}

export default SystemAppState;
