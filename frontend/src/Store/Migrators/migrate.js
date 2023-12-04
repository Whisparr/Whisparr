import migrateBlacklistToBlocklist from './migrateBlacklistToBlocklist';
import migrateMonitorToEnum from './migrateMonitorToEnum';

export default function migrate(persistedState) {
  migrateBlacklistToBlocklist(persistedState);
  migrateMonitorToEnum(persistedState);
}
