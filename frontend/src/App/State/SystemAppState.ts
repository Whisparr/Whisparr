import DiskSpace from 'typings/DiskSpace';
import Health from 'typings/Health';
import SystemStatus from 'typings/SystemStatus';
import Task from 'typings/Task';
import Update from 'typings/Update';
import AppSectionState, { AppSectionItemState } from './AppSectionState';
import BackupAppState from './BackupAppState';

export type DiskSpaceAppState = AppSectionState<DiskSpace>;
export type HealthAppState = AppSectionState<Health>;
export type SystemStatusAppState = AppSectionItemState<SystemStatus>;
export type UpdateAppState = AppSectionState<Update>;
export type TaskAppState = AppSectionState<Task>;

interface SystemAppState {
  backups: BackupAppState;
  diskSpace: DiskSpaceAppState;
  health: HealthAppState;
  updates: UpdateAppState;
  status: SystemStatusAppState;
  tasks: TaskAppState;
}

export default SystemAppState;
