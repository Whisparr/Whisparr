import moment from 'moment';
import PropTypes from 'prop-types';
import React from 'react';
import AgendaEventConnector from './AgendaEventConnector';
import styles from './Agenda.css';

function Agenda(props) {
  const {
    items,
    start,
    end
  } = props;

  const startDateParsed = Date.parse(start);
  const endDateParsed = Date.parse(end);

  items.forEach((item) => {
    const releaseDateParsed = Date.parse(item.releaseDate);
    const dates = [];

    if (releaseDateParsed > 0 && releaseDateParsed >= startDateParsed && releaseDateParsed <= endDateParsed) {
      dates.push(releaseDateParsed);
    }

    item.sortDate = Math.min(...dates);
    item.releaseDateParsed = releaseDateParsed;
  });

  items.sort((a, b) => ((a.sortDate > b.sortDate) ? 1 : -1));

  return (
    <div className={styles.agenda}>
      {
        items.map((item, index) => {
          const momentDate = moment(item.sortDate);
          const showDate = index === 0 ||
            !moment(items[index - 1].sortDate).isSame(momentDate, 'day');

          return (
            <AgendaEventConnector
              key={item.id}
              movieId={item.id}
              showDate={showDate}
              {...item}
            />
          );
        })
      }
    </div>
  );
}

Agenda.propTypes = {
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  start: PropTypes.string.isRequired,
  end: PropTypes.string.isRequired
};

export default Agenda;
