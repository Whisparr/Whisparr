import React from 'react';
import translate from 'Utilities/String/translate';
import InlineMarkdown from '../../Components/Markdown/InlineMarkdown';
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
          {translate('TheTpdb')}
        </div>

        <InlineMarkdown data={translate('SiteAndEpisodeInformationIsProvidedByTheTPDB')} />
      </div>

    </div>
  );
}

export default Tpdb;
