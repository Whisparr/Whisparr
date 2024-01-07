import PropTypes from 'prop-types';
import React from 'react';
import DescriptionList from 'Components/DescriptionList/DescriptionList';
import DescriptionListItem from 'Components/DescriptionList/DescriptionListItem';
import formatBytes from 'Utilities/Number/formatBytes';
import translate from 'Utilities/String/translate';
import styles from './StudioInfo.css';

function StudioInfo(props) {
  const {
    totalMovieCount,
    monitoredMovieCount,
    movieFileCount,
    sizeOnDisk
  } = props;

  return (
    <DescriptionList>
      <DescriptionListItem
        titleClassName={styles.title}
        descriptionClassName={styles.description}
        title={translate('Total')}
        data={totalMovieCount}
      />

      <DescriptionListItem
        titleClassName={styles.title}
        descriptionClassName={styles.description}
        title={translate('Monitored')}
        data={monitoredMovieCount}
      />

      <DescriptionListItem
        titleClassName={styles.title}
        descriptionClassName={styles.description}
        title={translate('WithFiles')}
        data={movieFileCount}
      />

      <DescriptionListItem
        titleClassName={styles.title}
        descriptionClassName={styles.description}
        title={translate('SizeOnDisk')}
        data={formatBytes(sizeOnDisk)}
      />
    </DescriptionList>
  );
}

StudioInfo.propTypes = {
  totalMovieCount: PropTypes.number.isRequired,
  monitoredMovieCount: PropTypes.number.isRequired,
  movieFileCount: PropTypes.number.isRequired,
  sizeOnDisk: PropTypes.number.isRequired
};

export default StudioInfo;
