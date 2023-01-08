import React from 'react';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import SettingsToolbarConnector from 'Settings/SettingsToolbarConnector';
import Tpdb from './Tpdb';

function MetadataSourceSettings() {
  return (
    <PageContent title="Metadata Source Settings">
      <SettingsToolbarConnector
        showSave={false}
      />

      <PageContentBody>
        <Tpdb />
      </PageContentBody>
    </PageContent>
  );
}

export default MetadataSourceSettings;
