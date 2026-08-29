import LogFile from 'typings/LogFile';
import Task from 'typings/Task';
import Update from 'typings/Update';
import AppSectionState from './AppSectionState';
import BackupAppState from './BackupAppState';
import LogsAppState from './LogsAppState';

export type TaskAppState = AppSectionState<Task>;
export type LogFilesAppState = AppSectionState<LogFile>;
export type UpdateAppState = AppSectionState<Update>;

interface SystemAppState {
  backups: BackupAppState;
  logFiles: LogFilesAppState;
  logs: LogsAppState;
  tasks: TaskAppState;
  updateLogFiles: LogFilesAppState;
  updates: UpdateAppState;
}

export default SystemAppState;
