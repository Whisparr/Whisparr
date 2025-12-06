import moment from 'moment';
import translate from 'Utilities/String/translate';
import formatDateTime from './formatDateTime';

interface GetRelativeDateOptions {
  date?: string;
  shortDateFormat: string;
  showRelativeDates: boolean;
  timeFormat?: string;
  includeSeconds?: boolean;
  timeForToday?: boolean;
  includeTime?: boolean;
  ignoreTimezone?: boolean;
}

function getRelativeDate({
  date,
  shortDateFormat,
  showRelativeDates,
  timeFormat,
  includeSeconds = false,
  timeForToday = false,
  includeTime = false,
  ignoreTimezone = false,
}: GetRelativeDateOptions) {
  if (date == null || date === '') {
    return '';
  }

  if (
    (includeTime || timeForToday) &&
    (timeFormat == null || timeFormat === '')
  ) {
    throw new Error(
      "getRelativeDate: 'timeFormat' is required when 'includeTime' or 'timeForToday' is true"
    );
  }
  // Detect date-only strings (YYYY-MM-DD) or midnight-UTC timestamps
  const isDateOnly = /^\d{4}-\d{2}-\d{2}$/.test(date || '');
  const isMidnightUtc = /T00:00:00(?:\.000)?Z$/.test(date || '');

  const useUtcCalendar = Boolean(ignoreTimezone || isDateOnly || isMidnightUtc);

  // Parse date and reference 'now' in the same mode (UTC or local)
  const m = useUtcCalendar ? moment.utc(date) : moment(date);
  const now = useUtcCalendar ? moment.utc() : moment();

  // Small local time formatter that mirrors Utilities/Date/formatTime behavior
  const time = timeFormat
    ? (() => {
        let tf = timeFormat;
        const t = m.clone();

        if (includeSeconds) {
          tf = tf.replace(/\(?:mm\)?/, ':mm:ss');
        } else if (t.minute() === 0) {
          tf = tf.replace('(:mm)', '');
        } else {
          tf = tf.replace('(:mm)', ':mm');
        }

        return t.format(tf);
      })()
    : '';

  const isTodayDate = m.isSame(now, 'day');

  if (isTodayDate && timeForToday) {
    return time;
  }

  if (showRelativeDates === false) {
    return m.format(shortDateFormat);
  }

  const isYesterdayDate = m.isSame(now.clone().subtract(1, 'day'), 'day');
  if (isYesterdayDate) {
    return includeTime
      ? translate('YesterdayAt', { time })
      : translate('Yesterday');
  }

  if (isTodayDate) {
    return includeTime ? translate('TodayAt', { time }) : translate('Today');
  }

  const isTomorrowDate = m.isSame(now.clone().add(1, 'day'), 'day');
  if (isTomorrowDate) {
    return includeTime
      ? translate('TomorrowAt', { time })
      : translate('Tomorrow');
  }

  const diffDays = m.startOf('day').diff(now.startOf('day'), 'days');
  if (diffDays > 0 && diffDays <= 7) {
    const day = m.format('dddd');
    return includeTime ? translate('DayOfWeekAt', { day, time }) : day;
  }

  return includeTime
    ? formatDateTime(date, shortDateFormat, timeFormat, { includeSeconds })
    : m.format(shortDateFormat);
}

export default getRelativeDate;
