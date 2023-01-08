import React from 'react';
import Link from 'Components/Link/Link';
import styles from './Tpdb.css';

function Tpdb(props) {
  return (
    <div className={styles.container}>
      <img
        className={styles.image}
        src={`${window.Whisparr.urlBase}/Content/Images/Tpdb.png`}
      />

      <div className={styles.info}>
        <div className={styles.title}>
          TPDB
        </div>

        <div>
          Site and Release information is provided by metadataapi.net. <Link to="https://www.patreon.com/metadataapi">Please consider supporting them.</Link>
        </div>
      </div>

    </div>
  );
}

export default Tpdb;
