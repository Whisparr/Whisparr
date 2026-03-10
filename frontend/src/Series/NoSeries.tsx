import React from 'react';
import Button from 'Components/Link/Button';
import { kinds } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import styles from './NoSeries.css';

interface NoSeriesProps {
  totalItems: number;
  seriesType?: string;
}

function NoSeries(props: NoSeriesProps) {
  const { totalItems, seriesType } = props;

  const addNewPath = seriesType === 'jav' ? '/jav/add/new' : '/add/new';

  if (totalItems > 0) {
    return (
      <div>
        <div className={styles.message}>
          {translate('AllSitesAreHiddenByTheAppliedFilter')}
        </div>
      </div>
    );
  }

  return (
    <div>
      <div className={styles.message}>
        {translate('NoSitesFoundImportOrAdd')}
      </div>

      {seriesType !== 'jav' && (
        <div className={styles.buttonContainer}>
          <Button to="/add/import" kind={kinds.PRIMARY}>
            {translate('ImportExistingSites')}
          </Button>
        </div>
      )}

      <div className={styles.buttonContainer}>
        <Button to={addNewPath} kind={kinds.PRIMARY}>
          {translate('AddNewSite')}
        </Button>
      </div>
    </div>
  );
}

export default NoSeries;
