import { kinds } from 'Helpers/Props';
import type { Kind } from 'Helpers/Props/kinds';

export function getStatusStyle(
  monitored: boolean,
  hasFile: boolean,
  isAvailable: boolean,
  queue = false
): Kind {
  if (queue) return kinds.QUEUE;
  if (hasFile && monitored) return kinds.SUCCESS;
  if (hasFile && !monitored) return kinds.DEFAULT;
  if (isAvailable && monitored) return kinds.DANGER;
  if (!monitored) return kinds.WARNING;
  return kinds.PRIMARY;
}

export default getStatusStyle;
