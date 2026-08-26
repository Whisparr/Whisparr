import React from 'react';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import SettingsToolbar from 'Settings/SettingsToolbar';
import translate from 'Utilities/String/translate';
import Tpdb from './Tpdb';

function MetadataSourceSettings() {
  return (
    <PageContent title={translate('MetadataSourceSettings')}>
      <SettingsToolbar showSave={false} />

      <PageContentBody>
        <Tpdb />
      </PageContentBody>
    </PageContent>
  );
}

export default MetadataSourceSettings;
