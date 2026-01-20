import React from 'react';
import DescriptionList from 'Components/DescriptionList/DescriptionList';
import DescriptionListItem from 'Components/DescriptionList/DescriptionListItem';
import translate from 'Utilities/String/translate';

function SeriesTypePopoverContent() {
  return (
    <DescriptionList>
      <DescriptionListItem
        title={translate('Standard')}
        data={translate('StandardTypeDescription')}
      />

      <DescriptionListItem
        title={translate('Jav')}
        data={translate('JavTypeDescription')}
      />
    </DescriptionList>
  );
}

export default SeriesTypePopoverContent;
